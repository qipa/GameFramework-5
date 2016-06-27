using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GOEngine
{
    public enum AnimatorParamType
    {
        TypeBool,
        TypeInt,
        TypeFloat
    }

    public enum AnimationType
    {
        Stand,
        Run,
        Birth,
        Hit,
        Attack1,
        Attack2,
    }

    public class AnimatorParam
    {
        string paraName;
        System.Object value;
        AnimatorParamType paraType;
        internal AnimatorParam(string name, System.Object va, AnimatorParamType type)
        {
            paraName = name;
            value = va;
            paraType = type;
        }

        internal void ExecuteParam(Animator _animator)
        {
            if (_animator == null)
                return;
            switch (paraType)
            {
                case AnimatorParamType.TypeBool:
                    _animator.SetBool(paraName, (bool)value);
                    break;
                case AnimatorParamType.TypeFloat:
                    _animator.SetFloat(paraName, (float)value);
                    break;
                case AnimatorParamType.TypeInt:
                    _animator.SetInteger(paraName, (int)value);
                    break;
            }
        }
    }

}
namespace GOEngine.Implement
{
    public class AnimatorConfig
    {
        private static Dictionary<AnimationType, AnimatorParam> _animatorConfigs = new Dictionary<AnimationType, AnimatorParam>();
        private static bool inited = false;
        internal static void init()
        {
            inited = true;
            _animatorConfigs.Add(AnimationType.Stand, new AnimatorParam("MoveTime", -1.0f, AnimatorParamType.TypeFloat));
            _animatorConfigs.Add(AnimationType.Run, new AnimatorParam("MoveTime", 1.0f, AnimatorParamType.TypeFloat));
        }

        internal static AnimatorParam GetAnimatorParam(AnimationType type)
        {
            if (!inited)
                init();
            AnimatorParam config;
            if (_animatorConfigs.TryGetValue(type, out config))
                return config;
            return null;
        }
    }
}

