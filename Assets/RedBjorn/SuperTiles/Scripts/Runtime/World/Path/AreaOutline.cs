using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RedBjorn.SuperTiles.Utils
{
    /// <summary>
    /// Colored wrapper of LineDrawer
    /// </summary>
    public class AreaOutline : MonoBehaviour
    {
        public LineDrawer Line;
        public Color ActiveColor;
        public Color InactiveColor;

        public void ActiveState()
        {
            SetColor(ActiveColor);
        }

        public void InactiveState()
        {
            SetColor(InactiveColor);
        }

        void SetColor(Color color)
        {
            Line.Line.material.color = color;
        }

        public void Show(List<Vector3> points)
        {
            Line.Show(points.Select(p => new Vector3(p.x, p.z, 0f)).ToArray());
        }

        public void Hide()
        {
            Line.Hide();
        }
    }
}
