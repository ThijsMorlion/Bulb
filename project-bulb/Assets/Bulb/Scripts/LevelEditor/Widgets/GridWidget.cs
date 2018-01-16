using Bulb.Visuals.Grid;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bulb.LevelEditor.Widgets
{
    public class GridWidget : MonoBehaviour
    {
        public TMP_InputField RowsProperty;
        public TMP_InputField ColumnsProperty;
        public TMP_InputField CellSizeProperty;

        private BulbGrid _grid;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                var next = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

                if (next != null)
                {
                    var inputField = next.GetComponent<TMP_InputField>();
                    if (inputField != null)
                    {
                        inputField.OnPointerClick(new PointerEventData(EventSystem.current));
                    }
                }
            }
        }

        private void Awake()
        {
            _grid = FindObjectOfType<BulbGrid>();
            if (_grid == null)
                Debug.LogFormat("{0} | No grid object found in scene!", this);

            Init();

            RowsProperty.onValueChanged.AddListener(OnRowsPropertyChanged);
            ColumnsProperty.onValueChanged.AddListener(OnColumnsPropertyChanged);
            CellSizeProperty.onValueChanged.AddListener(OnCellSizePropertyChanged);

            _grid.OnGridInitialized += Init;
            _grid.OnGridChanged += Init;
        }

        private void OnDestroy()
        {
            RowsProperty.onValueChanged.RemoveAllListeners();
            ColumnsProperty.onValueChanged.RemoveAllListeners();
            CellSizeProperty.onValueChanged.RemoveAllListeners();
        }

        private void Init()
        {
            RowsProperty.text = _grid.NumberOfRows.ToString();
            ColumnsProperty.text = _grid.NumberOfColumns.ToString();
            CellSizeProperty.text = _grid.CellSize.ToString();
        }

        private void OnRowsPropertyChanged(string input)
        {
            int newRowValue = 0;
            if(int.TryParse(input, out newRowValue))
            {
                _grid.SetNumberOfRows(newRowValue);
            }
        }

        private void OnColumnsPropertyChanged(string input)
        {
            int newColumnValue = 0;
            if (int.TryParse(input, out newColumnValue))
            {
                _grid.SetNumberOfColumns(newColumnValue);
            }
        }

        private void OnCellSizePropertyChanged(string input)
        {
            int newCellSize = 0;
            if (int.TryParse(input, out newCellSize))
            {
                _grid.CellSize = newCellSize;
            }
        }
    }
}
