using System.Collections.Generic;
using System.Linq;
using Bulb.Characters;
using Bulb.Characters.Wire;
using Bulb.Controllers;
using Bulb.Core;
using Bulb.LevelEditor.Popups;
using Bulb.Visuals.Grid;
using Settings.Model;
using UnityEngine;

namespace Bulb.Electricity
{
    public enum ErrorCode
    {
        TooManyBatteries,
        NoBatteries,
        BatteryNotConnected,
        UnclosedCircuit,
        NoConnectionsToBattery,
        ShortCircuit
    }

    public class CurrentWalker : MonoBehaviour
    {
        public static bool IsSimulating { get; private set; }
        public static List<ParallelGroup> ParallelGroups { get; set; }

        public delegate void SimulationFailed(ErrorCode code);
        public static event SimulationFailed OnSimulationFailed;

        public delegate void SimulationSucceeded();
        public static event SimulationSucceeded OnSimulationSucceeded;

        public delegate void ParallelGroupsAnalyzed();
        public static event ParallelGroupsAnalyzed OnParallelGroupsAnalyzed;

        private delegate void NodesEvaluated();
        private event NodesEvaluated OnNodesEvaluated;

        private List<CircuitPart> _persistentParts = new List<CircuitPart>();
        private List<CircuitPart> _partsToAnalyze = new List<CircuitPart>();
        private HashSet<StartPointData> _startPoints = new HashSet<StartPointData>();
        private HashSet<CircuitNode> _nodes = new HashSet<CircuitNode>();
        private int _parallelIterationLevel = 0;

        private Queue<CircuitNode> _nodeEvaluationQueue = new Queue<CircuitNode>();
        private List<CircuitNode> _runningEvaluationList = new List<CircuitNode>();
        private float _current = 0f;
        private BatteryCharacter _battery;
        private CircuitNode _startNode;
        private CircuitNode _endNode;
        private bool _nodesEvaluated = false;
        private bool _needsReevalution = false;
        private bool _isReevaluating = false;

        private struct StartPointData
        {
            public WirePiece WirePiece;
            public float BasePotentialDifference;

            public StartPointData(WirePiece wirePiece, float basePotentialDifference)
            {
                WirePiece = wirePiece;
                BasePotentialDifference = basePotentialDifference;
            }
        }

        public void OnEnable()
        {
            SettingsManager.Settings.ShowWireDirection.PropertyChanged += ShowWireDirection_PropertyChanged;
        }

        public void OnDisable()
        {
            SettingsManager.Settings.ShowWireDirection.PropertyChanged -= ShowWireDirection_PropertyChanged;
        }

        public void AnalyzeCircuit()
        {
            SettingsManager.Settings.ShowWireDirection.Value = false;

            _persistentParts.Clear();
            _partsToAnalyze.Clear();
            _nodes.Clear();
            _startPoints.Clear();

            ParallelGroups = new List<ParallelGroup>();
            _nodeEvaluationQueue.Clear();
            _runningEvaluationList.Clear();
            _parallelIterationLevel = 0;
            _nodesEvaluated = false;

            if (!_isReevaluating)
            {
                _needsReevalution = false;
                _isReevaluating = false;
            }

            OnNodesEvaluated -= OnNodesEvaluatedTriggered;

            var wireController = ApplicationController.Instance.WireController;
            wireController.ResetAllWirePieces();

            if (FindStartPoints())
            {
                wireController.ResetAllWirePieces();
                OnNodesEvaluated += OnNodesEvaluatedTriggered;

                FindCircuitNodes();
                FillNodeEvaluationQueue();
            }

        }

        private bool FindStartPoints()
        {
            var characters = ApplicationController.Instance.CharacterController.Characters;
            var batteryCount = characters.Where(c => c.Type == DrawableBase.DrawableType.Battery).Count();
            if (batteryCount == 0)
            {
                if (OnSimulationFailed != null)
                    OnSimulationFailed(ErrorCode.NoBatteries);

                WarningPopup.Instance.PopupMessage(WarningPopup.Type.Warning, "Geen batterijen gedetecteerd!");
                return false;
            }
            else if (batteryCount > 1)
            {
                if (OnSimulationFailed != null)
                    OnSimulationFailed(ErrorCode.TooManyBatteries);

                WarningPopup.Instance.PopupMessage(WarningPopup.Type.Warning, "Teveel batterijen gedetecteerd!");
                return false;
            }

            var battery = characters.Where(c => c.Type == DrawableBase.DrawableType.Battery).SingleOrDefault();
            _battery = battery as BatteryCharacter;
            var connections = battery.GetAllConnections();
            if (connections.Count != 2)
            {
                if (OnSimulationFailed != null)
                    OnSimulationFailed(ErrorCode.BatteryNotConnected);
                
                WarningPopup.Instance.PopupMessage(WarningPopup.Type.Warning, "Batterij is niet verbonden aan beide zijden");
                return false;
            }

            foreach (var connection in connections)
            {
                var wirePiece = connection as WirePiece;
                if (wirePiece != null)
                {
                    var pole = battery.GetConnectedPole(wirePiece);
                    var potential = pole == Pole.Negative ? -_battery.Battery.Voltage / 2 : _battery.Battery.Voltage / 2;

                    //wirePiece.DebugColor = Color.red;

                    if (!float.IsNaN(wirePiece.Voltage))
                        wirePiece.Voltage += potential;
                    else
                        wirePiece.Voltage = potential;

                    _startPoints.Add(new StartPointData(wirePiece, potential));
                    _startPoints.OrderByDescending(s => s.BasePotentialDifference);
                    _nodes.Add(new CircuitNode(wirePiece));
                }
            }

            return true;
        }

