using System.Collections.Generic;
using System.Linq;
using Bulb.Characters.Wire;
using Bulb.Visuals.Grid;

namespace Bulb.Electricity
{
    public class CircuitNode
    {
        public WirePiece WirePiece;
        public bool HasBeenVisited;

        private Dictionary<Direction, CircuitPart> _connections = new Dictionary<Direction, CircuitPart>();
        public Dictionary<Direction, CircuitPart> Connections
        {
            get
            {
                return _connections;
            }
        }

        private Dictionary<Direction, ParallelGroup> _parallelGroups = new Dictionary<Direction, ParallelGroup>();
        public Dictionary<Direction, ParallelGroup> ParallelGroups
        {
            get
            {
                return _parallelGroups;
            }
        }

        public bool IsDirectlyConnectedToBattery
        {
            get
            {
                foreach(var pair in WirePiece.Connections)
                {
                    if (pair.Value.Type == Characters.DrawableBase.DrawableType.Battery)
                        return true;
                }

                return false;
            }
        }

        public CircuitNode(WirePiece wirePiece)
        {
            WirePiece = wirePiece;
        }

        public List<Direction> GetConnectionDirections()
        {
            return _connections.Keys.ToList();
        }

        public CircuitPart GetConnectedCircuitPart(Direction direction)
        {
            if (_connections.ContainsKey(direction))
                return _connections[direction];

            return null;
        }

        public void AddConnection(Direction direction, CircuitPart part)
        {
            if (!_connections.ContainsKey(direction))
            {
                _connections.Add(direction, part);
            }
        }

        public void RemoveConnection(Direction direction)
        {
            if(_connections.ContainsKey(direction))
            {
                _connections.Remove(direction);
            }
        }

        public void ClearConnections()
        {
            _connections.Clear();
        }

        public int GetConnectionCount()
        {
            return _connections.Count;
        }

        public Direction GetConnectionDirection(CircuitPart part)
        {
            foreach (var connectionPair in _connections)
            {
                if (connectionPair.Value.Equals(part))
                {
                    return connectionPair.Key;
                }
            }

            return Direction.None;
        }

        public Direction GetConnectionDirectionOfParallelGroupPart(CircuitPart part)
        {
            foreach (var connectionPair in _connections)
            {
                var connectionPairValue = connectionPair.Value;
                if (part.ChildCircuitParts.Count > 0)
                {
                    if (part.ContainsChildCircuitPart(connectionPairValue))
                        return connectionPair.Key;
                }
                else
                {
                    if (connectionPair.Value.Equals(part))
                        return connectionPair.Key;
                }
            }

            return Direction.None;
        }

        public Direction GetConnectionDirectionOfParallelGroup(ParallelGroup group)
        {
            foreach (var connectionPair in _parallelGroups)
            {
                if (connectionPair.Value == group)
                {
                    return connectionPair.Key;
                }
            }

            return Direction.None;
        }

        public void AddParallelGroup(Direction direction, ParallelGroup group)
        {
            if (!_parallelGroups.ContainsKey(direction))
            {
                _parallelGroups.Add(direction, group);
            }
        }

        public bool IsConnectedToAParallelGroupInDirection(Direction direction)
        {
            return _parallelGroups.ContainsKey(direction);
        }

        public bool IsConnectedToParallelGroupInDirection(ParallelGroup group, Direction direction)
        {
            if(IsConnectedToAParallelGroupInDirection(direction))
            {
                return _parallelGroups[direction] == group;
            }

            return false;
        }

        public bool IsConnected(Direction direction)
        {
            return _connections.ContainsKey(direction);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            var otherNode = (CircuitNode)obj;
            return WirePiece.PositionOnGrid == otherNode.WirePiece.PositionOnGrid;
        }

        public static bool operator ==(CircuitNode n1, CircuitNode n2)
        {
            if (ReferenceEquals(n1, null))
            {
                return ReferenceEquals(n2, null);
            }

            return n1.Equals(n2);
        }

        public static bool operator !=(CircuitNode n1, CircuitNode n2)
        {
            return !(n1 == n2);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return WirePiece.PositionOnGrid.ToString();
        }
    }
}