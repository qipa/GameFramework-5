//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Tween the object's position.
/// </summary>

[AddComponentMenu("NGUI/Tween/Tween PositionExt")]
public class TweenPositionEx : UITweener
{
    public float xFrom, yFrom;
    public float xTo, yTo;

    public bool hasX, hasY;

    [HideInInspector]
    public bool worldSpace = false;

    public AnimationCurve animCurv2;

    Transform mTrans;
    UIRect mRect;

    public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }


    void Awake() { mRect = GetComponent<UIRect>(); }

    /// <summary>
    /// Tween the value.
    /// </summary>

    protected override void OnUpdate(float factor, bool isFinished)
    {
        float x = 0, y = 0;
        if (hasX)
        {
            x = xFrom * (1 - factor) + xTo * factor;
        }
        if (hasY && animCurv2 != null)
        {
            float fy = animCurv2.Evaluate(tweenFactor);
            y = yFrom * (1 - fy) + yTo * fy;
        }
        if (hasX || hasY)
        {
            if (mRect == null || !mRect.isAnchored || worldSpace)
            {
                if (worldSpace)
                {
                    Vector3 pos = cachedTransform.position;
                    if (hasX)
                        pos.x = x;
                    if (hasY)
                        pos.y = y;
                    cachedTransform.position = pos;
                }
                else
                {
                    Vector3 pos = cachedTransform.localPosition;
                    if (hasX)
                        pos.x = x;
                    if (hasY)
                        pos.y = y;
                    cachedTransform.localPosition = pos;
                }
            }
            else
            {
                x -= cachedTransform.localPosition.x;
                y -= cachedTransform.localPosition.y;
                NGUIMath.MoveRect(mRect, x, y);
            }
        }
        //value = from * (1f - factor) + to * factor;
    }

}