        private void FindCircuitNodes()
        {
            var wireController = ApplicationController.Instance.WireController;
            foreach (var wirePiece in wireController.WirePieces)
            {
                if (wirePiece.Connections.Count > 2)
                {
                    if (GetNode(wirePiece) == null)
                        _nodes.Add(new CircuitNode(wirePiece));
                }
            }
        }

        private void FillNodeEvaluationQueue()
        {
            if (_startPoints.Count > 0)
            {
                var startPoint = _startPoints.Where(s => s.BasePotentialDifference > 0).SingleOrDefault().WirePiece;
                _startNode = GetNode(startPoint);
                if (_startNode != null)
                {
                    _nodeEvaluationQueue.Enqueue(_startNode);
                    var connections = startPoint.Connections;
                    _endNode = GetNode(_battery.GetOppositePole(_battery.GetConnectedPole(startPoint)));
                    foreach (var connection in connections)
                    {
                        if (connection.Value.Type != DrawableBase.DrawableType.Battery)
                        {
                            _runningEvaluationList.Add(_startNode);
                            QueuedDetectNextNode(_startNode, connection.Key);
                        }
                    }

                    if(!_nodesEvaluated)
                    {

                        if (OnSimulationFailed != null)
                            OnSimulationFailed(ErrorCode.UnclosedCircuit);

                        WarningPopup.Instance.PopupMessage(WarningPopup.Type.Warning, "Je circuit is niet gesloten!");
                    }
                }
            }
            else
            {
                if (OnSimulationFailed != null)
                    OnSimulationFailed(ErrorCode.NoConnectionsToBattery);
                
                WarningPopup.Instance.PopupMessage(WarningPopup.Type.Warning, "De batterij is nergens mee verbonden!");
            }
        }

        private void QueuedDetectNextNode(CircuitNode startNode, Direction dir)
        {
            if (_nodesEvaluated)
                return;

            var newCircuitPart = new CircuitPart(startNode);
            startNode.AddConnection(dir, newCircuitPart);
            newCircuitPart.AddDrawableBase(startNode.WirePiece);
            var comingFromDirection = dir;

            var nextDrawable = startNode.WirePiece.GetConnection(dir);
            var previousDrawable = startNode.WirePiece as DrawableBase;
            var node = GetNode(nextDrawable);
            while (node == null)
            {
                newCircuitPart.AddDrawableBase(nextDrawable);

                // NextDrawable is a wire
                if (nextDrawable.Type == DrawableBase.DrawableType.Wire)
                {
                    var wirePiece = nextDrawable as WirePiece;
                    var wirePieceConnections = wirePiece.Connections;

                    // Wirepiece is a loose end
                    if (wirePieceConnections.Count == 1)
                    {
                        newCircuitPart.IsLooseEnd = true;
                        newCircuitPart.End = new CircuitNode(wirePiece);
                        _persistentParts.Add(newCircuitPart);

                        break;
                    }
                    // Wirepiece is straight
                    else if (wirePieceConnections.Count == 2)
                    {
                        foreach (var connection in wirePieceConnections)
                        {
                            if (connection.Value != previousDrawable)
                            {
                                previousDrawable = nextDrawable;
                                nextDrawable = connection.Value;
                                comingFromDirection = connection.Key;
                                break;
                            }
                        }
                    }
                }
                // Or a character
                else if (nextDrawable.Type != DrawableBase.DrawableType.Battery)
                {
                    if (nextDrawable.Type == DrawableBase.DrawableType.Switch)
                    {
                        var switchChar = nextDrawable as SwitchCharacter;
                        if (switchChar.IsOpen)
                        {
                            newCircuitPart.IsLooseEnd = true;
                            newCircuitPart.End = new CircuitNode(switchChar.GetConnectionViaDirection(comingFromDirection));
                            _persistentParts.Add(newCircuitPart);

                            break;
                        }
                    }

                    var character = nextDrawable as CharacterBase;
                    var connections = character.GetAllConnections();
                    newCircuitPart.AddResistance(character.Resistance);
                    foreach (var connection in connections)
                    {
                        if (connection != previousDrawable && connection != null)
                        {
                            previousDrawable = nextDrawable;
                            nextDrawable = connection;
                            comingFromDirection = (nextDrawable.PositionOnGrid - previousDrawable.PositionOnGrid).GetDirection();
                            break;
                        }
                    }

                    if (character.IsLinear == false && _isReevaluating == false)
                        _needsReevalution = true;

                    // If the next drawable is not a wire, the character is not connected, break the loop
                    if (nextDrawable.Type != DrawableBase.DrawableType.Wire)
                    {
                        node = null;
                        break;
                    }
                }
                // Or a battery
                else
                {
                    node = null;
                    break;
                }

                node = GetNode(nextDrawable);
            }

            if (node != null)
            {
                if (_nodeEvaluationQueue.Contains(node) == false)
                    _nodeEvaluationQueue.Enqueue(node);

                newCircuitPart.End = node;
                newCircuitPart.AddDrawableBase(node.WirePiece);
                node.AddConnection(comingFromDirection.GetOpposite(), newCircuitPart);
                _persistentParts.Add(newCircuitPart);
            }

            // Node evaluation completed, so remove it from the queue and the running list
            _runningEvaluationList.RemoveAt(_runningEvaluationList.LastIndexOf(startNode));

            // Continue with next node in queue
            CircuitNode nextNodeToEvaluate = null;
            if (_runningEvaluationList.Count == 0)
            {
                if (_nodeEvaluationQueue.Count > 0)
                    _nodeEvaluationQueue.Dequeue();

                if (_nodeEvaluationQueue.Count > 0)
                {
                    nextNodeToEvaluate = _nodeEvaluationQueue.Peek();

                    var wirePieceCount = nextNodeToEvaluate.IsDirectlyConnectedToBattery == true ? nextNodeToEvaluate.WirePiece.Connections.Count - 1 : nextNodeToEvaluate.WirePiece.Connections.Count;
                    while (nextNodeToEvaluate.GetConnectionCount() == wirePieceCount)
                    {
                        _nodeEvaluationQueue.Dequeue();

                        if (_nodeEvaluationQueue.Count > 0)
                            nextNodeToEvaluate = _nodeEvaluationQueue.Peek();
                        else
                            break;
                    }

                    foreach (var conn in nextNodeToEvaluate.WirePiece.Connections)
                    {
                        if (nextNodeToEvaluate.IsConnected(conn.Key) == false && conn.Value.Type != DrawableBase.DrawableType.Battery)
                        {
                            _runningEvaluationList.Add(nextNodeToEvaluate);
                        }
                    }
                }
            }
            else
            {
                nextNodeToEvaluate = startNode;
            }

            if (nextNodeToEvaluate != null)
            {
                foreach (var nextNodeConnections in nextNodeToEvaluate.WirePiece.Connections)
                {
                    if (nextNodeToEvaluate.IsConnected(nextNodeConnections.Key) == false && nextNodeConnections.Value.Type != DrawableBase.DrawableType.Battery)
                    {
                        QueuedDetectNextNode(nextNodeToEvaluate, nextNodeConnections.Key);
                    }
                }
            }

            if (!_nodesEvaluated && AllNodesEvaluated())
            {
                Debug.Log("Reached end");
                if (OnNodesEvaluated != null)
                    OnNodesEvaluated();

                _nodesEvaluated = true;
            }
        }

