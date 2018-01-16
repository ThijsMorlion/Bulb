using System.Collections.Generic;
using UnityEngine;

namespace Create.UI.Tweenables
{
    /// <summary>
    /// Applies delays to Tweenables at the first child level.
    /// </summary>
    public class ApplyIncrementalDelays : MonoBehaviour
    {
        public IncrementalDelaysDirection Direction;
        /// <summary>
        /// Delay added between each element.
        /// </summary>
        [Tooltip("Delay added between each element.")]
        public float IncrementalDelay = .05f;
        public float InitialDelay;
        /// <summary>
        /// If greater than zero, delays will be grouped for every x elements. Used to combine rows in a grid.
        /// </summary>
        [Tooltip("If greater than zero, delays will be grouped for every x elements. Used to combine rows in a grid.")]
        public int GroupByModulo;
        /// <summary>
        /// Whether only the root Tweenable in the child is to be affected, or all Tweenables within it.
        /// </summary>
        [Tooltip("Whether only the root Tweenable in the child is to be affected, or all Tweenables within it.")]
        public bool AffectAllTweenablesWithinChild = true;

        private IncrementalDelaysDirection _previousDirection;
        private float _previousIncrementalDelay, _previousInitialDelay;
        private int _previousGroupByModulo;
        private bool _previousAffectAllTweenablesWithinChild;

        void Start()
        {
            SetPreviousValues();

            ApplyDelays();
        }

        void OnValidate()
        {
            if (IncrementalDelay < 0)
            {
                IncrementalDelay = 0;
            }
            if (InitialDelay < 0)
            {
                InitialDelay = 0;
            }
            if (GroupByModulo < 0)
            {
                GroupByModulo = 0;
            }
        }

        void Update()
        {
            if(ValuesChanged())
            {
                ApplyDelays();
                SetPreviousValues();
            }
        }

        void OnTransformChildrenChanged()
        {
            ApplyDelays();
        }

        /// <summary>
        /// Delays are applied automatically when the child count of the transform changes.
        /// </summary>
        public void ApplyDelays()
        {
            var tweenables = new List<Tweenable[]>();
            foreach (Transform child in transform)
            {
                if (AffectAllTweenablesWithinChild)
                {
                    var tweenablesInChild = child.GetComponentsInChildren<Tweenable>();
                    if (tweenablesInChild != null)
                    {
                        tweenables.Add(tweenablesInChild);
                    }
                }
                else
                {
                    var tweenable = child.GetComponent<Tweenable>();
                    if (tweenable != null)
                    {
                        tweenables.Add(new Tweenable[] { tweenable });
                    }
                }
            }

            if (tweenables.Count == 0)
                return;

            ApplyDelaysByDirection(tweenables);
        }

        private void ApplyDelaysByDirection(List<Tweenable[]> tweenables)
        {
            if (Direction == IncrementalDelaysDirection.FirstToLast)
            {
                ApplyDelaysFirstToLast(tweenables);
            }
            else if (Direction == IncrementalDelaysDirection.LastToFirst)
            {
                ApplyDelaysLastToFirst(tweenables);
            }
            else if (Direction == IncrementalDelaysDirection.EdgeToCenter)
            {
                ApplyDelaysEdgeToCenter(tweenables);
            }
            else if (Direction == IncrementalDelaysDirection.CenterToEdge)
            {
                ApplyDelaysCenterToEdge(tweenables);
            }
        }

        private void ApplyDelaysFirstToLast(List<Tweenable[]> tweenablesPerChild)
        {
            for (int i = 0; i < tweenablesPerChild.Count; i++)
            {
                foreach (var tweenable in tweenablesPerChild[i])
                {
                    float cumulativeDelay = InitialDelay + i * IncrementalDelay;
                    if (GroupByModulo != 0)
                    {
                        cumulativeDelay = InitialDelay + (i % GroupByModulo) * IncrementalDelay;
                    }

                    tweenable.Delay = cumulativeDelay;
                }
            }
        }

        private void ApplyDelaysLastToFirst(List<Tweenable[]> tweenablesPerChild)
        {
            for (int i = tweenablesPerChild.Count - 1; i >= 0; i--)
            {
                foreach (var tweenable in tweenablesPerChild[i])
                {
                    float cumulativeDelay = InitialDelay + (tweenablesPerChild.Count - 1 - i) * IncrementalDelay;
                    if (GroupByModulo != 0)
                    {
                        cumulativeDelay = InitialDelay + ((ClosestGreaterMultiple(tweenablesPerChild.Count, GroupByModulo) - 1 - i) % GroupByModulo) * IncrementalDelay;
                    }

                    tweenable.Delay = cumulativeDelay;
                }
            }
        }

        private void ApplyDelaysEdgeToCenter(List<Tweenable[]> tweenablesPerChild)
        {
            if (GroupByModulo == 0)
            {
                ApplyDelaysEdgeToCenterWithoutGrouping(tweenablesPerChild);
            }
            else
            {
                ApplyDelaysEdgeToCenterWithGrouping(tweenablesPerChild);
            }
        }

