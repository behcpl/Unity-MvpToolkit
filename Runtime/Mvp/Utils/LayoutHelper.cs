using Behc.Mvp.PopupMenuManager;
using UnityEngine;

namespace Behc.Mvp.Utils
{
    public static class LayoutHelper
    {
        public static void KeepInsideParentRect(RectTransform parentTm, RectTransform tm, Vector2 desiredPosition)
        {
            Rect parentRect = parentTm.rect;
            Rect tmRect = tm.rect;
            Vector2 tmPivot = tm.pivot;
            tmRect.position = desiredPosition - Vector2.Scale(tmPivot, tmRect.size);

            Vector2 adjust = Vector2.zero;
            if (tmRect.xMax > parentRect.xMax)
                adjust.x = -(tmRect.xMax - parentRect.xMax);
            if (tmRect.xMin < parentRect.xMin)
                adjust.x = -(tmRect.xMin - parentRect.xMin);
            if (tmRect.yMax > parentRect.yMax)
                adjust.y = -(tmRect.yMax - parentRect.yMax);
            if (tmRect.yMin < parentRect.yMin)
                adjust.y = -(tmRect.yMin - parentRect.yMin);

            Vector2 off = Vector2.Scale(parentTm.pivot, parentRect.size) - Vector2.Scale(tm.anchorMin, parentRect.size);

            tm.anchoredPosition = desiredPosition + adjust + off;
        }

        public static void AdjacentRect(RectTransform parentTm, RectTransform tm, Rect ownerRect, Vector2 separation, PopupMenuManagerPresenter.Alignment alignment)
        {
            //TODO: should direction be configurable? for now every layout (but TOP) tries to go down first

            Rect parentRect = parentTm.rect;
            Rect tmRect = tm.rect;
            Vector2 tmPivot = tm.pivot;

            if (alignment == PopupMenuManagerPresenter.Alignment.TOP || alignment == PopupMenuManagerPresenter.Alignment.BOTTOM)
            {
                if (alignment == PopupMenuManagerPresenter.Alignment.TOP)
                {
                    tmRect.position = new Vector2(ownerRect.xMin + separation.x, ownerRect.yMax + separation.y) - Vector2.Scale(tmPivot, tmRect.size);
                    if (tmRect.yMax > parentRect.yMax)
                        tmRect.position = new Vector2(ownerRect.xMin + separation.x, ownerRect.yMin - separation.y) - Vector2.Scale(tmPivot, tmRect.size);
                }
                else
                {
                    tmRect.position = new Vector2(ownerRect.xMin + separation.x, ownerRect.yMin - separation.y) - Vector2.Scale(tmPivot, tmRect.size);
                    if (tmRect.yMin < parentRect.yMin)
                        tmRect.position = new Vector2(ownerRect.xMin + separation.x, ownerRect.yMax + separation.y) - Vector2.Scale(tmPivot, tmRect.size);
                }
            }
            else
            {
                if (alignment == PopupMenuManagerPresenter.Alignment.LEFT)
                {
                    tmRect.position = new Vector2(ownerRect.xMin - tmRect.width - separation.x, ownerRect.yMax + separation.y) - Vector2.Scale(tmPivot, tmRect.size);
                    if (tmRect.xMin < parentRect.xMin)
                        tmRect.position = new Vector2(ownerRect.xMax + separation.x, ownerRect.yMax + separation.y) - Vector2.Scale(tmPivot, tmRect.size);
                }
                else
                {
                    tmRect.position = new Vector2(ownerRect.xMax + separation.x, ownerRect.yMax + separation.y) - Vector2.Scale(tmPivot, tmRect.size);
                    if (tmRect.xMax > parentRect.xMax)
                        tmRect.position = new Vector2(ownerRect.xMin - tmRect.width - separation.x, ownerRect.yMax + separation.y) - Vector2.Scale(tmPivot, tmRect.size);
                }

                if (tmRect.yMin < parentRect.yMin)
                    tmRect.y = ownerRect.yMin - separation.y + tmRect.height - tmPivot.y * tmRect.height;
            }

            Vector2 adjust = Vector2.zero;
            if (tmRect.xMax > parentRect.xMax)
                adjust.x = -(tmRect.xMax - parentRect.xMax);
            if (tmRect.xMin < parentRect.xMin)
                adjust.x = -(tmRect.xMin - parentRect.xMin);
            if (tmRect.yMax > parentRect.yMax)
                adjust.y = -(tmRect.yMax - parentRect.yMax);
            if (tmRect.yMin < parentRect.yMin)
                adjust.y = -(tmRect.yMin - parentRect.yMin);

            Vector2 off = Vector2.Scale(parentTm.pivot, parentRect.size) - Vector2.Scale(tm.anchorMin, parentRect.size);
            tm.anchoredPosition = tmRect.position + Vector2.Scale(tmPivot, tmRect.size) + adjust + off;
        }
    }
}