using System.Collections.Generic;
using Bulb.Characters;
using UnityEngine;
using System.Linq;

namespace Bulb.Electricity
{
    public class ParallelGroup
    {
        public CircuitNode Start { get; set; }
        public CircuitNode End { get; set; }
        public List<CircuitPart> CircuitParts { get; private set; }
        public Color DebugColor { get; set; }
        public int IterationLevel { get; set; }

        public ParallelGroup()
        {
            CircuitParts = new List<CircuitPart>();

            DebugColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.7f, 1f);
        }

        public float EquivalentResistance
        {
            get
            {
                var sum = 0f;
                CircuitParts.ForEach(p => sum += 1 / p.ResistanceValue);
                return 1 / sum;
            }
        }

        public void AddCircuitPart(CircuitPart part)
        {
            CircuitParts.Add(part);
        }

        public CircuitPart GetEquivalentCircuitPart()
        {
            var equivalentCircuitPart = new CircuitPart(Start)
            {
                End = End
            };

            CircuitParts.ForEach(c =>
            {
                var nodes = c.Drawables;
                foreach (var node in nodes)
                {
                    equivalentCircuitPart.AddDrawableBase(node);
                }

                equivalentCircuitPart.ChildCircuitParts.Add(c);
            });

            equivalentCircuitPart.AddResistance(EquivalentResistance);
            return equivalentCircuitPart;
        }

        public bool ContainsNode(CircuitNode node)
        {
            var returnValue = false;
            foreach (var part in CircuitParts)
            {
                returnValue = (Start == node) || (End == node);
            }

            return returnValue;
        }

        public bool ResistancesInGroupAreEqual()
        {
            var firstResistance = CircuitParts[0].ResistanceValue;
            foreach(var part in CircuitParts)
            {
                if (part.ResistanceValue != firstResistance)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// If one of the parts of the group has 0 resistance, it will be returned
        /// </summary>
        /// <returns></returns>
        public CircuitPart GetZeroResistancePart()
        {
            var partList = new List<CircuitPart>();
            foreach(var part in CircuitParts)
            {
                if (part.ResistanceValue == 0)
                    partList.Add(part);
            }

            partList = partList.OrderBy(p => p.Drawables.Count).ToList();

            return (partList.Count > 0) == true ? partList[0] : null;
        }

        public bool ContainsDrawable(DrawableBase drawable)
        {
            var returnValue = false;
            foreach (var part in CircuitParts)
            {
                returnValue = part.ContainsDrawable(drawable);
                if (returnValue)
                    return true;
            }

            return returnValue;
        }

        public CircuitPart GetParentCircuitPartInGroup(DrawableBase drawable)
        {
            foreach (var part in CircuitParts)
            {
                if (part.ContainsDrawable(drawable))
                    return part;
            }

            return null;
        }

        public void DrawDebug(bool value)
        {
            CircuitParts.ForEach(c =>
            {
                var nodes = c.GetDrawablesRecursive();
                foreach (var node in nodes)
                {
                    if (node.Type == DrawableBase.DrawableType.Wire)
                        node.DebugColor = value == true ? DebugColor : Color.white;
                }
            });
        }

        public override string ToString()
        {
            return string.Format("Start: {0} | End: {1} | CircuitPartCount: {2} | Resistance: {3} | IterationLevel: {4}",
                                    Start,
                                    End,
                                    CircuitParts.Count,
                                    EquivalentResistance,
                                    IterationLevel);
        }
    }
}
