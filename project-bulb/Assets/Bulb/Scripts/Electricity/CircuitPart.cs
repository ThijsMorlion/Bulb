using System.Collections.Generic;
using System.Linq;
using Bulb.Characters;
using Bulb.Characters.Wire;
using UnityEngine;

namespace Bulb.Electricity
{
    public class CircuitPart
    {
        public float Current = float.NaN;
        public List<DrawableBase> Drawables { get; private set; }
        public bool LinkedToBattery { get; private set; }
        public bool IsLooseEnd { get; set; }
        public Color DebugColor { get; set; }

        public List<float> ResistancesList { get; private set; }
        public List<CircuitPart> ChildCircuitParts { get; set; }

        public float ResistanceValue
        {
            get
            {
                var sum = 0f;

                foreach (var value in ResistancesList)
                {
                    sum += value;
                }

                return sum;
            }
        }

        public int ComponentCount
        {
            get
            {
                var sum = 0;

                if (ChildCircuitParts.Count == 0)
                {
                    foreach (var node in Drawables)
                    {
                        if (node)
                            sum += node.Type != DrawableBase.DrawableType.Wire ? 1 : 0;
                    }
                }
                else
                {
                    foreach (var child in ChildCircuitParts)
                    {
                        sum += child.ComponentCount;
                    }
                }

                return sum;
            }
        }

        public EndPointsClass EndPoints { get; private set; }

        private CircuitNode _start;
        public CircuitNode Start
        {
            get { return _start; }
            set
            {
                _start = value;
                EndPoints.Start = _start;
                UpdateConnectedToBattery();
            }
        }

        private CircuitNode _end;
        public CircuitNode End
        {
            get { return _end; }
            set
            {
                _end = value;
                EndPoints.End = _end;
                UpdateConnectedToBattery();
            }
        }

        public CircuitPart(CircuitNode start)
        {
            EndPoints = new EndPointsClass();

            Start = start;
            Drawables = new List<DrawableBase>();
            ResistancesList = new List<float>();
            ChildCircuitParts = new List<CircuitPart>();

            DebugColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.7f, 1f);
        }

        public CircuitPart Clone()
        {
            var clone = new CircuitPart(Start);
            foreach (var node in Drawables)
            {
                clone.AddDrawableBase(node);
            }

            foreach (var resistance in ResistancesList)
            {
                clone.AddResistance(resistance);
            }

            clone.EndPoints = new EndPointsClass()
            {
                End = End,
                Start = Start,
            };

            clone.End = End;
            clone.Start = Start;
            clone.DebugColor = DebugColor;
            clone.IsLooseEnd = IsLooseEnd;
            clone.LinkedToBattery = LinkedToBattery;

            return clone;
        }

        public List<DrawableBase> GetDrawablesRecursive()
        {
            if (ChildCircuitParts.Count > 0)
            {
                var returnVal = new List<DrawableBase>();
                foreach (var child in ChildCircuitParts)
                {
                    foreach (var drawable in child.GetDrawablesRecursive())
                    {
                        returnVal.Add(drawable);
                    }
                }

                return returnVal;
            }
            else
            {
                return Drawables;
            }
        }

        public void SortChildCircuitParts()
        {
            var query = ChildCircuitParts.Where(c => c.Start.Equals(Start) || c.End.Equals(Start));

            // Return if this the circuitpart is a parallel group
            if (query.Count() > 1)
                return;

            var nextChild = query.SingleOrDefault();
            var previousChild = nextChild;
            var childIndex = 0;
            while (childIndex < ChildCircuitParts.Count)
            {
                var reverse = !previousChild.Equals(nextChild) && !previousChild.End.Equals(nextChild.Start);
                if (reverse)
                {
                    nextChild.Drawables.Reverse();

                    var temp = nextChild.Start;
                    nextChild.Start = nextChild.End;
                    nextChild.End = temp;
                }

                previousChild = nextChild;

                if (reverse)
                    nextChild = ChildCircuitParts.Where(c => !c.Equals(nextChild) && (c.Start.Equals(nextChild.Start) || c.End.Equals(nextChild.Start))).SingleOrDefault();
                else
                    nextChild = ChildCircuitParts.Where(c => !c.Equals(nextChild) && (c.Start.Equals(nextChild.End) || c.End.Equals(nextChild.End))).SingleOrDefault();

                ++childIndex;
            }
        }