        //private void CheckCircuitPartsDirections()
        //{
        //    var partCount = _partsToAnalyze.Count();
        //    var partIndex = 0;

        //    while(partIndex < partCount)
        //    {
        //        var part = _partsToAnalyze[partIndex];
        //        var partsInWrongDirection = _partsToAnalyze.Where(p => p.End.Equals(part.End) && !p.Equals(part)).ToList();
        //        partIndex++;
        //        Debug.Log(partsInWrongDirection.Count());
        //    }
        //}

        private void OnNodesEvaluatedTriggered()
        {
            // Reset wirepieces
            var wireController = ApplicationController.Instance.WireController;
            wireController.ResetColor();

            _partsToAnalyze = _persistentParts.ConvertAll(part => part.Clone());

            //LogParts(_partsToAnalyze);

            DeleteLoops();
            DeleteLooseEnds();

            //CheckCircuitPartsDirections();

            while (FindParallelParts())
            {
                SimplifyCircuit();
                ReplaceParallelGroups();
                ++_parallelIterationLevel;
                DeleteEmptyCircuitParts(_partsToAnalyze);
            }

            if (OnParallelGroupsAnalyzed != null)
                OnParallelGroupsAnalyzed();

            //DebugDrawCircuit();

            // Calculate total equivalent resistance
            var totalEquivalentResistance = 0f;
            foreach (var part in _partsToAnalyze)
            {
                totalEquivalentResistance += part.ResistanceValue;
            }

            if (totalEquivalentResistance > 0)
            {
                // Calculate current in circuit
                var circuitVoltage = _battery.Battery.Voltage;
                _current = circuitVoltage / totalEquivalentResistance;
                Debug.LogFormat("The total equivalent resistance is {0}", totalEquivalentResistance);
                Debug.LogFormat("The current coming out of the battery is {0}", _current);

                // Propagate voltage to next node starting from startPoint
                PropagateCurrentAndVoltage(_startNode);

                if (!_battery.IsShortCircuited())
                {
                    var charController = ApplicationController.Instance.CharacterController;
                    foreach (var character in charController.Characters)
                    {
                        if (character.Type != DrawableBase.DrawableType.Battery)
                        {
                            if (_needsReevalution == false)
                            {
                                character.EvaluatePower();
                            }
                            else
                                character.EvaluatePotentialDifference();
                        }
                    }

                    if (_needsReevalution)
                    {
                        _needsReevalution = false;
                        _isReevaluating = true;
                        AnalyzeCircuit();
                    }
                    else
                    {
                        if (OnSimulationSucceeded != null)
                            OnSimulationSucceeded();
                    }

                    if (_isReevaluating == true)
                        _isReevaluating = false;
                }
                else
                {
                    if (OnSimulationFailed != null)
                        OnSimulationFailed(ErrorCode.ShortCircuit);

                    //WarningPopup.Instance.PopupMessage(WarningPopup.Type.Warning, "Je hebt een kortsluiting gemaakt!");
                }
            }
            else
            {
                if (OnSimulationFailed != null)
                    OnSimulationFailed(ErrorCode.ShortCircuit);
                
                //WarningPopup.Instance.PopupMessage(WarningPopup.Type.Warning, "Je hebt een kortsluiting gemaakt!");
            }
        }

