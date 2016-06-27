using UnityEngine;
using System.Collections;

namespace GOEngine.Implement
{
    [System.Reflection.Obfuscation(Exclude = true)]
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class GOECurvKeyFrameGO : MonoBehaviour
    {
        public GOECurvKeyFrame Keyframe { get; set; }

        Vector3 oldPos, oldScale;
        Quaternion oldRot;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Keyframe == null)
                return;
            if (oldPos != transform.localPosition)
            {
                oldPos = transform.localPosition;
                Keyframe.Position = oldPos;
            }
            if (oldRot != transform.localRotation)
            {
                oldRot = transform.localRotation;
                Keyframe.Rotation = oldRot;
            }
            if (oldScale != transform.localScale)
            {
                oldScale = transform.localScale;
                Keyframe.Scale = transform.localScale;
            }
        }

        public void SetPosition(ref Vector3 pos)
        {
            oldPos = pos;
            transform.localPosition = pos;            
        }

        public void SetRotation(ref Quaternion rot)
        {
            oldRot = rot;
            transform.localRotation = rot;            
        }

        public void SetScale(ref Vector3 scale)
        {
            oldScale = scale;
            transform.localScale = scale;
        }
    }
}