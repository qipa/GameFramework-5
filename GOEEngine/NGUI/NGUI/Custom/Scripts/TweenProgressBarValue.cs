//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Tween the object's alpha.
/// </summary>

[AddComponentMenu("NGUI/Tween/Tween ProgressBar Value")]
public class TweenProgressBarValue : UITweener
{
#if UNITY_3_5
    public float from = 1f;
    public float to = 1f;
#else
    [Range(0f, 1f)]
    public float from = 1f;
    [Range(0f, 1f)]
    public float to = 1f;
#endif

    UIProgressBar mRect;

    public UIProgressBar CachedProgressBar
    {
        get
        {
            if (mRect == null)
            {
                mRect = GetComponent<UIProgressBar>();
                if (mRect == null) mRect = GetComponentInChildren<UIProgressBar>();
            }
            return mRect;
        }
    }

    /// <summary>
    /// Tween's current value.
    /// </summary>

    public float Value { get { return CachedProgressBar.value; } set { CachedProgressBar.value = value; } }

    /// <summary>
    /// Tween the value.
    /// </summary>

    protected override void OnUpdate(float factor, bool isFinished) { Value = Mathf.Lerp(from, to, factor); }

    /// <summary>
    /// Start the tweening operation.
    /// </summary>

    static public TweenProgressBarValue Begin(GameObject go, float duration, float from, float to, float startDelay = 0)
    {
        TweenProgressBarValue comp = UITweener.Begin<TweenProgressBarValue>(go, duration);
        comp.from = from;
        comp.to = to;
        comp.delay = startDelay;

        if (duration <= 0f)
        {
            comp.Sample(1f, true);
            comp.enabled = false;
        }
        return comp;
    }

    public override void SetStartToCurrentValue() { from = Value; }
    public override void SetEndToCurrentValue() { to = Value; }
}
