using Bulb.Visuals.Grid;
using UnityEngine;
using UnityEngine.UI;

namespace Bulb.LevelEditor.Widgets
{
    public class GridModeToggleGroup : MonoBehaviour
    {
        public Toggle SelectionMode, MoveMode, WireMode;

        public delegate void ToggleValueChanged(BulbGrid.GridMode mode);
        public ToggleValueChanged OnToggleValueChanged;

        private BulbGrid.GridMode _previousMode = BulbGrid.GridMode.None;

        private void Awake()
        {
            ToggleModeChanged();
        }

        public BulbGrid.GridMode CurrentDrawMode
        {
            get
            {
                if (SelectionMode.isOn)
                    return BulbGrid.GridMode.Selection;
                else if (WireMode.isOn)
                    return BulbGrid.GridMode.DrawWire;
                else if (MoveMode.isOn)
                    return BulbGrid.GridMode.Move;

                return BulbGrid.GridMode.None;
            }
        }

        public void ToggleModeChanged()
        {
            if(_previousMode != CurrentDrawMode)
            {
                if (OnToggleValueChanged != null)
                    OnToggleValueChanged(CurrentDrawMode);

                _previousMode = CurrentDrawMode;
            }
        }

        public void SetInteractable(bool value)
        {
            SetModeColor(SelectionMode, value);
            SetModeColor(MoveMode, value);
            SetModeColor(WireMode, value);
        }

        private void SetModeColor(Toggle mode, bool isInteractable)
        {
            var currColor = mode.GetComponent<RectTransform>().GetChild(0).GetComponent<Image>().color;
            currColor.a = isInteractable == false ? .5f : 1f;

            mode.GetComponent<RectTransform>().GetChild(0).GetComponent<Image>().color = currColor;
            mode.interactable = isInteractable;
        }
    }
}