        private void DeleteLooseEnds()
        {
            var looseEnds = _partsToAnalyze.Where(p => p.IsLooseEnd).ToList();
            foreach (var looseEnd in looseEnds)
            {
                var start = looseEnd.Start.Connections.Count > 0 ? looseEnd.Start : looseEnd.End;
                var node = GetNode(start.WirePiece);

                node.RemoveConnection(node.GetConnectionDirection(looseEnd));

                if (node.GetConnectionCount() == 2)
                {
                    var connections = node.GetConnectionDirections();

                    var firstConn = FindCircuitPartInWorkingPartList(node.GetConnectedCircuitPart(connections[0]));
                    var secondConn = FindCircuitPartInWorkingPartList(node.GetConnectedCircuitPart(connections[1]));

                    _partsToAnalyze.Remove(firstConn);
                    _partsToAnalyze.Remove(secondConn);

                    var newPart = MergeConnectedSerialCircuitParts(firstConn, secondConn);
                    _partsToAnalyze.Add(newPart);

                    _nodes.Remove(node);
                }
            }

            _partsToAnalyze.RemoveAll(p => p.IsLooseEnd);
        }

        private void DeleteLoops()
        {
            var loops = _partsToAnalyze.Where(p => p.Start.Equals(p.End)).ToList();
            foreach (var loop in loops)
            {
                _partsToAnalyze.Remove(loop);

                var node = GetNode(loop.Start.WirePiece);
                var nodeConnections = node.Connections;
                var connectionsToRemove = new List<Direction>();
                foreach (var connPair in nodeConnections)
                {
                    if (connPair.Value.Equals(loop))
                    {
                        connectionsToRemove.Add(connPair.Key);
                    }
                }

                connectionsToRemove.ForEach(c => node.RemoveConnection(c));

                if (node.GetConnectionCount() == 1)
                {
                    var circuitPart = node.GetConnectedCircuitPart(node.Connections.First().Key);
                    circuitPart.IsLooseEnd = true;
                }
            }
        }

        private void DeleteEmptyCircuitParts(List<CircuitPart> parts)
        {
            Debug.Log("DELETE EMPTY CIRCUIT PARTS");
            var emptyParts = parts.Where(p => p.ResistancesList.Count == 0 && p.LinkedToBattery == false).ToList();

            var emptyPartsToRemove = new List<CircuitPart>();
            foreach (var emptyPart in emptyParts)
            {
                if (NodeIsPartOfGroup(emptyPart.Start) || NodeIsPartOfGroup(emptyPart.End))
                {
                    foreach (var part in _partsToAnalyze)
                    {
                        if (emptyPart != part)
                        {
                            if (part.Start == emptyPart.Start)
                            {
                                part.Start = emptyPart.End;
                            }

                            if (part.End == emptyPart.Start)
                            {
                                part.End = emptyPart.End;
                            }

                            emptyPartsToRemove.Add(emptyPart);

                            foreach (var drawable in emptyPart.Drawables)
                            {
                                part.AddDrawableBase(drawable);
                            }
                        }
                    }
                }
            }

            parts.RemoveAll(p => emptyPartsToRemove.Contains(p));
            emptyParts.Clear();
        }

        private bool FindParallelParts()
        {
            Debug.Log("FIND PARALLEL PARTS");

            var parallelGroups = _partsToAnalyze.GroupBy(p => new { Endpoints = p.EndPoints }).Where(group => group.Count() > 1);

            if (parallelGroups.Count() == 0)
                return false;

            foreach (var group in parallelGroups)
            {
                var distinctGroup = group.Distinct(new CircuitPartComponentComparer()).ToList();
                if (distinctGroup.Count > 1)
                {
                    var parallelGroup = new ParallelGroup
                    {
                        Start = group.Key.Endpoints.Start,
                        End = group.Key.Endpoints.End,
                        IterationLevel = _parallelIterationLevel
                    };

                    ParallelGroups.Add(parallelGroup);

                    foreach (var circuitPart in distinctGroup)
                    {
                        var startNode = parallelGroup.Start;
                        startNode.AddParallelGroup(startNode.GetConnectionDirectionOfParallelGroupPart(circuitPart), parallelGroup);

                        var endNode = parallelGroup.End;
                        endNode.AddParallelGroup(endNode.GetConnectionDirectionOfParallelGroupPart(circuitPart), parallelGroup);

                        var persistentCircuitPart = _persistentParts.Where(p => p.Equals(circuitPart)).SingleOrDefault();
                        if (persistentCircuitPart == null)
                            parallelGroup.AddCircuitPart(circuitPart);
                        else
                            parallelGroup.AddCircuitPart(persistentCircuitPart);
                    }
                }
            }

            return true;
        }