        public void PropagateCurrent(float current, bool reverse = false)
        {
            if (ChildCircuitParts.Count == 0)
            {
                Current = current;

                foreach (var drawable in Drawables)
                {
                    if (drawable != Start.WirePiece && drawable != End.WirePiece)
                    {
                        if (drawable.Type == DrawableBase.DrawableType.Wire)
                        {
                            var wirePiece = drawable as WirePiece;
                            wirePiece.Current = current;
                        }
                        else if (drawable.Type == DrawableBase.DrawableType.Buzzer)
                        {
                            var buzzer = drawable as BuzzerCharacter;
                            buzzer.EvaluateDiodeDirection();
                            buzzer.Current = current;
                        }
                        else
                        {
                            var character = drawable as CharacterBase;
                            character.Current = current;
                        }
                    }
                }

                if (reverse)
                {
                    if (Start != null)
                    {
                        if (float.IsNaN(Start.WirePiece.Current))
                            Start.WirePiece.Current = current;
                        else
                            Start.WirePiece.Current += current;
                    }
                }
                else
                {
                    if (End != null)
                    {
                        if (float.IsNaN(End.WirePiece.Current))
                            End.WirePiece.Current = current;
                        else
                            End.WirePiece.Current += current;
                    }
                }
            }
            else
            {
                SortChildCircuitParts();
                foreach (var child in ChildCircuitParts)
                {
                    child.PropagateCurrent(current, reverse);
                }
            }
        }

        public void PropagateVoltage(bool reverse = false)
        {
            if (ChildCircuitParts.Count == 0)
            {
                var voltage = Start.WirePiece.Voltage;

                foreach (var drawable in Drawables)
                {
                    if (drawable != Start.WirePiece && drawable != End.WirePiece)
                    {
                        if (drawable.Type == DrawableBase.DrawableType.Wire)
                        {
                            var wirePiece = drawable as WirePiece;
                            wirePiece.Voltage = voltage;
                        }
                        else if (drawable.Type != DrawableBase.DrawableType.Battery)
                        {
                            var character = drawable as CharacterBase;
                            var resistance = character.Resistance;

                            voltage -= resistance * Current;
                            if (voltage < 0.001)
                                voltage = 0;
                        }
                    }
                }

                if (Start != null && End != null)
                {
                    if (reverse)
                        Start.WirePiece.Voltage = voltage;
                    else
                        End.WirePiece.Voltage = voltage;
                }
            }
            else
            {
                SortChildCircuitParts();
                foreach (var child in ChildCircuitParts)
                {
                    child.PropagateVoltage(reverse);
                }
            }
        }

        public DrawableBase GetRandomDrawable()
        {
            foreach (var drawable in Drawables)
            {
                if (drawable != Start.WirePiece && (End == null || drawable != End.WirePiece))
                    return drawable;
            }

            return null;
        }

        public void AddDrawableBase(DrawableBase drawable)
        {
            Drawables.Add(drawable);
        }

        public bool ContainsDrawable(DrawableBase drawable)
        {
            if (ChildCircuitParts.Count > 0)
            {
                foreach (var child in ChildCircuitParts)
                {
                    if (child.ContainsDrawable(drawable))
                        return true;
                }

                return false;
            }
            else
            {
                return Drawables.Contains(drawable);
            }
        }

        public bool ContainsNode(CircuitNode node)
        {
            return (Start == node || End == node);
        }

