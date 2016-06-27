using System;
using System.Collections.Generic;
using UnityEngine;

namespace GOEngine.Implement
{
    [System.Reflection.Obfuscation(Exclude = true)]
    [DisplayName("角色ACT")]        
#if UNITY_EDITOR
    public
#else
    internal
#endif
    class GOEEntityAct : GOEEntityComponent, IGOEEntityAct
    {
        private float mLifeTime = 3.0f;

        private float mLocalTime = 0.0f;
        private string mActName;
        private bool mWaitForOneFrame = false;

        private List<Entity> mListTarget = new List<Entity>();
        private Vector3 mTargetPos = Vector3.zero;
        public ActType TypeFilters = ActType.AnimationQueue | ActType.ActorVisible;
        public float ActSpeed = 1f;

        [DisplayName("生命时间")]
        public float LiftTime
        {
            get { return mLifeTime;}
            set { mLifeTime = value;}
        }

        [JsonField(JsonFieldTypes.UnEditable)]
        public bool ImmediatelyRun { get; set; }

        [JsonField(JsonFieldTypes.UnEditable)]
        public string ActName
        {
            set { mActName = value; }
            get { return mActName;}
        }

        [JsonField(JsonFieldTypes.UnEditable)]
        public float LocalTime
        {
            set { mLocalTime = value;}
            get { return mLocalTime; }
        }

        private List<GOEActComponent> _listComponent = new List<GOEActComponent>();
        [JsonFieldAttribute(JsonFieldTypes.HasChildren)]
        public List<GOEActComponent> ListComponent
        {
            get { return _listComponent; }
            set { _listComponent = value; }
        }

        internal List<Entity> TargetList
        {
            get { return mListTarget; }
        }

        [JsonField(JsonFieldTypes.UnEditable)]
        public Vector3 TargetPos
        {
            get { return mTargetPos; }
            set { mTargetPos = value; }
        }

        [JsonField(JsonFieldTypes.UnEditable)]
        public float DistanceTowardsCamera { get; set; }
        public GOEEntityAct()
        {
        }

        public void AddComponent(GOEActComponent comp)
        {
            comp.EntityAct = this;
            ListComponent.Add( comp );
        }

        public GOEActComponent GetComponent(int nIndex)
        {
            return ListComponent[nIndex];
        }
        public T[] GetComponent<T>() where T : GOEActComponent
        {
            List<T> listT = new List<T>();
            for ( int i = 0; i < ListComponent.Count; ++i )
            {
                GOEActComponent comp = ListComponent[i];
                T temp = comp as T;
                if ( temp != null )
                {
                    listT.Add( temp );
                }
            }

            return listT.ToArray();
        }

        public int GetComponentCount()
        {
            return ListComponent.Count;
        }

        public void DelComponent(int nIndex)
        {
            ListComponent.RemoveAt( nIndex );
        }

        public static GOEEntityAct GetActInstance(string name)
        {
            MemoryPool<GOEEntityAct> pool;
            GOEEntityAct act;
            if (_actPool.TryGetValue(name, out pool))
            {
                act = pool.Alloc();
                if (act != null)
                    return act;
            }
            return new GOEEntityAct();
        }

        private static Dictionary<string, MemoryPool<GOEEntityAct>> _actPool = new Dictionary<string, MemoryPool<GOEEntityAct>>();
        internal override void OnDestroy()
        {
            Stop();
            mLocalTime = 0.0f;
            MemoryPool<GOEEntityAct> pool;
            if(!_actPool.TryGetValue(ActName, out pool))
            {
                pool = new MemoryPool<GOEEntityAct>();
                _actPool.Add(ActName, pool);
            }
            pool.Free(this);
            DestroyActComponent(true);
        }

        internal override void Update()
        {
            base.Update ();
            
            if ( this.Entity.ResStatus != ResStatus.OK )
            {
                mWaitForOneFrame = true;
                return;
            }

            // OnloadGameObject有些是异步消息//
            if ( mWaitForOneFrame )
            {
                mWaitForOneFrame = false;

                return;
            }

            mLocalTime = GOERoot.RealTime.DeltaTime * ActSpeed + mLocalTime;
            
            for( int i = 0; i < ListComponent.Count; ++i )
            {
                GOEActComponent comp = ListComponent[i];
                comp.LocalTime = mLocalTime;
                comp.CheckTrigger();
                if (comp.Enable)
                    comp.Update();
                if (comp.LifeOver)
                {
                    comp.OnDestroy();
                    comp.Enable = false;
                }
            }

            if ( mLifeTime > 0 && mLocalTime > mLifeTime )
            {
                Enable = false;
                if (!GOERoot.IsEditor)
                {
                    Stop();
                    this.LifeOver = true;
                }
                else
                {
                    DestroyActComponent(true);
                }
            }
        }
        