        private void SimplifyCircuit()
        {
            Debug.Log("SIMPLIFY CIRCUIT");

            _partsToAnalyze = _partsToAnalyze.Distinct(new CircuitPartEndPointsComparer()).ToList();
            foreach (var part in _partsToAnalyze)
            {
                var group = FindParallelGroup(part.Start, part.End);
                if (group != null)
                {
                    var oldCircuitPartIndex = _partsToAnalyze.IndexOf(part);
                    var newCircuitPart = group.GetEquivalentCircuitPart();
                    _partsToAnalyze[oldCircuitPartIndex] = newCircuitPart;
                }
            }
        }

        private void ReplaceParallelGroups()
        {
            Debug.Log("REPLACE PARALLEL GROUPS START");
            
            foreach (var group in ParallelGroups)
            {
                var parallelGroupPart = _partsToAnalyze.Where(p => p.Start == group.Start &&
                                                                   p.End == group.End).SingleOrDefault();

                if (parallelGroupPart != null)
                {
                    CircuitPart previousPart = null;
                    CircuitPart nextPart = null;

                    var query = parallelGroupPart.Start.Connections.Where(c => !parallelGroupPart.Start.IsConnectedToParallelGroupInDirection(group, c.Key))
                                                                   .Select(c => c.Value);
                    var persistentPartBeforeGroup = query.Count() == 1 ? query.Single() : null;
                    if (persistentPartBeforeGroup != null)
                    {
                        previousPart = FindCircuitPartInWorkingPartList(persistentPartBeforeGroup);
                        previousPart.ChildCircuitParts.Add(persistentPartBeforeGroup); 
                    }

                    query = parallelGroupPart.End.Connections.Where(c => !parallelGroupPart.End.IsConnectedToParallelGroupInDirection(group, c.Key))
                                                             .Select(c => c.Value);
                    var persistentPartAfterGroup = query.Count() == 1 ? query.Single() : null;
                    if (persistentPartAfterGroup != null)
                    {
                        nextPart = FindCircuitPartInWorkingPartList(persistentPartAfterGroup);
                        nextPart.ChildCircuitParts.Add(persistentPartAfterGroup); 
                    }

                    // Merge three parts (previous - parallel - next)
                    if(previousPart != null && nextPart != null)
                    {
                        var previousPartIndex = _partsToAnalyze.IndexOf(previousPart);
                        previousPart = MergeConnectedSerialCircuitParts(previousPart, parallelGroupPart);
                        previousPart = MergeConnectedSerialCircuitParts(previousPart, nextPart);

                        _partsToAnalyze[previousPartIndex] = previousPart;
                        _partsToAnalyze.Remove(parallelGroupPart);
                        _partsToAnalyze.Remove(nextPart);
                    }
                    // Merge two parts (parallel - next)
                    else if(previousPart == null && nextPart != null)
                    {
                        var parallelGroupPartIndex = _partsToAnalyze.IndexOf(parallelGroupPart);
                        parallelGroupPart = MergeConnectedSerialCircuitParts(parallelGroupPart, nextPart);

                        _partsToAnalyze[parallelGroupPartIndex] = parallelGroupPart;
                        _partsToAnalyze.Remove(nextPart);
                    }
                    // Merge two parts (previous - parallel)
                    else if (previousPart != null && nextPart == null)
                    {
                        var previousPartIndex = _partsToAnalyze.IndexOf(previousPart);
                        previousPart = MergeConnectedSerialCircuitParts(previousPart, parallelGroupPart);

                        _partsToAnalyze[previousPartIndex] = previousPart;
                        _partsToAnalyze.Remove(parallelGroupPart);
                    }
                }
            }
        }

        private void PropagateCurrentAndVoltage(CircuitNode startNode)
        {
            var currVoltage = _battery.Battery.Voltage;
            var currCurrent = _current;

            var nextNode = startNode;
            nextNode.WirePiece.Current = currCurrent;
            nextNode.WirePiece.Voltage = currVoltage;

            while (true)
            {
                if (nextNode.Connections.Count == 1)
                {
                    var part = nextNode.Connections.First().Value;
                    part.PropagateCurrent(nextNode.WirePiece.Current);
                    part.PropagateVoltage();

                    nextNode = part.End;
                }
                else
                {
                    if (NodeIsPartOfGroup(nextNode))
                    {
                        var group = FindParallelGroupByStart(nextNode);
                        if (group != null)
                        {
                            PropagateGroup(group);
                            nextNode = group.End;
                        }
                        // Node is end of group, propagate remaining current
                        else
                        {
                            var remainingCurrent = GetRemainingCurrentInNode(nextNode);
                            foreach (var pair in nextNode.Connections)
                            {
                                if (float.IsNaN(pair.Value.Current))
                                {
                                    pair.Value.PropagateCurrent(remainingCurrent);
                                    pair.Value.PropagateVoltage();

                                    nextNode = pair.Value.End;
                                }
                            }
                        }
                    }
                    else
                    {
                        // TODO : Find a better, working way to let this work
                        foreach (var connectionPair in nextNode.Connections)
                        {
                            if (float.IsNaN(connectionPair.Value.Current) && (connectionPair.Value.End != null && connectionPair.Value.Start != null) && connectionPair.Value.IsLooseEnd == false)
                            {
                                connectionPair.Value.PropagateCurrent(nextNode.WirePiece.Current);
                                connectionPair.Value.PropagateVoltage();

                                if (connectionPair.Value.End.Equals(nextNode))
                                    nextNode = connectionPair.Value.Start;
                                else
                                    nextNode = connectionPair.Value.End;

                                if (nextNode == _endNode)
                                    break;
                            }
                        }
                    }
                }

                if (nextNode == null || nextNode == _endNode)
                    break;
            }
        }