        public void DrawDebug(bool value)
        {
            foreach (var drawable in Drawables)
            {
                if (End != null && End.WirePiece == drawable)
                    continue;

                if (Start != null && Start.WirePiece == drawable)
                    continue;

                if (drawable.Type == DrawableBase.DrawableType.Wire)
                    drawable.DebugColor = value == true ? DebugColor : Color.white;
            }
        }

        private void UpdateConnectedToBattery()
        {
            if (Start != null)
            {
                var connections = Start.WirePiece.Connections;
                foreach (var conn in connections)
                {
                    if (conn.Value.Type == DrawableBase.DrawableType.Battery)
                    {
                        LinkedToBattery = true;
                        return;
                    }
                }
            }

            if (End != null)
            {
                var connections = End.WirePiece.Connections;
                foreach (var conn in connections)
                {
                    if (conn.Value.Type == DrawableBase.DrawableType.Battery)
                    {
                        LinkedToBattery = true;
                        return;
                    }
                }
            }

            LinkedToBattery = false;
        }

        public void AddResistance(float value)
        {
            ResistancesList.Add(value);
        }

        public bool ContainsChildCircuitPart(CircuitPart part)
        {
            var returnValue = ChildCircuitParts.Contains(part);
            if (returnValue == false && ChildCircuitParts.Count > 0)
            {
                foreach (var childpart in ChildCircuitParts)
                {
                    if (childpart.ContainsChildCircuitPart(part))
                        returnValue = true;
                }
            }
            else if (returnValue == false && ChildCircuitParts.Count == 0)
            {
                returnValue = part.Equals(this);
            }

            return returnValue;
        }

        public bool ContainsLooseEnd()
        {
            if (ChildCircuitParts.Count > 0)
            {
                var containsLooseEnd = false;
                foreach (var child in ChildCircuitParts)
                {
                    if (child.ContainsLooseEnd())
                        containsLooseEnd = true;
                }

                return containsLooseEnd;
            }

            return IsLooseEnd;
        }

        public override int GetHashCode()
        {
            var sum = 0;
            foreach (var node in GetDrawablesRecursive())
            {
                sum += node.GetHashCode();
            }

            return sum;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            var otherPart = (CircuitPart)obj;

            foreach (var node in otherPart.GetDrawablesRecursive())
            {
                if (ContainsDrawable(node) == false)
                    return false;
            }

            if (Start.WirePiece.PositionOnGrid != otherPart.Start.WirePiece.PositionOnGrid)
                return false;

            if (End.WirePiece.PositionOnGrid != otherPart.End.WirePiece.PositionOnGrid)
                return false;

            return true;
        }

        public override string ToString()
        {
            return string.Format("Start: {0} | End: {1} | Resistance: {2} | Drawables: {3} | Components: {4}", Start, End, ResistanceValue, GetDrawablesRecursive().Count, ComponentCount);
        }
    }

    public class EndPointsClass
    {
        public CircuitNode Start;
        public CircuitNode End;

        public override bool Equals(object obj)
        {
            var otherEndPoints = (EndPointsClass)obj;
            return (Start == otherEndPoints.Start && End == otherEndPoints.End) || (Start == otherEndPoints.End && End == otherEndPoints.Start);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(EndPointsClass first, EndPointsClass second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(EndPointsClass first, EndPointsClass second)
        {
            return !first.Equals(second);
        }
    }

    public class CircuitPartComponentComparer : IEqualityComparer<CircuitPart>
    {
        public bool Equals(CircuitPart first, CircuitPart second)
        {
            return first.GetHashCode() == second.GetHashCode();
        }

        public int GetHashCode(CircuitPart obj)
        {
            return obj.GetHashCode();
        }
    }

    public class CircuitPartEndPointsComparer : IEqualityComparer<CircuitPart>
    {
        public bool Equals(CircuitPart first, CircuitPart second)
        {
            return (first.Start == second.Start && first.End == second.End) || (first.Start == second.End && first.End == second.Start);
        }

        public int GetHashCode(CircuitPart obj)
        {
            return 0;
        }
    }
}