using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GOEngine.Implement;

namespace GOEngine
{
    //特效组合动作类型//
    public enum ActType
    {
        [DisplayName("无")]
        None = 0,
        [DisplayName("场景特效")]
        SceneEffect = 1,				//效果//
        [DisplayName("角色特效")]
        ActorEffect = 2,				//绑定效果//
        [DisplayName("角色音效")]
        ActorSound = 4,				//声音//
        [DisplayName("角色动画")]
        AnimationQueue = 8,		//动画队列//
        [DisplayName("更换Shader")]
        ChangeShader = 0x10,
        [DisplayName("震屏")]
        CameraShake = 0x20,			//震屏//
        [DisplayName("相机曲线")]
        CameraCurve = 0x40,
        [DisplayName("事件")]
        Event = 0x80,						//事件//
        [DisplayName("Attach可见性")]
        AttachVisible = 0x100,			//Attach可见性//
        [DisplayName("ParaCurve")]
        ParaCurve = 0x200,
        [DisplayName("Actor可见性")]
        ActorVisible = 0x400,
        [DisplayName("弹道（飞向目标角色）")]
        EffectToTarget = 0x800,
        [DisplayName("弹道（飞向目标点）")]
        EffectToPos = 0x1000,
        [DisplayName("随机弹道（飞向角色）")]
        EffectRandomToTarget = 0x2000,
        [DisplayName("转向目标")]
        TurnToTarget = 0x4000,
        [DisplayName("ActivePos场景特效")]
        ActivePosSceneEffect = 0x8000,
        [DisplayName("将目标Attach到Actor")]
        AttachTarget = 0x10000,
        [DisplayName("将目标移动到目标点")]
        MoveTargetToPos = 0x20000,
        [DisplayName("角色震动")]
        ActorShake = 0x40000,
        [DisplayName("解除绑点下物件的绑定")]
        DetachBinding = 0x80000,
        [DisplayName("摄像机控制")]
        CameraControl = 0x100000,
        [DisplayName("天气控制")]
        ChangeWeather = 0x200000,
        [DisplayName("旋转指定角度")]
        TurnDegree = 0x400000,
        [DisplayName("角色挂件")]
        ActorAttachment = 0x800000
    }
    public class ActTypeNameAttribute : Attribute
    {
        private ActType _actype;
        public ActTypeNameAttribute(ActType type)
        {
            this._actype = type;
        }

        public ActType ActTypeName
        {
            get{return _actype;}
        }

        public static ActType GetTypeFlag(object obj)
        {
            Type t = obj.GetType();
            return getTypeFlagByType(t);
        }
        public static ActType getTypeFlagByType(Type pro)
        {
            object[] atts = pro.GetCustomAttributes(typeof(ActTypeNameAttribute), false);
            if (atts.Length > 0)
            {
                ActTypeNameAttribute attr = atts[0] as ActTypeNameAttribute;
                return attr.ActTypeName;
            }
            return ActType.None;
        }
    }
}