        private void ApplyDelaysEdgeToCenterWithoutGrouping(List<Tweenable[]> tweenablesPerChild)
        {
            int b = 0, t = tweenablesPerChild.Count - 1;

            float cumulativeDelay = InitialDelay;
            for (int i = 0; i < tweenablesPerChild.Count; i++)
            {
                // Update tweenables from left edge.
                foreach (var tweenable in tweenablesPerChild[b++])
                {
                    tweenable.Delay = cumulativeDelay;
                }

                // Update tweenables from right edge.
                foreach (var tweenable in tweenablesPerChild[t--])
                {
                    tweenable.Delay = cumulativeDelay;
                }

                cumulativeDelay += IncrementalDelay;

                // Center crossed - break.
                if (b > t)
                    break;
            }
        }

        private void ApplyDelaysEdgeToCenterWithGrouping(List<Tweenable[]> tweenablesPerChild)
        {
            int startIndex = 0;
            while (startIndex < tweenablesPerChild.Count)
            {
                int b = startIndex, t = startIndex + GroupByModulo - 1;

                float cumulativeDelay = InitialDelay;
                for (int i = startIndex; i < startIndex + GroupByModulo; i++)
                {
                    // Update tweenables from left edge.
                    foreach (var tweenable in tweenablesPerChild[b++])
                    {
                        tweenable.Delay = cumulativeDelay;
                    }

                    // Updates tweenables from right edge.
                    if (t >= tweenablesPerChild.Count)
                    {
                        t--;
                    }
                    else
                    {
                        foreach (var tweenable in tweenablesPerChild[t--])
                        {
                            tweenable.Delay = cumulativeDelay;
                        }
                    }

                    cumulativeDelay += IncrementalDelay;

                    // Center crossed - break.
                    if (b >= tweenablesPerChild.Count || b > t)
                        break;
                }

                startIndex += GroupByModulo;
            }
        }

        private void ApplyDelaysCenterToEdge(List<Tweenable[]> tweenablesPerChild)
        {
            if(GroupByModulo == 0)
            {
                ApplyDelaysCenterToEdgeToGroup(tweenablesPerChild);
            }
            else
            {
                int startIndex = 0;
                while(startIndex < tweenablesPerChild.Count)
                {
                    ApplyDelaysCenterToEdgeToGroup(tweenablesPerChild.GetRange(startIndex, startIndex + GroupByModulo < tweenablesPerChild.Count ? GroupByModulo : tweenablesPerChild.Count - startIndex));
                    startIndex += GroupByModulo;
                }
            }
        }

        private void ApplyDelaysCenterToEdgeToGroup(List<Tweenable[]> tweenablesPerChild)
        {
            int groupCount = GroupByModulo > 0 ? GroupByModulo : tweenablesPerChild.Count;
            float center = ((float)groupCount - 1) / 2;

            // Bottom half.
            float cumulativeDelay = InitialDelay;
            for (int i = Mathf.FloorToInt(center); i >= 0; i--)
            {
                if (i >= tweenablesPerChild.Count)
                    continue;

                foreach (var tweenable in tweenablesPerChild[i])
                {
                    tweenable.Delay = cumulativeDelay;
                }

                cumulativeDelay += IncrementalDelay;
            }

            // Top half.
            cumulativeDelay = InitialDelay;
            for (int i = groupCount % 2 == 0 ? Mathf.CeilToInt(center) : Mathf.FloorToInt(center); i < groupCount; i++)
            {
                if (i >= tweenablesPerChild.Count)
                    break;

                foreach (var tweenable in tweenablesPerChild[i])
                {
                    tweenable.Delay = cumulativeDelay;
                }

                cumulativeDelay += IncrementalDelay;
            }
        }
    
        private int ClosestGreaterMultiple(int value, int multipleOf)
        {
            return Mathf.CeilToInt((float)value / multipleOf) * multipleOf;
        }

        private bool ValuesChanged()
        {
            return _previousAffectAllTweenablesWithinChild != AffectAllTweenablesWithinChild || _previousDirection != Direction || _previousGroupByModulo != GroupByModulo 
                || _previousIncrementalDelay != IncrementalDelay || _previousInitialDelay != InitialDelay;
        }

        private void SetPreviousValues()
        {
            _previousDirection = Direction;
            _previousIncrementalDelay = IncrementalDelay;
            _previousInitialDelay = InitialDelay;
            _previousGroupByModulo = GroupByModulo;
            _previousAffectAllTweenablesWithinChild = AffectAllTweenablesWithinChild;
        }
    }

    public enum IncrementalDelaysDirection
    {
        FirstToLast,
        LastToFirst,
        EdgeToCenter,
        CenterToEdge,
        Random
    }
}