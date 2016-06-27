using System;
using UnityEngine;
using System.Collections.Generic;
namespace GOEngine.Implement
{
    [ActTypeName(ActType.ActorEffect)]
    [System.Reflection.Obfuscation(Exclude = true)]
    [DisplayName("角色特效")]        
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class GOEActActorEffect : GOEActComponent
    {
        
        private string mEffectName = "dunji.prefab";					//特效名称//
        private string mBindPoint = "self"; // 绑定点//
        private bool mIsHitEffect = false;
        private bool mAttachToActor = true;
        bool mIsTargetActor=false;
        Vector3 mOffSet;
        float time;

        private List<IEntity> mEffect = new List<IEntity>();
        Dictionary<IEntity, IEntity> effectActorMapping;

        [DisplayName("特效名称")]       
        public string EffectName
        {
            get { return mEffectName; }
            set { mEffectName = value;}
        }

        [DisplayName("绑点")]
        [JsonField(JsonFieldTypes.BindPoint)]
        public string BindPoint
        {
            get { return mBindPoint; }
            set { mBindPoint = value; }
        }

        [DisplayName("是否是被击特效")]
        public bool IsHitEffect
        {
            get { return mIsHitEffect; }
            set { mIsHitEffect = value; }
        }

        [DisplayName("绑定到角色")]
        public bool AttachToActor
        {
            get { return mAttachToActor; }
            set { mAttachToActor = value; }
        }

        [DisplayName("是否在目标Actor播放")]
        public bool IsTargetActor
        {
            get { return mIsTargetActor; }
            set { mIsTargetActor = value; }
        }

        [DisplayName("最大播放时间")]
        public float MaxTime { get; set; }
        [DisplayName("偏移")]
        public Vector3 Offset
        {
            get { return mOffSet; }
            set { mOffSet = value; }
        }

        [DisplayName("本地偏移")]
        public Vector3 LocalOffset
        {
            get;
            set;
        }

        internal override void Start()
        {
            base.Start();
            time = 0;
        }

        internal override void OnTrigger()
        {
            base.OnTrigger ();
            try
            {
                time = 0;
                if (this.Entity.ResStatus != ResStatus.OK)
                {
                    return;
                }

                if (!AttachToActor)
                {
                    if (IsTargetActor)
                    {
                        for (int i = 0; i < EntityAct.TargetList.Count; i++)
                        {
                            Entity ent = EntityAct.TargetList[i];

                            Transform tm = ent.GetBindPoint(BindPoint);
                            if (tm == null)
                                return;
                            float scale = tm.localScale.x;
                            Vector3 delta = Camera.main.transform.position - tm.position;
                            Vector3 offset = Offset + tm.rotation * LocalOffset;

                            IEntity effect = ent.Scene.AddEffect(this.EffectName, tm.position + offset + delta.normalized * EntityAct.DistanceTowardsCamera, tm.rotation, scale);
                            effect.Priority = LoadPriority.PostLoad;
                            effect.Load();

                            if (mIsHitEffect)
                                effect.Orientation = this.Entity.Orientation * -1;
                            mEffect.Add(effect);
                        }
                    }
                    else
                    {
                        Transform tm = this.Entity.GetBindPoint(BindPoint);
                        if (tm == null)
                            return;
                        Vector3 delta = Camera.main.transform.position - tm.position;
                        float scale = tm.localScale.x;
                        Vector3 offset = Offset + tm.rotation * LocalOffset;

                        IEntity effect = this.Entity.Scene.AddEffect(this.EffectName, tm.position + offset + delta.normalized * EntityAct.DistanceTowardsCamera, tm.rotation, scale);
                        effect.Priority = LoadPriority.PostLoad;
                        effect.Load();

                        if (mIsHitEffect)
                            effect.Orientation = this.Entity.Orientation * -1;
                        mEffect.Add(effect);
                    }
                }
                else
                {
                    if (!IsTargetActor)
                    {
                        IEntity effect = (this.Entity as GOEActorEntity).AddEffect(this.EffectName, BindPoint);
                        effect.Priority = LoadPriority.PostLoad;
                        effect.Load();

                        if (mIsHitEffect)
                            effect.Orientation = this.Entity.Orientation * -1;
                        mEffect.Add(effect);
                    }
                    else
                    {
                        for (int i = 0; i < EntityAct.TargetList.Count; i++)
                        {
                            Entity ent = EntityAct.TargetList[i];

                            IEntity effect = (ent as GOEActorEntity).AddEffect(this.EffectName, BindPoint);
                            effect.Priority = LoadPriority.PostLoad;
                            effectActorMapping[effect] = ent;
                            effect.Load();

                            if (mIsHitEffect)
                                effect.Orientation = this.Entity.Orientation * -1;
                            mEffect.Add(effect);
                        }
                    }
                }
                if (this.EntityAct.ActSpeed != 1)
                {
                    for (int i = 0; i < mEffect.Count; i++)
                    {
                        mEffect[i].SetSpeed(EntityAct.ActSpeed);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.ToString());
            }
            //this.Enable = false;
        }

        internal override void Pause()
        {
            base.Pause();
            for (int i = 0; i < mEffect.Count;i++ )
            {
                mEffect[i].Pause();
            }
            Enable = false;
        }

        internal override void Restart()
        {
            base.Pause();
            for (int i = 0; i < mEffect.Count; i++)
            {
                mEffect[i].Restart();
            }
            Enable = true;
        }

        internal override void Update()
        {
            base.Update();
            time += Time.deltaTime;
            if (time > MaxTime && MaxTime > 0)
            {
                LifeOver = true;
            }
        }

        internal override void PreLoad()
        {
            GOERoot.ResMgrImp.GetAsset( mEffectName, null );
        }

        internal override void OnDestroy()
        {
            for (int i = 0; i < mEffect.Count; i++)
            {
                IEntity effect = mEffect[i];
                if (!AttachToActor)
                    this.Entity.Scene.DelEntity(effect);
                else
                {
                    if (!IsTargetActor)
                        this.Entity.RemoveAttach(effect);
                    else
                        effectActorMapping[effect].RemoveAttach(effect);
                }
            }
            mEffect.Clear();
            base.OnDestroy();
        }

        internal override void GetResAsset(HashSet<string> setRes)
        {
            if ( mEffectName != string.Empty )
            {
                setRes.Add( mEffectName );
            }
        }

    }
}

