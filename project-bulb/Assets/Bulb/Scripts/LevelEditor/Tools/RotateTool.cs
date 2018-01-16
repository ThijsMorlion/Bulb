using Bulb.Characters;
using Bulb.Controllers;
using Bulb.Visuals.Grid;

namespace Bulb.LevelEditor.Tools
{
    public static class RotateTool
    {
        private static BulbGrid _grid;
        private static SelectionController _selectionController;

        public static void Init(BulbGrid grid)
        {
            _grid = grid;
            _selectionController = ApplicationController.Instance.SelectionController;
        }

        public static void RotateLeft(DrawableBase drawable)
        {
            Rotate(drawable, 90f);
        }

        public static void RotateRight(DrawableBase drawable)
        {
            Rotate(drawable, -90f);
        }

        private static void Rotate(DrawableBase drawable, float angle)
        {
            if (_grid.TryRotate(drawable, angle))
            {
                _selectionController.ClearSelection();

                drawable.RotateAroundPivot(angle);
                _grid.SetGridCells(drawable.transform.position, drawable);

                if (drawable.GetType().IsSubclassOf(typeof(CharacterBase)))
                {
                    var charBase = (CharacterBase)drawable;
                    charBase.RemoveAllConnections();
                    charBase.UpdateConnectionPoints();
                }

                _selectionController.AddToSelection(drawable);
            }
        }
    }
}