using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIMyGrid : UIGrid
{
    public bool VariableChildSize = true;

    public void RepositionManualy()
    {
        ResetPosition(GetChildList());
    }

    protected override void ResetPosition(List<Transform> list)
    {
        if (!VariableChildSize)
        {
            base.ResetPosition(list);
            return;
        }
        mReposition = false;

        // Epic hack: Unparent all children so that we get to control the order in which they are re-added back in
        for (int i = 0, imax = list.Count; i < imax; ++i)
            list[i].parent = null;

        int x = 0;
        int y = 0;
        int maxX = 0;
        int maxY = 0;
        Transform myTrans = transform;
        Vector3 pos = Vector3.zero;
        // Re-add the children in the same order we have them in and position them accordingly
        for (int i = 0, imax = list.Count; i < imax; ++i)
        {
            Transform t = list[i];
            t.parent = myTrans;

            float depth = t.localPosition.z;
            if (animateSmoothly && Application.isPlaying)
            {
                SpringPosition.Begin(t.gameObject, pos, 15f).updateScrollView = true;
            }
            else
            {
                t.localPosition = pos;
            }

            maxX = Mathf.Max(maxX, (int)pos.x);
            maxY = Mathf.Min(maxY, (int)pos.y);

            if (++x >= maxPerLine && maxPerLine > 0)
            {
                x = 0;
                ++y;
            }

            UIWidget wid = t.GetComponentInChildren<UIMyGridItem>();
            if(wid != null)
            {
                if (arrangement == Arrangement.Horizontal)
                {
                    if (x > 0)
                        pos.x += wid.width;
                    if (y > 0)
                        pos.y -= wid.height;
                }
                else
                {
                    if (y > 0)
                        pos.x += wid.width;
                    if (x > 0)
                        pos.y -= wid.height;
                }
            }
        }

        // Apply the origin offset
        if (pivot != UIWidget.Pivot.TopLeft)
        {
            Vector2 po = NGUIMath.GetPivotOffset(pivot);

            float fx, fy;
            fx = Mathf.Lerp(0f, maxX, po.x);
            fy = Mathf.Lerp(maxY, 0f, po.y);

            for (int i = 0; i < myTrans.childCount; ++i)
            {
                Transform t = myTrans.GetChild(i);
                SpringPosition sp = t.GetComponent<SpringPosition>();

                if (sp != null)
                {
                    sp.target.x -= fx;
                    sp.target.y -= fy;
                }
                else
                {
                    Vector3 pos1 = t.localPosition;
                    pos1.x -= fx;
                    pos1.y -= fy;
                    t.localPosition = pos1;
                }
            }
        }
    }
}