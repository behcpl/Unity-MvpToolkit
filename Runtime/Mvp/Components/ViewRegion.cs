using System;
using Behc.Utils;
using UnityEngine;

namespace Behc.Mvp.Components
{
    public class ViewRegion : MonoBehaviour
    {
        public Rect ClipRect
        {
            get
            {
                UpdateRects();
                return _intersectionRect;
            }
        }

        private Rect _worldRect = Rect.zero;
        private Rect _intersectionRect = Rect.zero;
        private ViewRegion _parentRegion;

        private void OnTransformParentChanged()
        {
            UpdateParentRegion();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(new Vector3(_worldRect.xMin, _worldRect.yMin), new Vector3(_worldRect.xMax, _worldRect.yMin));
            Gizmos.DrawLine(new Vector3(_worldRect.xMax, _worldRect.yMin), new Vector3(_worldRect.xMax, _worldRect.yMax));
            Gizmos.DrawLine(new Vector3(_worldRect.xMax, _worldRect.yMax), new Vector3(_worldRect.xMin, _worldRect.yMax));
            Gizmos.DrawLine(new Vector3(_worldRect.xMin, _worldRect.yMax), new Vector3(_worldRect.xMin, _worldRect.yMin));

            Gizmos.color = _intersectionRect.width > 0 && _intersectionRect.height > 0 ? Color.green : Color.red;
            Gizmos.DrawLine(new Vector3(_intersectionRect.xMin, _intersectionRect.yMin), new Vector3(_intersectionRect.xMax, _intersectionRect.yMin));
            Gizmos.DrawLine(new Vector3(_intersectionRect.xMax, _intersectionRect.yMin), new Vector3(_intersectionRect.xMax, _intersectionRect.yMax));
            Gizmos.DrawLine(new Vector3(_intersectionRect.xMax, _intersectionRect.yMax), new Vector3(_intersectionRect.xMin, _intersectionRect.yMax));
            Gizmos.DrawLine(new Vector3(_intersectionRect.xMin, _intersectionRect.yMax), new Vector3(_intersectionRect.xMin, _intersectionRect.yMin));
        }
    
        private void UpdateRects()
        {
            RectTransform rt = (RectTransform) transform;
            
            Rect rect = rt.rect;

            Vector3 lMin = new Vector3(rect.xMin, rect.yMin, 0);
            Vector3 lMax = new Vector3(rect.xMax, rect.yMax, 0);
            
            Vector3 wMin = rt.TransformPoint(lMin);
            Vector3 wMax = rt.TransformPoint(lMax);

            _worldRect = Rect.MinMaxRect(wMin.x, wMin.y, wMax.x, wMax.y);

            if (_parentRegion.IsNotNull())
            {
                _parentRegion.UpdateRects();
                _intersectionRect = Rect.MinMaxRect(
                    Math.Max(_worldRect.xMin, _parentRegion._intersectionRect.xMin),
                    Math.Max(_worldRect.yMin, _parentRegion._intersectionRect.yMin),
                    Math.Min(_worldRect.xMax, _parentRegion._intersectionRect.xMax),
                    Math.Min(_worldRect.yMax, _parentRegion._intersectionRect.yMax));
            }
            else
            {
                _intersectionRect = _worldRect;
            }
        }

        private void UpdateParentRegion()
        {
            _parentRegion = null;
            Transform tr = transform.parent;
            while (tr != null)
            {
                _parentRegion = tr.GetComponent<ViewRegion>();
                if (_parentRegion != null)
                {
                    _parentRegion.UpdateParentRegion();
                    break;
                }

                tr = tr.parent;
            }
        }
    }
}