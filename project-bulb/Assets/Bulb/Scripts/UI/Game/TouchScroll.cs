using System.Collections;
using System.Collections.Generic;
using TouchScript.Gestures.TransformGestures;
using UnityEngine;
using UnityEngine.UI;

namespace Bulb.UI
{
    public class TouchScroll : ScrollRect
    {

        private ScreenTransformGesture _gesture;
        private bool _isDragging;
        private Vector2 offset = new Vector2(0, 0);
        private Vector2 _prevPosition = Vector2.zero;
        private new Bounds _prevContentBounds;
        private new Bounds _prevViewBounds;
        private new Bounds _viewBounds;

        protected override void OnEnable()
        {
            base.OnEnable();
            _gesture = GetComponent<ScreenTransformGesture>();
            if (_gesture != null)
            {
                _gesture.Transformed += Gesture_Transformed;
                _gesture.TransformCompleted += Gesture_TransformCompleted;
                _gesture.TransformStarted += Gesture_TransformStarted;
            }
        }

        private void Gesture_TransformStarted(object sender, System.EventArgs e)
        {
            _isDragging = true;
            offset = Vector2.zero;
        }

        private void Gesture_TransformCompleted(object sender, System.EventArgs e)
        {
            _isDragging = false;
        }

        private void Gesture_Transformed(object sender, System.EventArgs e)
        {
            offset = new Vector2(_gesture.DeltaPosition.x / Screen.width, _gesture.DeltaPosition.y / Screen.height);
            verticalNormalizedPosition -= _gesture.DeltaPosition.y / Screen.height;
            if (verticalNormalizedPosition < 0) verticalNormalizedPosition = 0;
            else if (verticalNormalizedPosition > 1) verticalNormalizedPosition = 1;

            /*if (movementType == MovementType.Elastic)
            {
                if (offset.y != 0)
                    verticalNormalizedPosition = verticalNormalizedPosition + RubberDelta(offset.y, viewRect.rect.size.y);
            }*/

            if (_isDragging && inertia)
            {
                Vector2 newVelocity = new Vector2(0, (_gesture.DeltaPosition.y / Screen.height) / Time.unscaledDeltaTime);
                velocity = Vector2.Lerp(velocity, newVelocity, Time.unscaledDeltaTime * 10);
            }

            onValueChanged.Invoke(normalizedPosition);

        }

        private float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }

        public override void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
        {
            //base.OnDrag ();
        }

        public override void OnScroll(UnityEngine.EventSystems.PointerEventData data)
        {
            //base.OnScroll (data);
        }

        public override void OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
        {
            //base.OnBeginDrag (eventData);
        }

        public override void OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
        {
            //base.OnEndDrag (eventData);
        }

        protected override void LateUpdate()
        {
            if (!content)
                return;

            float deltaTime = Time.unscaledDeltaTime;
            if (!_isDragging && (offset != Vector2.zero || velocity != Vector2.zero))
            {
                Vector2 position = content.anchoredPosition;
                float speed = velocity.y;
                // Apply spring physics if movement is elastic and content has an offset from the view.
                if (movementType == MovementType.Elastic && offset.y != 0)
                {
                    position.y = Mathf.SmoothDamp(content.anchoredPosition.y, content.anchoredPosition.y + offset.y, ref speed, elasticity, Mathf.Infinity, deltaTime);
                    velocity = new Vector2(velocity.x, speed);
                }
                // Else move content according to velocity with deceleration applied.
                else if (inertia)
                {
                    speed *= Mathf.Pow(decelerationRate, deltaTime);
                    if (Mathf.Abs(velocity.y) < 1)
                        speed = 0;
                    velocity = new Vector2(velocity.x, speed);
                    position.y += velocity.y * deltaTime;
                }
                // If we have neither elaticity or friction, there shouldn't be any velocity.
                else
                {
                    //print("no velocity");
                    speed = 0;
                    velocity = new Vector2(velocity.x, speed);
                }


                if (velocity != Vector2.zero)
                {
                    if (movementType == MovementType.Clamped)
                    {
                        offset = CalculateOffset(position - content.anchoredPosition);
                        position += offset;
                    }

                    SetContentAnchoredPosition(position);
                }
            }

            if (_isDragging && inertia)
            {
                Vector3 newVelocity = (content.anchoredPosition - _prevPosition) / deltaTime;
                velocity = Vector3.Lerp(velocity, newVelocity, deltaTime * 10);
            }

            if (_viewBounds != _prevViewBounds || m_ContentBounds != _prevContentBounds || content.anchoredPosition != _prevPosition)
            {
                //UpdateScrollbars(offset);
                //onValueChanged.Invoke(normalizedPosition);
                UpdatePrevData2();
            }

            //base.LateUpdate();
        }

        public IEnumerator SetAnchoredPosition(Vector2 pos)
        {
            yield return new WaitForEndOfFrame();

            SetAnchoredPosition2D(pos);
        }

        public void SetAnchoredPosition2D(Vector2 pos)
        {
            var contentHeight = content.offsetMax.y;
            pos.y = Mathf.Clamp(pos.y, 0, 4096);

            SetContentAnchoredPosition(pos);
        }

        private void UpdatePrevData2()
        {
            if (content == null)
                _prevPosition = Vector2.zero;
            else
                _prevPosition = content.anchoredPosition;
            _prevViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            _prevContentBounds = m_ContentBounds;
        }

        private Vector2 CalculateOffset(Vector2 delta)
        {
            Vector2 offset = Vector2.zero;
            if (movementType == MovementType.Unrestricted)
                return offset;

            Vector2 min = m_ContentBounds.min;
            Vector2 max = m_ContentBounds.max;

            Bounds viewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            if (horizontal)
            {
                min.x += delta.x;
                max.x += delta.x;
                if (min.x > viewBounds.min.x)
                    offset.x = viewBounds.min.x - min.x;
                else if (max.x < viewBounds.max.x)
                    offset.x = viewBounds.max.x - max.x;
            }

            if (vertical)
            {
                min.y += delta.y;
                max.y += delta.y;
                if (max.y < viewBounds.max.y)
                    offset.y = viewBounds.max.y - max.y;
                else if (min.y > viewBounds.min.y)
                    offset.y = viewBounds.min.y - min.y;
            }

            return offset;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_gesture != null)
            {
                _gesture.Transformed -= Gesture_Transformed;
                _gesture.TransformCompleted -= Gesture_TransformCompleted;
                _gesture.TransformStarted -= Gesture_TransformStarted;
            }
        }
    }
}