        private void PropagateGroup(ParallelGroup group)
        {
            // First check whether all the parts have resistances, otherwise ignore all the ones with resistances
            var zeroResistancePart = group.GetZeroResistancePart();
            if (zeroResistancePart != null)
            {
                var voltage = group.Start.WirePiece.Voltage;

                foreach (var part in group.CircuitParts)
                {
                    part.PropagateCurrent(part.Equals(zeroResistancePart) ? group.Start.WirePiece.Current : 0);
                    part.PropagateVoltage();
                }

                group.Start.WirePiece.Voltage = voltage;
                group.End.WirePiece.Voltage = voltage;
            }
            // Then see if the group all has equal resistances (most easy way)
            else if (group.ResistancesInGroupAreEqual())
            {
                // Propagate group
                var newCurrent = group.Start.WirePiece.Current / group.CircuitParts.Count;
                foreach (var circuitPart in group.CircuitParts)
                {
                    if (float.IsNaN(circuitPart.Current))
                    {
                        circuitPart.PropagateCurrent(newCurrent, circuitPart.Start.Equals(group.End));
                        circuitPart.PropagateVoltage(circuitPart.Start.Equals(group.End));
                    }
                }
            }
            // Otherwise divide current between circuitparts
            else
            {
                foreach (var part in group.CircuitParts)
                {
                    PropagateParallelGroupCircuitPart(group, part);
                }
            }
        }

