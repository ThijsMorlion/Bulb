using UnityEngine;
using UnityEngine.EventSystems;

namespace Settings.PerformanceStats
{
    public class PerformanceStatsPanelDrag : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private Vector2 _startPiecePositionOnInput;
        private Vector2 _startMousePositionOnInput;
        private bool _isDragging;

        private void Update()
        {
            if (_isDragging)
            {
                gameObject.GetComponent<RectTransform>().anchoredPosition = _startPiecePositionOnInput - _startMousePositionOnInput + GetMousePosition();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isDragging = true;
            _startMousePositionOnInput = GetMousePosition();
            _startPiecePositionOnInput = gameObject.GetComponent<RectTransform>().anchoredPosition;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isDragging = false;
        }

        public Vector2 GetMousePosition()
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(FindObjectOfType<Canvas>().transform as RectTransform, Input.mousePosition, FindObjectOfType<Canvas>().worldCamera, out pos);
            return pos;
        }
    }
}