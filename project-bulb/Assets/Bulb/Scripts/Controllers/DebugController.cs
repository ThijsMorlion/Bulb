using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Bulb.Controllers
{
    public class DebugController : MonoBehaviour
    {
        public RectTransform DebugPointPrefab;
        public bool IsDebugMode;

        private CanvasController _canvasController;
        private Dictionary<string, RectTransform> _debugPoints = new Dictionary<string, RectTransform>();
        private List<RectTransform> _debugPointsPool = new List<RectTransform>();

        private int _initialPoolSize = 10;
        private int _growFactor = 5;

        public void Awake()
        {
            _canvasController = ApplicationController.Instance.CanvasController;

            if (_canvasController)
                GrowPool(_initialPoolSize);
        }

        public void Update()
        {
            if (!IsDebugMode)
            {
                foreach (var debugPoint in _debugPoints)
                {
                    DeleteDebugPoint(debugPoint.Key);
                }

                _debugPoints.Clear();
            }
        }

        public void DrawDebugPoint(string id, Vector2 pos, Color color)
        {
            RectTransform debugPoint = null;
            if (_debugPoints.ContainsKey(id))
            {
                debugPoint = _debugPoints[id];
            }
            else
            {
                debugPoint = GetFreeDebugPoint();
                _debugPoints.Add(id, debugPoint);
            }

            if (debugPoint != null)
            {
                debugPoint.gameObject.SetActive(true);
                debugPoint.position = pos;
                debugPoint.SetAsLastSibling();
                debugPoint.GetComponent<Image>().color = color;
            }
            else
            {
                Debug.LogFormat("{0} | No debugpoint found and pool did not return valid debugpoint!", this);
            }
        }

        public void DeleteDebugPoint(string id)
        {
            if (_debugPoints.ContainsKey(id))
            {
                var debugPoint = _debugPoints[id];
                debugPoint.gameObject.SetActive(false);
            }
        }

        public RectTransform GetFreeDebugPoint()
        {
            var debugPoint = _debugPointsPool.FirstOrDefault(d => d.gameObject.activeSelf == false);
            if (debugPoint != null)
                return debugPoint.GetComponent<RectTransform>();

            GrowPool(_growFactor);
            return GetFreeDebugPoint();
        }

        private void GrowPool(int factor)
        {
            for (var i = 0; i < factor; ++i)
            {
                var newDebugPoint = Instantiate(DebugPointPrefab, _canvasController.DebugContainer, false);
                newDebugPoint.position = Vector2.zero;
                newDebugPoint.gameObject.SetActive(false);

                _debugPointsPool.Add(newDebugPoint);
            }
        }
    }
}