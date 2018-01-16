using Bulb.Characters;
using Bulb.Controllers;
using Bulb.Visuals.Grid;
using TouchScript.Gestures.TransformGestures;
using UnityEngine;
using UnityEngine.UI;

namespace Bulb.LevelEditor.Widgets
{
    [RequireComponent(typeof(TransformGesture))]
    public class CharacterWidgetItem : MonoBehaviour
    {
        public CharacterBase CharacterPrefab;

        protected TransformGesture _transformGesture;
        protected bool _canBePlaced = false;

        private CharacterBase _characterInstance = null;
        private BulbGrid _grid;

        private void Awake()
        {
            _transformGesture = GetComponent<TransformGesture>();
            _transformGesture.TransformStarted += _transformGesture_TransformStarted;
            _transformGesture.Transformed += _transformGesture_Transformed;
            _transformGesture.TransformCompleted += _transformGesture_TransformCompleted;

            _grid = FindObjectOfType<BulbGrid>();
        }

        private void OnDestroy()
        {
            _transformGesture.TransformStarted -= _transformGesture_TransformStarted;
            _transformGesture.Transformed -= _transformGesture_Transformed;
            _transformGesture.TransformCompleted -= _transformGesture_TransformCompleted;
        }

        #region TRANSFORM EVENTS
        protected virtual void _transformGesture_TransformStarted(object sender, System.EventArgs e)
        {
            if (_characterInstance == null)
            {
                _characterInstance = Instantiate(CharacterPrefab.gameObject).GetComponent<CharacterBase>();
                _characterInstance.transform.SetParent(ApplicationController.Instance.CanvasController.CharacterContainer, false);
                _characterInstance.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(CharacterPrefab.GridSize.x * _grid.CellSize, CharacterPrefab.GridSize.y * _grid.CellSize);
                _canBePlaced = false;
            }

            var parentScrollRect = GetComponentInParent<ScrollRect>();
            if (parentScrollRect != null)
                parentScrollRect.vertical = false;
        }

        private void _transformGesture_Transformed(object sender, System.EventArgs e)
        {
            if (_characterInstance)
            {
                var snapPos = _transformGesture.ScreenPosition;
                _canBePlaced = _grid.TryGetSnapPosition(_transformGesture.ScreenPosition, out snapPos, CharacterPrefab);
                _characterInstance.gameObject.transform.position = snapPos;
                _characterInstance.Image.color = _canBePlaced == true ? Color.white : Color.red;
            }
        }

        protected virtual void _transformGesture_TransformCompleted(object sender, System.EventArgs e)
        {
            var parentScrollRect = GetComponentInParent<ScrollRect>();
            if (parentScrollRect != null)
                parentScrollRect.vertical = true;

            if (_canBePlaced)
            {
                var snapPos = _transformGesture.ScreenPosition;
                _grid.TryGetSnapPosition(_transformGesture.ScreenPosition, out snapPos, CharacterPrefab);
                _grid.SetGridCells(snapPos, _characterInstance);

                var characterController = ApplicationController.Instance.CharacterController;
                switch (_characterInstance.Type)
                {
                    case DrawableBase.DrawableType.Bulb:
                        characterController.AddCharacter((LightBulbCharacter)_characterInstance, snapPos);
                        break;
                    case DrawableBase.DrawableType.Battery:
                        characterController.AddCharacter((BatteryCharacter)_characterInstance, snapPos);
                        break;
                    case DrawableBase.DrawableType.Obstruction:
                        characterController.AddCharacter((ObstructionCharacter)_characterInstance, snapPos);
                        break;
                    case DrawableBase.DrawableType.Motor:
                        characterController.AddCharacter((MotorCharacter)_characterInstance, snapPos);
                        break;
                    case DrawableBase.DrawableType.Buzzer:
                        characterController.AddCharacter((BuzzerCharacter)_characterInstance, snapPos);
                        break;
                    case DrawableBase.DrawableType.Switch:
                        characterController.AddCharacter((SwitchCharacter)_characterInstance, snapPos);
                        break;
                }
            }
            else
            {
                Destroy(_characterInstance.gameObject);
            }

            _characterInstance = null;
        }
        #endregion

        public void SetImage(Texture2D image)
        {
            var currImage = GetComponent<Image>();
            currImage.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.zero);
            currImage.preserveAspect = true;
        }
    }
}