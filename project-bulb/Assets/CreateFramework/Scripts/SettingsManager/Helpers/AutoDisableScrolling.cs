using UnityEngine;
using UnityEngine.UI;

namespace Settings.Helpers
{
    [RequireComponent(typeof(ScrollRect))]
    public class AutoDisableScrolling : MonoBehaviour
    {
        private ScrollRect _scrollRect;
        private LayoutGroup _contentLayoutGroup;
        private bool _horizontal, _vertical;

        void Start()
        {
            _scrollRect = GetComponent<ScrollRect>();
            if (_scrollRect == null)
            {
                Debug.LogWarningFormat("[{0}] No {1} found on {2}.", GetType(), typeof(ScrollRect), name);
            }
            else
            {
                _horizontal = _scrollRect.horizontal;
                _vertical = _scrollRect.vertical;
                if (_scrollRect.content != null)
                {
                    _contentLayoutGroup = _scrollRect.content.GetComponent<LayoutGroup>();
                    if (_contentLayoutGroup == null)
                    {
                        Debug.LogWarningFormat("[{0}] No layout group found on content {1}.", GetType(), _scrollRect.content.name);
                    }
                }
                else
                {
                    Debug.LogWarningFormat("[{0}] No content assigned on {1}.", GetType(), name);
                }
            }
        }

        void Update()
        {
            if (_scrollRect == null || _scrollRect.content == null || _scrollRect.viewport == null || _contentLayoutGroup == null)
                return;

            if (_horizontal)
            {
                _scrollRect.horizontal = _contentLayoutGroup.preferredWidth > _scrollRect.viewport.rect.width;
            }
            if (_vertical)
            {
                _scrollRect.vertical = _contentLayoutGroup.preferredHeight > _scrollRect.viewport.rect.height;
            }
        }
    }
}