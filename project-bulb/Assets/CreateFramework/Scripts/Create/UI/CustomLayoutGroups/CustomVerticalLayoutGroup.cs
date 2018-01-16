using UnityEngine;

namespace Create.UI.CustomLayoutGroups
{
    /// <summary>
    /// Basic vertical layout group that caches child info, to prevent expensive layout recalculations done by the native vertical layout group. 
    /// The native vertical group needs to do a GetComponent<> on all nested children before calculating the layout, creating a large garbage buildup.
    /// </summary>
    public class CustomVerticalLayoutGroup : CustomLayoutBase
    {
        protected override void UpdateLayout()
        {
            _previousSpacing = Spacing;

            if (transform.childCount == 0)
            {
                if (SizeToContent && _layoutElement != null)
                {
                    _layoutElement.preferredHeight = 0;
                }
                return;
            }

            Vector2 cumulativePosition = new Vector2(Padding.left, Padding.bottom * LayoutScalar);
            for (int i = 0; i < transform.childCount; i++)
            {
                // Update previous size.
                _previousSizes[i] = _childLayoutElements[i] == null ? 0 : _childLayoutElements[i].preferredHeight;

                if (_childRects[i] == null)
                    continue;

                // Update child position.
                _childRects[i].anchoredPosition = cumulativePosition;
                cumulativePosition += new Vector2(0, (_previousSizes[i] + (i < transform.childCount - 1 ? Spacing : 0)) * LayoutScalar);
            }

            // Update own (preferred) size.
            if (SizeToContent)
            {
                if (_layoutElement != null)
                {
                    _layoutElement.preferredHeight = cumulativePosition.y;
                }
                else if(_rect != null)
                {
                    _rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cumulativePosition.y);
                }
            }
        }
    }
}