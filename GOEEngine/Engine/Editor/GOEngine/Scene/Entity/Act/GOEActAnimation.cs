using System;
using System.Collections.Generic;
using UnityEngine;

namespace GOEngine.Implement
{
    [ActTypeName(ActType.AnimationQueue)]
    [System.Reflection.Obfuscation(Exclude = true)]
    [DisplayName("角色动画")]
#if UNITY_EDITOR
    public
#else
    internal
#endif
 class GOEActAnimation : GOEActComponent
    {
        float crossfadeTime = 0.3f;
        public GOEActAnimation()
        {
        }
        [DisplayName("动画1")]
        [JsonFieldAttribute(JsonFieldTypes.Animation)]
        public string Animation1
        {
            get;
            set;
        }
        [DisplayName("动画2")]
        [JsonFieldAttribute(JsonFieldTypes.Animation)]
        public string Animation2
        {
            get;
            set;
        }
        [DisplayName("动画3")]
        [JsonFieldAttribute(JsonFieldTypes.Animation)]
        public string Animation3
        {
            get;
            set;
        }

        [DisplayName("动画播放完毕是否不能回到站立状态")]
        public bool DoNotReturnToStand { get; set; }

        [DisplayName("动画需要融合")]
        public bool IsAnimationBlend { get; set; }
        [DisplayName("融合权重")]
        public float BlendWeight { get; set; }
        //[DisplayName("动画融合节点")]
        //public string BlendPointName { get { return blindPoint; } set { blindPoint = value; } }

        [DisplayName("动画渐变时间")]
        public float CrossfadeTime { get { return crossfadeTime; } set { crossfadeTime = value; } }

        [DisplayName("下一动画是否禁用渐变")]
        public bool IsNextAnimationNoCrossfade { get; set; }

        private bool bBlend = false;
        internal override void OnTrigger()
        {
            base.OnTrigger();
            GOEActorEntity act = this.Entity as GOEActorEntity;
            bool has1 = !string.IsNullOrEmpty(Animation1);
            bool has2 = !string.IsNullOrEmpty(Animation2);
            bool has3 = !string.IsNullOrEmpty(Animation3);
            if (EntityAct.ActSpeed != 1)
                act.AnimationSpeed = EntityAct.ActSpeed;
            if (has1)
            {
                //只与部分动作融合（跑步）
                if (IsAnimationBlend && act.CheckBlendAnim())
                {
                    if (act.PlayBlend(Animation1, BlendWeight, act.BlendPoint))
                        bBlend = true;
                }
                else
                {
                    act.Stop();
                    if (!has2)
                        act.CrossFade(Animation1, crossfadeTime, PlayMode.StopSameLayer, !DoNotReturnToStand);
                    else
                        act.CrossFade(Animation1, crossfadeTime, PlayMode.StopSameLayer, false);
                }
            }
            if (has2)
            {
                if (!has3)
                    act.PlayQueued(Animation2, QueueMode.CompleteOthers, PlayMode.StopSameLayer, !DoNotReturnToStand);
                else
                    act.PlayQueued(Animation2);
            }
            if (has3)
            {
                act.PlayQueued(Animation3, QueueMode.CompleteOthers, PlayMode.StopSameLayer, !DoNotReturnToStand);
            }
            if (IsNextAnimationNoCrossfade)
                act.NextAnimationNoCrossfade = true;
            this.Enable = false;
        }

        internal override void OnDestroy()
        {
            base.OnDestroy();
            GOEActorEntity act = this.Entity as GOEActorEntity;
            if (bBlend)
            {
                act.ClearBlend(Animation1, act.BlendPoint);
            }
            if (EntityAct.ActSpeed != 1)
                act.AnimationSpeed = 1;
        }

        internal override void GetResAsset(HashSet<string> setRes)
        {

        }

        internal override void PreLoad()
        {

        }
    }
}