        private void PropagateParallelGroupCircuitPart(ParallelGroup group, CircuitPart part)
        {
            var nextNode = group.Start;
            while (nextNode != group.End)
            {
                // If there are too many unpropagated connections, bail out
                if (NumberOfUnpropagatedConnections(nextNode) > 1 && nextNode != group.Start)
                    return;

                foreach (var dir in nextNode.GetConnectionDirections())
                {
                    var persistentConnectedPart = nextNode.GetConnectedCircuitPart(dir);
                    if (float.IsNaN(persistentConnectedPart.Current))
                    {
                        if (part.ChildCircuitParts.Count == 0 || part.ContainsChildCircuitPart(persistentConnectedPart))
                        {
                            var newCurrent = GetRemainingCurrentInNode(nextNode);
                            if (float.IsNaN(newCurrent))
                                newCurrent = nextNode.WirePiece.Current * (group.EquivalentResistance / part.ResistanceValue);

                            // If the end of the circuitPart matches nextNode, reverse propagate
                            persistentConnectedPart.PropagateCurrent(newCurrent, nextNode == persistentConnectedPart.End);
                            persistentConnectedPart.PropagateVoltage(nextNode == persistentConnectedPart.End);

                            if (nextNode != persistentConnectedPart.End)
                            {
                                if (persistentConnectedPart.End != part.End)
                                {
                                    var newGroup = FindParallelGroupByStart(persistentConnectedPart.End);

                                    if (newGroup != null)
                                    {
                                        PropagateGroup(newGroup);

                                        nextNode = newGroup.End;
                                        break;
                                    }
                                    else
                                    {
                                        nextNode = persistentConnectedPart.End;
                                        break;
                                    }
                                }
                                else
                                {
                                    nextNode = persistentConnectedPart.End;
                                    break;
                                }
                            }
                            else
                            {
                                if (persistentConnectedPart.Start != part.End)
                                {
                                    var newGroup = FindParallelGroupByStart(persistentConnectedPart.Start);

                                    if (newGroup != null)
                                    {
                                        PropagateGroup(newGroup);

                                        nextNode = newGroup.End;
                                        break;
                                    }
                                    else
                                    {
                                        nextNode = persistentConnectedPart.Start;
                                        break;
                                    }
                                }
                                else
                                {
                                    nextNode = persistentConnectedPart.Start;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private CircuitNode PropagateLastNodeConnection(CircuitNode node, float current, float voltage)
        {
            foreach (var dir in node.GetConnectionDirections())
            {
                var part = node.GetConnectedCircuitPart(dir);
                if (float.IsNaN(part.Current))
                {
                    part.PropagateCurrent(current, part.End == node);
                    part.PropagateVoltage(part.End == node);

                    if (part.End == node)
                        return part.Start;
                    else
                        return part.End;
                }
            }

            return null;
        }

        private int NumberOfUnpropagatedConnections(CircuitNode node)
        {
            return node.Connections.Where(c => float.IsNaN(c.Value.Current)).Count();
        }

        /// <summary>
        /// Get remaining current in the node
        /// Only works if the node is the end of a group
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private float GetRemainingCurrentInNode(CircuitNode groupEnd)
        {
            var group = FindParallelGroupByEnd(groupEnd);
            var remainingCurrent = float.NaN;
            if (group != null)
            {
                foreach (var pair in groupEnd.Connections)
                {
                    if (groupEnd.IsConnectedToAParallelGroupInDirection(pair.Key))
                    {
                        if (float.IsNaN(remainingCurrent))
                            remainingCurrent = pair.Value.Current;
                        else
                            remainingCurrent += pair.Value.Current;
                    }
                }
            }

            return remainingCurrent;
        }

        #region HELPER_METHODS
        private CircuitPart MergeConnectedSerialCircuitParts(CircuitPart first, CircuitPart second)
        {
            var newCircuitPart = new CircuitPart(first.Start);

            newCircuitPart.AddResistance(first.ResistanceValue);
            newCircuitPart.AddResistance(second.ResistanceValue);
            newCircuitPart.ChildCircuitParts.Add(_persistentParts.Where(p => p.Equals(first)).SingleOrDefault() ?? first);
            newCircuitPart.ChildCircuitParts.Add(_persistentParts.Where(p => p.Equals(second)).SingleOrDefault() ?? second);
            newCircuitPart.Start = GetNonSharedCircuitNode(first, second);
            newCircuitPart.End = GetNonSharedCircuitNode(second, first);

            return newCircuitPart;
        }

        private CircuitPart FindCircuitPartInWorkingPartList(CircuitPart persistentPart)
        {
            var returnValue = _partsToAnalyze.Where(p => p.ContainsChildCircuitPart(persistentPart)).SingleOrDefault();
            return returnValue;
        }

        private ParallelGroup FindParallelGroup(CircuitNode start, CircuitNode end)
        {
            return FindParallelGroup(start, end, _parallelIterationLevel);
        }

        private ParallelGroup FindParallelGroup(CircuitNode start, CircuitNode end, int iterationLevel)
        {
            var group = ParallelGroups.Find(p => (p.IterationLevel == iterationLevel) && ((p.Start == start && p.End == end) || (p.Start == end && p.End == start)));
            return group;
        }

        private ParallelGroup FindParallelGroupByStart(CircuitNode start)
        {
            var group = ParallelGroups.Find(p => p.Start == start);
            return group;
        }

        private ParallelGroup FindParallelGroupByEnd(CircuitNode end)
        {
            var group = ParallelGroups.Find(p => p.End == end);
            return group;
        }

        private ParallelGroup FindParallelGroupByDrawable(DrawableBase drawable)
        {
            var group = ParallelGroups.Find(p => p.ContainsDrawable(drawable));
            return group;
        }

        private ParallelGroup FindParallelGroupByDrawable(DrawableBase drawable, int iterationLevel)
        {
            for (var i = ParallelGroups.Count - 1; i > -1; --i)
            {
                var group = ParallelGroups[i];
                if (group.IterationLevel <= iterationLevel)
                {
                    if (group.ContainsDrawable(drawable))
                        return group;
                }
            }

            return null;
        }

        /// <summary>
        /// To be used when merging two serial parts. 
        /// They must have at least 1 common circuit node
        /// </summary>
        /// <param name="firstPart"></param>
        /// <param name="secondPart"></param>
        /// <returns>Returns the shared circuit node of the two pieces</returns>
        private CircuitNode GetSharedCircuitNode(CircuitPart firstPart, CircuitPart secondPart)
        {
            if (firstPart.Start == secondPart.Start || firstPart.Start == secondPart.End)
                return firstPart.Start;

            if (firstPart.End == secondPart.Start || firstPart.End == secondPart.End)
                return firstPart.End;

            return null;
        }

        /// <summary>
        /// To be used when merging two serial parts. 
        /// They must have at least 1 common circuit node
        /// </summary>
        /// <param name="firstPart"></param>
        /// <param name="secondPart"></param>
        /// <returns>Returns the non shared circuit node of the First Part</returns>
        private CircuitNode GetNonSharedCircuitNode(CircuitPart firstPart, CircuitPart secondPart)
        {
            if (firstPart.Start == secondPart.Start || firstPart.Start == secondPart.End)
                return firstPart.End;

            if (firstPart.End == secondPart.Start || firstPart.End == secondPart.End)
                return firstPart.Start;

            return null;
        }

        private CircuitPart FindPartByEnd(CircuitNode end)
        {
            foreach (var part in _partsToAnalyze)
            {
                if (part.End == end)
                    return part;
            }

            return null;
        }

        private CircuitPart FindPartByEndOrStart(CircuitNode node)
        {
            foreach (var part in _partsToAnalyze)
            {
                if (part != null)
                {
                    if (part.End == node || part.Start == node)
                        return part;
                }
            }

            return null;
        }

        private bool AllNodesEvaluated()
        {
            foreach (var node in _nodes)
            {
                if (node.IsDirectlyConnectedToBattery)
                {
                    if (node.WirePiece.Connections.Count - 1 != node.GetConnectionCount())
                        return false;
                }
                else
                {
                    if (node.WirePiece.Connections.Count != node.GetConnectionCount())
                        return false;
                }
            }

            return true;
        }

        private bool NodeIsPartOfGroup(CircuitNode node)
        {
            foreach (var group in ParallelGroups)
            {
                if (group.ContainsNode(node))
                    return true;
            }

            return false;
        }

        private bool NodeIsStartOfGroup(CircuitNode node)
        {
            foreach (var group in ParallelGroups)
            {
                if (group.Start == node)
                    return true;
            }

            return false;
        }

        private bool NodeIsEndOfGroup(CircuitNode node)
        {
            foreach (var group in ParallelGroups)
            {
                if (group.End == node)
                    return true;
            }

            return false;
        }

        private StartPointData GetStartPoint(DrawableBase piece)
        {
            foreach (var startPoint in _startPoints)
            {
                var wirePiece = startPoint.WirePiece;
                if (wirePiece == piece)
                {
                    return startPoint;
                }
            }

            return new StartPointData();
        }

        private CircuitNode GetNode(DrawableBase piece)
        {
            foreach (var node in _nodes)
            {
                if (node.WirePiece == piece)
                {
                    return node;
                }
            }

            return null;
        }

        private bool DrawableIsNode(DrawableBase piece)
        {
            if (piece.Type == DrawableBase.DrawableType.Wire)
            {
                foreach (var node in _nodes)
                {
                    if (node.WirePiece == piece)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void LogParts(List<CircuitPart> list)
        {
            var index = 0;
            foreach (var part in list)
            {
                Debug.LogFormat("Part {0} starts at {1} and ends at {2} and has {3} components and resistance value {4}",
                                    index,
                                    part.Start != null ? part.Start.WirePiece.PositionOnGrid.ToString() : "[NO_START]",
                                    part.End != null ? part.End.WirePiece.PositionOnGrid.ToString() : "[NO_END]",
                                    part.ComponentCount,
                                    part.ResistanceValue);
                ++index;

                part.Start.WirePiece.DebugColor = Color.red;
                part.End.WirePiece.DebugColor = Color.red;

                part.DrawDebug(false);
            }
        }

        private void LogParallelGroups()
        {
            var index = 0;
            foreach (var group in ParallelGroups)
            {
                Debug.LogFormat("ParallelGroup {0} with start at {1} and end at {2} has equivalent resistance of {3}",
                                                index,
                                                group.Start.WirePiece.PositionOnGrid,
                                                group.End.WirePiece.PositionOnGrid,
                                                group.EquivalentResistance);
                ++index;
            }
        }

        private void DebugDrawCircuit()
        {
            var wireController = ApplicationController.Instance.WireController;
            wireController.ResetColor();

            foreach (var node in _nodes)
            {
                node.WirePiece.DebugColor = Color.red;
            }

            LogParts(_persistentParts);
            LogParts(_partsToAnalyze);
            LogParallelGroups();
        }

        public static void ShowParallelGroup(int index)
        {
            SettingsManager.Settings.ShowWireDirection.Value = false;

            for (var i = 0; i < ParallelGroups.Count; ++i)
            {
                var group = ParallelGroups[i];
                group.DebugColor = new Color32(255, 140, 0, 255);
                group.DrawDebug(false);

                group.Start.WirePiece.DebugColor = Color.red;
                group.End.WirePiece.DebugColor = Color.red;
            }

            if (index >= 0 && index < ParallelGroups.Count)
            {
                var newGroup = ParallelGroups[index];
                newGroup.DrawDebug(true);

                newGroup.Start.WirePiece.DebugColor = Color.yellow;
                newGroup.End.WirePiece.DebugColor = Color.yellow;
            }
        }

        public void SetIsSimulating(bool value)
        {
            IsSimulating = value;

            if(!value)
            {
                ApplicationController.Instance.WireController.ResetAllWirePieces();
                ApplicationController.Instance.CharacterController.ResetAllCharacters();
            }
        }

        private void ShowWireDirection_PropertyChanged(object sender, SettingChangedEventArgs<bool> e)
        {
            if (e.NewValue)
            {
                foreach (var part in _persistentParts)
                {
                    var drawables = part.GetDrawablesRecursive();
                    float index = 0;
                    float count = drawables.Count;
                    foreach (var drawable in drawables)
                    {
                        if (drawable.Type == DrawableBase.DrawableType.Wire)
                        {
                            if (!drawable.Equals(part.Start.WirePiece) && !drawable.Equals(part.End.WirePiece))
                            {
                                drawable.DebugColor = new Color(index / count, (drawables.Count - index) / count, 0);
                            }
                            else
                            {
                                drawable.DebugColor = Color.red;
                            }

                            ++index;
                        }
                    }
                }
            }
            else
            {
                var wireController = ApplicationController.Instance.WireController;
                wireController.ResetColor();
            }
        }
        #endregion
    }
}