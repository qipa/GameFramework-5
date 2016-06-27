using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class CurveUtility
{
	public static void ClearCurve(AnimationCurve p)  // 清除curve数据
	{
        if (p != null)
        {
            while (p.length > 0)
            {
                p.RemoveKey(0);
            }
        }
    }

    public static void CopyCurve(AnimationCurve lp, AnimationCurve rp) // 拷贝curve数据从右参至左参
	{
        ClearCurve(lp);
        for (int i = 0; i < rp.length; ++i)
        {
            lp.AddKey(rp[i]);
        }
    }


    public static void CurveMoveShift(AnimationCurve acx, float shift_x) // 将当前curve转换坐标
    {
        Keyframe[] tempkey = new Keyframe[acx.keys.Length];
        for (int i = 0; i < acx.keys.Length; ++i)
        {
            tempkey[i] = acx.keys[i];
            tempkey[i].value += shift_x;
        }
        ClearCurve(acx);
        foreach (Keyframe kf in tempkey)
        {
            acx.AddKey(kf);
        }
    }

    public static void ModifyCurveKey(AnimationCurve curve, float time, float value)
    {
        for (int i = 0; i < curve.length; i++)
        {
            if (curve[i].time == time)
            {
                Keyframe oldKeyframe = curve[i];
                curve.MoveKey(i, new Keyframe(time, value, oldKeyframe.inTangent, oldKeyframe.outTangent));
                break;
            }
        }
    }

    public static void RemoveKey(AnimationCurve curve, float time)
    {
        for (int i = 0; i < curve.length; i++)
        {
            if (curve[i].time == time)
            {
                curve.RemoveKey(i);
                break;
            }
        }
    }

    public static bool IsKeyFrame(AnimationCurve curve, float time)
    {
        foreach (Keyframe kf in curve.keys)
        {
            if (Mathf.Abs(kf.time - time) < 0.01)
                return true;
        }
        return false;
    }

    public static float GetEndTime(AnimationCurve curve)
    {
        return curve[curve.length - 1].time;
    }
}

