using System.Collections.Generic;
using Bulb.Characters;

namespace Bulb.Electricity
{
    public class Circuit
    {
        private HashSet<DrawableBase> _drawables = new HashSet<DrawableBase>();

        public bool AddDrawable(DrawableBase drawable)
        {
            return _drawables.Add(drawable);
        }
    }
}