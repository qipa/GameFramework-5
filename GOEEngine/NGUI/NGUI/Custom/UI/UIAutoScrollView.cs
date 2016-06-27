using System;
using UnityEngine;

public class UIAutoScrollView:UIScrollView
{
    private bool Inited = false;
    public override void UpdateScrollbars(bool recalculateBounds)
    {
        if (!Inited)
        {
            verticalScrollBar.alpha = 0;
            horizontalScrollBar.alpha = 0;
            Inited = true;
        }
        base.UpdateScrollbars(recalculateBounds);
        if (!recalculateBounds)
            return;
        if (verticalScrollBar != null)
        {
            verticalScrollBar.foregroundWidget.height = Mathf.CeilToInt(panel.height - 10);
            verticalScrollBar.backgroundWidget.height = Mathf.CeilToInt(panel.height);
            verticalScrollBar.transform.localPosition = new Vector3(panel.width - verticalScrollBar.backgroundWidget.width, 0);
        }
        if (horizontalScrollBar != null)
        {
            horizontalScrollBar.foregroundWidget.width = Mathf.CeilToInt(panel.width - 10);
            horizontalScrollBar.backgroundWidget.width = Mathf.CeilToInt(panel.width);
            verticalScrollBar.transform.localPosition = new Vector3(0,panel.height - verticalScrollBar.backgroundWidget.height, 0);
        }
        if (shouldMoveHorizontally)
        {
            movement = shouldMoveVertically ? Movement.Unrestricted : Movement.Horizontal;
        }
        else if (shouldMoveVertically)
            movement = Movement.Vertical;
    }
}
