using RedBjorn.ProtoTiles;
using System.Collections.Generic;
using UnityEngine;

namespace RedBjorn.SuperTiles.Utils
{
    /// <summary>
    /// Path drawer which use LineDrawer
    /// </summary>
    public class PathDrawer : MonoBehaviour
    {
        public LineDrawer Line;
        public Material MaterialActive;
        public Material MaterialInactive;

        GameObject Tail;
        MeshRenderer TailRenderer;
        MapEntity Map;

        public bool IsEnabled { get; set; }

        public void Init(MapEntity map)
        {
            Map = map;
            Tail = Map.TileCreate(true, false, 0.2f, inner: MaterialInactive);
            Tail.transform.SetParent(transform);
            TailRenderer = Tail.GetComponent<MeshRenderer>();
            Tail.SetActive(false);
        }

        public void ActiveState()
        {
            if (TailRenderer)
            {
                TailRenderer.material = MaterialActive;
            }
            Line.Line.material = MaterialActive;
        }

        public void InactiveState()
        {
            if (TailRenderer)
            {
                TailRenderer.material = MaterialInactive;
            }
            Line.Line.material = MaterialInactive;
        }

        public void Show(List<Vector3> points)
        {
            if (points == null || points.Count == 0)
            {
                Hide();
            }
            else
            {
                var tailPos = points[points.Count - 1];
                Tail.transform.localPosition = tailPos;
                Tail.SetActive(true);
                if (points.Count > 1)
                {
                    points[points.Count - 1] = (points[points.Count - 1] + points[points.Count - 2]) / 2f;
                    Line.Show(points);
                }
            }
        }

        public void Hide()
        {
            Line.Hide();
            if (Tail)
            {
                Tail.SetActive(false);
            }
        }
    }
}