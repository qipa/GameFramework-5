using System;
using UnityEngine;
using System.Collections.Generic;
namespace GOEngine.Implement
{
    [ActTypeName(ActType.ActorAttachment)]
    [System.Reflection.Obfuscation(Exclude = true)]
    [DisplayName("角色挂件")]        
#if UNITY_EDITOR
    public
#else
    internal
#endif
    class GOEActActorAttachment : GOEActComponent
    {
        
        private string mEffectName = "dunji.prefab";					//特效名称//
        private string mBindPoint = "self"; // 绑定点//
        private bool mAttachToActor = true;
        bool mIsTargetActor=false;
        Vector3 mScale = Vector3.one;
        float time;

        private List<IEntity> mEffect = new List<IEntity>();
        Dictionary<IEntity, IEntity> effectActorMapping;

        [DisplayName("挂件名称")]       
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
        [DisplayName("缩放")]
        public Vector3 Scale
        {
            get { return mScale; }
            set { mScale = value; }
        }
        [DisplayName("旋转角度")]
        public float Degree { get; set; }
        internal override void Start()
        {
            base.Start();
            time = 0;
        }

        internal override void OnTrigger()
        {
            base.OnTrigger();
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
                            IEntity effect = ent.Scene.AddEntity(this.EffectName);//, tm.position + delta.normalized * EntityAct.DistanceTowardsCamera, tm.rotation, scale);
                            effect.Position = tm.position;
                            var ori = tm.rotation * Quaternion.Euler(0, Degree, 0);
                            effect.Rotation = ori;

                            effect.Scale = mScale;
                            effect.Priority = LoadPriority.PostLoad;
                            effect.Load();

                            mEffect.Add(effect);
                        }
                    }
                    else
                    {
                        Transform tm = this.Entity.GetBindPoint(BindPoint);
                        if (tm == null)
                            return;

                        IEntity effect = this.Entity.Scene.AddEntity(this.EffectName);//, tm.position + delta.normalized * EntityAct.DistanceTowardsCamera, tm.rotation, scale);
                        effect.Position = tm.position;
                        effect.Rotation = tm.rotation;
                        effect.Scale = mScale;
                        effect.Priority = LoadPriority.PostLoad;
                        effect.Load();

                        mEffect.Add(effect);
                    }
                }
                else
                {
                    if (!IsTargetActor)
                    {
                        IEntity effect = (this.Entity as GOEActorEntity).AddAttach(this.EffectName, BindPoint);
                        effect.Scale = mScale;
                        effect.Priority = LoadPriority.PostLoad;
                        effect.Load();

                        mEffect.Add(effect);
                    }
                    else
                    {
                        for (int i = 0; i < EntityAct.TargetList.Count; i++)
                        {
                            Entity ent = EntityAct.TargetList[i];

                            IEntity effect = (ent as GOEActorEntity).AddAttach(this.EffectName, BindPoint);
                            effect.Scale = mScale;
                            effect.Priority = LoadPriority.PostLoad;
                            effectActorMapping[effect] = ent;
                            effect.Load();

                            mEffect.Add(effect);
                        }
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
            Enable = false;
        }

        internal override void Restart()
        {
            base.Pause();
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
            GOERoot.ResMgrImp.GetAsset(mEffectName, (name, obj) => GOERoot.ResMgrImp.ReleaseAsset(name, obj));
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

