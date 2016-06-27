using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GOEngine.Implement
{
    public enum GOECurvInterpolationType
    {
        Const,
        Linear,
        Bezier,
    }
    
#if UNITY_EDITOR
    public
#else
    internal
#endif
        partial class GOECurvKeyFrame
    {
        public float Time { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public float InTangent { get; set; }
        public float OutTangent { get; set; }

        public GOECurvInterpolationType PositionInterpolationType { get; set; }

        public GOECurvInterpolationType RotationInterpolationType { get; set; }

        public GOECurvInterpolationType ScaleInterpolationType { get; set; }
        public GOECurvKeyFrame()
        {
        }
    }
}