        //本函数只在编辑器模式下使用//
#if UNITY_EDITOR
        public void Replay()
        {
            DestroyActComponent(true);
            this.Enable = true;
            this.LocalTime = 0.0f;
            this.LifeOver = false;

            //this.TargetPos = this.Entity.Position + this.Entity.Orientation * 10;
            
            foreach(GOEActComponent comp in ListComponent )
            {
                comp.LifeOver = false;
                GOEActParaCurve paraCurve = comp as GOEActParaCurve;
                if ( paraCurve != null )
                {
                    Vector3 pos = this.Entity.Position + this.Entity.Orientation * 5;
                    paraCurve.Pos = pos;
                }
            }
        }
#endif
        
        internal void EnableAct< T >(bool bEnable) where T : GOEActComponent
        {
            for ( int i = 0; i < ListComponent.Count; ++i )
            {
                GOEActComponent comp = ListComponent[i];
                if ( comp as T != null )
                {
                    comp.Enable = bEnable;
                }
            }
        }

        public void Clone(string actStr)
        {
            this.DestroyActComponent();

            Profiler.BeginSample("json.FillObject");
            fastJSON.JSON.FillObject(this, actStr);
            Profiler.EndSample();

            foreach (GOEActComponent com in ListComponent)
                com.EntityAct = this;
            _listComponent.Sort(SortACT);
            this.PreLoad();
            play();
        }

        int SortACT(GOEActComponent a, GOEActComponent b)
        {
            if (a is GOEActTurnToTarget)
            {
                if (b is GOEActTurnToTarget)
                {
                    return 0;
                }
                else
                    return -1;
            }
            else
            {
                if (b is GOEActTurnToTarget)
                    return 1;
                else
                    return 0;
            }
        }

        private void PreLoad()
        {
            for ( int i = 0; i < ListComponent.Count; ++i )
            {
                ListComponent[i].PreLoad();
            }
        }
        
        protected override void OnEnabled ()
        {
            base.OnEnabled ();			
        }

        public void LoadAndPlay()
        {
            GOERoot.ResMgrImp.GetString(mActName, OnGetAct);
        }

        public void Pause()
        {
            this.Enable = false;
            for (int i = 0; i < ListComponent.Count; ++i)
            {
                ListComponent[i].Pause();
            }
        }

        public void Restart()
        {
            this.Enable = true;
            for (int i = 0; i < ListComponent.Count; ++i)
            {
                ListComponent[i].Restart();
            }
        }

        public void Stop()
        {
            this.Enable = false;
            this.DestroyActComponent(true);
            this.ClearTarget();
            this.LifeOver = true;
        }

        public void play()
        {
            this.LifeOver = false;
            this.LocalTime = 0;
            Start();
            Enable = true;

            if (ImmediatelyRun)
            {
                if (this.Entity.ResStatus != ResStatus.OK)
                {
                    return;
                }
                for (int i = 0; i < ListComponent.Count; ++i)
                {
                    GOEActComponent comp = ListComponent[i];
                    comp.LocalTime = mLocalTime;
                    comp.CheckTrigger();
                }
            }
        }

        internal override void Start()
        {
            base.Start();
            for (int i = 0; i < ListComponent.Count; ++i)
            {
                GOEActComponent comp = ListComponent[i];
                comp.Start();
            }
            setFilters();
        }

        private void setFilters()
        {
            if(TypeFilters != null)
            {
                for (int i = 0; i < ListComponent.Count; ++i)
                {
                    ActType type = ActTypeNameAttribute.GetTypeFlag(ListComponent[i]);
                    if ((TypeFilters & type ) != ActType.None)
                        ListComponent[i].IsIgored = true;
                }
            }
        }

        private void OnGetAct(string name, string actstring)
        {
            if (null != actstring)
            {
                this.Clone(actstring);
            }
            else
            {
                Logger.GetFile(LogFile.Res).LogError(mActName + " not find");		
            }
        }

        private void DestroyActComponent( bool onlyDestory = false)
        {
            for ( int i = 0; i < ListComponent.Count; ++i )
            {
                ListComponent[i].OnDestroy();
            }
            if (!onlyDestory)
                ListComponent.Clear();
        }

        internal void GetRes(HashSet<string> setRes)
        {
            for ( int i = 0; i < ListComponent.Count; ++i )
            {
                ListComponent[i].GetRes( setRes );
            }
        }

        internal void GetResAsset(HashSet<string> setRes)
        {
            for ( int i = 0; i < ListComponent.Count; ++i )
            {
                ListComponent[i].GetResAsset( setRes );
            }
        }

        public void AddTarget( IEntity entity )
        {
            Entity ent = entity as Entity;
            if (mListTarget.Contains(ent))
            {
                return;
            }

            mListTarget.Add(ent);
        }

        internal void ClearTarget()
        {
            mListTarget.Clear();
        }
    }
}

