#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

namespace GOEngine.Implement
{
    public partial class GOECurvController
    {

        [JsonFieldAttribute(JsonFieldTypes.UnEditable)]
        public GameObject EditorGameObject { get; set; }

        public void EnsureEditorGameObject(GameObject parent)
        {
            if (!EditorGameObject)
            {
                EditorGameObject = new GameObject("CurvAnimGO");
            }

            foreach (GOECurvKeyFrame j in keyFrames)
                j.EnsureEditorGO(EditorGameObject);
            if (parent != null)
                EditorGameObject.transform.parent = parent.transform;
            EditorGameObject.transform.localPosition = Vector3.zero;
        }

        public void DisposeEditorGameObject()
        {
            if (EditorGameObject)
            {
                foreach (GOECurvKeyFrame j in keyFrames)
                        j.DisposeEditorGO();
                
                GameObject.Destroy(EditorGameObject);
            }
        }

        public void MoveKey(int keyIdx, float time)
        {
            GOECurvKeyFrame kf = keyFrames[keyIdx];
            kf.Time = time;
            keyFrames.RemoveAt(keyIdx);

            int left, right;
            GOECurveFindIndexResults b = FindIndexByTime(time, out left, out right);
            if (b == GOECurveFindIndexResults.AfterAllKeyFrame || b == GOECurveFindIndexResults.FailedNoKeyFrame)
                keyFrames.Add(kf);
            else
                keyFrames.Insert(left + 1, kf);
        }
        public void DeleteKey(int keyIdx)
        {
            KeyFrames[keyIdx].DisposeEditorGO();
            keyFrames.RemoveAt(keyIdx);
        }


        public void AddKey(float time, ref Vector3 vp, ref Quaternion vr, ref Vector3 vs, GOECurvInterpolationType pt = GOECurvInterpolationType.Linear, GOECurvInterpolationType rt = GOECurvInterpolationType.Linear, GOECurvInterpolationType st = GOECurvInterpolationType.Linear)
        {
            GOECurvKeyFrame kf = new GOECurvKeyFrame();
            kf.Time = time;
            kf.Position = vp;
            kf.Rotation = vr;
            kf.Scale = vs;
            kf.PositionInterpolationType = pt;
            kf.RotationInterpolationType = rt;
            kf.ScaleInterpolationType = st;

            int left, right;
            GOECurveFindIndexResults b = FindIndexByTime(time, out left, out right);
            if (b == GOECurveFindIndexResults.AfterAllKeyFrame || b == GOECurveFindIndexResults.FailedNoKeyFrame)
                keyFrames.Add(kf);
            else if (b == GOECurveFindIndexResults.BeforeAllKeyFrame)
                KeyFrames.Insert(0, kf);
            else
                keyFrames.Insert(left + 1, kf); 

            if (EditorGameObject)
            {
                kf.EnsureEditorGO(EditorGameObject);
                UnityEditor.Selection.activeGameObject = kf.EditorGameObject;
            }
        }
    }
}
#endif