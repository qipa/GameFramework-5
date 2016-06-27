using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
    partial class GOECurvController
    {
        List<GOECurvKeyFrame> keyFrames = new List<GOECurvKeyFrame>();
        [JsonFieldAttribute(JsonFieldTypes.HasChildren)]
        public List<GOECurvKeyFrame> KeyFrames { get { return keyFrames; } }
        public GOECurvController()
        {
        }

        /// <summary>
        /// 根据时间查找关键帧Index
        /// </summary>
        /// <param name="t">时间</param>
        /// <param name="left">左方关键帧Idx</param>
        /// <param name="right">右方关键帧Idx</param>
        /// <returns>查找结果</returns>
        public GOECurveFindIndexResults FindIndexByTime(float t, out int left, out int right)
        {
            //0无关键帧，查找失败 1在所有帧之前 2在所有帧之后 3时间精确匹配某帧 4时间在两个关键帧之间
            //使用posx，假定其他长度相同//
            left = -1;
            right = -1;
            if (1 > keyFrames.Count)
            {
                return GOECurveFindIndexResults.FailedNoKeyFrame;
            }
            if (t < keyFrames[0].Time)
            {
                left = right = 0;
                return GOECurveFindIndexResults.BeforeAllKeyFrame;
            }
            if (t > keyFrames[keyFrames.Count - 1].Time)
            {
                left = right = keyFrames.Count - 1;
                return GOECurveFindIndexResults.AfterAllKeyFrame;
            }
            for (int i = 0; i < keyFrames.Count; ++i)
            {
                if (MathUtil.FloatEqual(keyFrames[i].Time,t))
                {
                    left = right = i;
                    return GOECurveFindIndexResults.ExactMatchFrame;
                }
                else if (keyFrames[i].Time > t)
                {
                    left = i - 1;
                    if (left < 0)
                        left = 0;
                    right = i;
                    return GOECurveFindIndexResults.Between2Frames;
                }
            }
            return GOECurveFindIndexResults.FailedNoKeyFrame;
        }
        public float GetTimeByIndex(int idx)
        {
            return keyFrames[idx].Time;
        }

        public bool IsKey(float t)
        {
            if (1 > keyFrames.Count || t < keyFrames[0].Time || t > keyFrames[keyFrames.Count - 1].Time)
            {
                return false;
            }
            for (int i = 0; i < keyFrames.Count; ++i)
            {
                if (MathUtil.FloatEqual(keyFrames[i].Time,t))
                {
                    return true;
                }
            }
            return false;
        }

        public void SampleByTime(float t, out Vector3 pos, out Quaternion q, out Vector3 s)
        {
            int left = 0, right = 0;
            pos = Vector3.zero;
            q = Quaternion.identity;
            s = Vector3.one;
            GOECurveFindIndexResults b = FindIndexByTime(t, out left, out right);
            if (b > GOECurveFindIndexResults.FailedNoKeyFrame)
            {
                GOECurvKeyFrame l = keyFrames[left];
                GOECurvKeyFrame r = keyFrames[right];
                GOECurvInterpolationType interpMode = l.PositionInterpolationType;
                pos = SampleKeyByTime(t, l, r, interpMode, l.Position, r.Position);
                
                interpMode = l.RotationInterpolationType;

                if (interpMode == GOECurvInterpolationType.Const || left == right)
                {
                    q = l.Rotation;
                }
                else if (interpMode == GOECurvInterpolationType.Linear)
                {
                    Quaternion ql, qr;
                    ql = l.Rotation;
                    qr = r.Rotation;
                    q = Quaternion.Slerp(ql, qr, (t - l.Time) / (r.Time - l.Time));
                }

                interpMode = l.ScaleInterpolationType;
                s = SampleKeyByTime(t, l, r, interpMode, l.Scale, r.Scale);
            }
        }

        public void SampleByIndex(int idx, ref Vector3 pos, ref Quaternion q, ref Vector3 s)
        {
            GOECurvKeyFrame frame = keyFrames[idx];
            pos = frame.Position;
            q = frame.Rotation;
            s = frame.Scale; 
        }

        Vector3 SampleKeyByTime(float t, GOECurvKeyFrame left, GOECurvKeyFrame right, GOECurvInterpolationType interpMode, Vector3 val, Vector3 val2)
        {
            if (interpMode == GOECurvInterpolationType.Const || left.Time == right.Time)
            {
                return val ;
            }
            else if (interpMode == GOECurvInterpolationType.Linear)
            {
                float durance = right.Time - left.Time;
                if (0 == durance)
                {
                    return val;
                }
                else
                {
                    float factor = (t - left.Time) / durance;
                    return val * (1 - factor) + val2 * factor;
                }
            }
            else if (interpMode == GOECurvInterpolationType.Bezier)
            {
                float x = SampleBezierByTime(t, left, right, val.x, val2.x);
                float y = SampleBezierByTime(t, left, right, val.y, val2.y);
                float z = SampleBezierByTime(t, left, right, val.z, val2.z);

            }
            return val;
        }

        float SampleBezierByTime(float t, GOECurvKeyFrame left, GOECurvKeyFrame right, float val, float val2)
        {
            Vector2 v0 = new Vector2(left.Time, val);
            Vector2 v3 = new Vector2(right.Time, val2);
            float durance = right.Time - left.Time;
            float diff = val2 - val;
            float len = Vector2.Distance(v0, v3);
            float l = len / 2;//使用1/4作为切线控制点长度//
            Vector2 v1 = new Vector2(1, left.OutTangent);
            v1 = v0 + v1.normalized * l;
            Vector2 v2 = new Vector2(1, right.InTangent);
            v2 = v3 - v2.normalized * l;
            float a = (t - left.Time) / durance;
            float b = 1 - a;
            Vector2 v = v0 * b * b * b + 3 * v1 * a * b * b + 3 * v2 * a * a * b + v3 * a * a * a;
            return v.y;
        }

        public void SampleByIndex2(int idx, ref Vector3 pos, ref Quaternion q, ref Vector3 s,
            ref GOECurvInterpolationType posInterpMode, ref GOECurvInterpolationType rotInterpMode, ref GOECurvInterpolationType scaleInterpMode)
        {
            SampleByIndex(idx, ref pos, ref q, ref s);
            GOECurvKeyFrame frame = keyFrames[idx];

            posInterpMode = frame.PositionInterpolationType;
            rotInterpMode = frame.RotationInterpolationType;
            scaleInterpMode = frame.ScaleInterpolationType;
        }

        public void SampleByIndex3(int idx, ref Vector3 pos, ref Quaternion q, ref Vector3 s,
            ref GOECurvInterpolationType posInterpMode, ref GOECurvInterpolationType rotInterpMode, ref GOECurvInterpolationType scaleInterpMode,
            ref Vector3 posInTangent, ref Vector3 posOutTangent,
            ref Vector3 scaleInTangent, ref Vector3 scaleOutTangent)
        {
            SampleByIndex2(idx, ref pos, ref q, ref s,
                ref posInterpMode, ref rotInterpMode, ref scaleInterpMode);
            /*posInterpMode.x = posx.keys[idx].tangentMode;
            posInterpMode.y = posy.keys[idx].tangentMode;
            posInterpMode.z = posz.keys[idx].tangentMode;
            rotInterpMode = rotx.keys[idx].tangentMode;
            scaleInterpMode.x = scalex.keys[idx].tangentMode;
            scaleInterpMode.y = scaley.keys[idx].tangentMode;
            scaleInterpMode.z = scalez.keys[idx].tangentMode;
            rotType = rottype.keys[idx].value;
            posInTangent.x = posx.keys[idx].inTangent;
            posInTangent.y = posy.keys[idx].inTangent;
            posInTangent.z = posz.keys[idx].inTangent;
            posOutTangent.x = posx.keys[idx].outTangent;
            posOutTangent.y = posy.keys[idx].outTangent;
            posOutTangent.z = posz.keys[idx].outTangent;
            scaleInTangent.x = scalex.keys[idx].inTangent;
            scaleInTangent.y = scaley.keys[idx].inTangent;
            scaleInTangent.z = scalez.keys[idx].inTangent;
            scaleOutTangent.x = scalex.keys[idx].outTangent;
            scaleOutTangent.y = scaley.keys[idx].outTangent;
            scaleOutTangent.z = scalez.keys[idx].outTangent;*/
        }
    }
}
