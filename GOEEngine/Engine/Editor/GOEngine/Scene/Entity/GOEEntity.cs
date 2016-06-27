using System;
using UnityEngine;
using System.Collections.Generic;
using GOEngine.Editor.GOEngine.util;

namespace GOEngine
{
    public enum CastShadows
    {
        Default,
        Yes,
        No,
    }
}
namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class Entity : GameObjectRes, IEntity
    {
        protected Vector3 mPos = Vector3.zero;
        protected Quaternion mRotation = Quaternion.identity;
        protected Vector3 mScale =  Vector3.one;		
        private Transform mParent = null;
        private bool mVisible = true;
        private Bounds mBounds = new Bounds(Vector3.zero, Vector3.zero);
        private Bounds mOriBounds = new Bounds( Vector3.zero, Vector3.zero );
        private float effSpeed = 1;
        private string id;
        private Vector3 mOri = Vector3.forward;
        private bool mEnablePhysics = false;
        private int mLayer = -1;
        private CastShadows mCastShadows = CastShadows.Default;
        private AnimationCullingType mAniCullingType = AnimationCullingType.BasedOnRenderers;
        private bool mCustomBound = false;
        private float mWeight = 100;
        private GOEGameScene mScene = null;

        private MeshCollider mMeshCollider = null;
        static internal int mDebugCount = 0;
        private bool mEnableCollid = false;
        private float mCollisionRadius = 0.3f;
        bool renderVisible = true;
        Renderer[] renderers;
        
        /// <summary>
        /// 是否是虚拟的，比如武器，本身为身体的一部份，只是添加代码控制
        /// </summary>
        public bool IsVirtual = false;
        bool pause = false;
        
        // 用于解决在Update里删除的问题 //
        private bool mLifeOver = false;

        public Entity( string name )
        {
            ++mDebugCount;
            ID = "Entity_" + mDebugCount;
            base.Name = name;
        }

        public Entity()
        {
            ++mDebugCount;
            ID = "Entity_" + mDebugCount;
        }

        internal GOEGameScene Scene
        {
            set { mScene = value; }
            get { return mScene; }
        }

        public float Weight
        {
            set { mWeight = value; }
            get { return mWeight; }
        }
        
        public string ID
        {
            internal set { id = value; }
            get { return id; }
        }

        public MeshCollider MeshCollider
        {
            get { return mMeshCollider; }
        }

        public CastShadows CastShadows
        {
            set { this.SetCastShadows( value ); }
            get { return mCastShadows; }
        }

        internal AnimationCullingType AniCullingType
        {
            set { mAniCullingType = value; }
            get { return mAniCullingType; }
        }
        public Vector3 Position
        {
            set{ this.SetPos( value );}
            get { return this.GetPos();}
        }
        
        public Vector3 Orientation
        {
            set { this.SetOrientation( value ); }
            get { return this.GetOrientation();}
        }
        
        public Quaternion Rotation
        {
            get { return this.GetRotation(); }
            set { this.SetRotation( value);  }
        }
        
        public Vector3 Scale
        {
            get { return this.GetScale(); }
            set { this.SetScale( value );}
        }

        public bool GlobalVisible
        {
            get { return null == GameObject ? false : GameObject.activeInHierarchy; }
        }

        public bool Visible
        {
            get { return null == GameObject ? mVisible : GameObject.activeSelf; }
            set { this.SetVisible( value ); }
        }

        public float CollisionRadius
        {
            get { return mCollisionRadius; }
            set { mCollisionRadius = value; }
        }

        public bool IgnorePathFindingCollision { get; set; }
        public bool RenderVisible
        {
            get
            {
                return renderVisible;
            }
            set
            {
                if (renderVisible != value && renderers != null)
                {
                    renderVisible = value;
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        renderers[i].enabled = value;
                    }
                }
            }
        }
        public Transform Parent
        {
            set { this.SetParent( value);}
            get { return this.GetParent();}
        }
        
        public int Layer
        {
            set { this.SetLayer( value ); }
            get { return mLayer;}
        }
        
        public bool LifeOver
        {
            set { mLifeOver = value; }
            get { return mLifeOver; }
        }
        
        internal Bounds Bound
        {
            get { return mBounds; }
            set
            {
                mBounds = value;
                mOriBounds = value;
                mCustomBound = true;

                Bounds temp = mBounds;
                Vector3 center = temp.center;
                center.y *= Scale.x; //人物的原点在中心点的上方
                temp.center = center;
                temp.size *= Scale.x;
                mBounds = temp;
            } 
        }
        
        internal Bounds BoundInScene
        {
            get
            {
                Bounds bd = new Bounds(Position + Bound.center, Bound.size);
                return bd;
            }
        }

        //@xl 去掉这个接口，这个在能修改gameobject的前提下没有意义 //
        //internal Transform transform
        //{
        //    get
        //    {
        //        if ( this.GameObject != null )
        //        {
        //            if (!mTransform)
        //            {
        //                mTransform = GameObject.transform;
        //            }
        //            return mTransform;
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //}
        
        public bool EnablePhysics
        {
            set { if ( mEnablePhysics != value) this.SetPhysics( value); }
            get { return mEnablePhysics; }
        }

        public bool EnableCollid
        {
            set { if (mEnableCollid != value) this.SetCollid(value); }
            get { return mEnableCollid; }
        }

        internal Animation Animation
        {
            get
            {
                if (this.GameObject == null)
                    return null;
                return this.GameObject.GetComponent<Animation>(); 
            }
        }

        static public Entity CreateEmptyEntity()
        {
            Entity child = new Entity();
            GameObject obj = new GameObject();
            
            child.mGameObject = obj;

            child.mObject = obj;
            child.mResStatus = ResStatus.OK;
            
            return child;
        }
        
        public virtual void ShowBoundLineRenderer( bool showLine )
        {
            if ( this.ResStatus != ResStatus.OK || this.GameObject == null )
            {
                return;
            }

            LineRenderer lineRd = this.GameObject.GetComponent<LineRenderer> ();
            if ( !showLine )
            {
                if ( lineRd != null )
                {
                    lineRd.enabled = false;
                }
                return;
            }

            Color c1 = Color.yellow;
            Color c2 = Color.red;
            float width1 = 0.13f;
            float width2 = 0.13f;
            
            if ( lineRd == null )
            {
                lineRd = this.GameObject.AddComponent<LineRenderer> ();
                lineRd.material = new Material ( Shader.Find ( "Particles/Additive" ) );
                lineRd.SetColors ( c2, c1 );
                lineRd.SetWidth ( width1, width2 );
            }
            lineRd.enabled = true;
            List<Vector3> list = new List<Vector3> ();
            Vector3 center = this.GameObject.transform.position + Bound.center;
            Vector3 dir = Bound.size / 2;
            float dirx = Bound.size.x;
            float diry = Bound.size.y;
            float dirz = Bound.size.z;

            Vector3 pos1 = center - dir;
            Vector3 pos2 = center - dir + dirx * Vector3.right;
            Vector3 pos3 = center - dir + diry * Vector3.up;
            Vector3 pos4 = center - dir + dirz * Vector3.forward;
            Vector3 pos5 = center + dir;
            Vector3 pos6 = center + dir - dirx * Vector3.right;
            Vector3 pos7 = center + dir - diry * Vector3.up;
            Vector3 pos8 = center + dir - dirz * Vector3.forward;

            list.Add ( pos1 );
            list.Add ( pos2 );
            list.Add ( pos7 );
            list.Add ( pos4 );
            list.Add ( pos1 );
            list.Add ( pos3 );
            list.Add ( pos8 );
            list.Add ( pos5 );
            list.Add ( pos6 );
            list.Add ( pos3 );
            list.Add ( pos2 );
            list.Add ( pos8 );
            list.Add ( pos7 );
            list.Add ( pos5 );
            list.Add ( pos4 );
            list.Add ( pos6 );


            lineRd.SetVertexCount ( list.Count );
            for ( int i = 0; i < list.Count; ++i )
            {
                lineRd.SetPosition ( i, list[i] );
            }
        }
        
        internal override void Destroy ()
        {
            Parent = null;
            removeAllAttach();
            base.Destroy ();
        }
        
        protected void removeAllAttach()
        {
            foreach (Entity att in mAttaches)
            {
                att.Destroy();
            }
            mAttaches.Clear();
        }

        private void SetLayer( int nlayer )
        {
            mLayer = nlayer;
            if ( this.GameObject != null )
            {
                GameObjectUtil.SetLayer(this.GameObject, nlayer);
            }
        }

        protected List<Entity> mAttaches = new List<Entity>();
        public IEntity AddAttach(string name, Transform tm)
        {
            Entity attach = this.Scene.AddEntity(name) as Entity;
            attach.SetParent(tm);
            mAttaches.Add(attach);
            return attach;
        }

        public IEntity AddAttach(string name, string bd)
        {
            Transform tm = GetBindPoint(bd);
            return AddAttach(name, tm);
        }

        public void AddAttach(IEntity attach, Transform tm)
        {
            attach.Parent = tm;
            attach.Visible = true;
            mAttaches.Add(attach as Entity);
        }

        public void AddAttach(IEntity attach, string bd)
        {

            Transform tm = GetBindPoint(bd);
            AddAttach(attach, tm);
        }

        public void RemoveAttach(IEntity attach, bool noDelete = false)
        {
            if (!noDelete)
                this.Scene.DelEntity(attach);
            else
            {
                attach.Parent = null;
                attach.Visible = false;
            }
            Entity ent = attach as Entity;
            if (mAttaches.Contains(ent))
            {
                mAttaches.Remove(ent);
            }
        }

        public IEntity GetAttchById(string id)
        {
            foreach( Entity att in mAttaches )
            {
                if( att.ID == id)
                    return att;
            }
            return null;
        }

        public IEntity GetAttchByName(string name)
        {
            foreach (Entity att in mAttaches)
            {
                if (att.Name.Contains(name))
                    return att;
            }
            return null;
        }
        
        private void SetParent( Transform parent )
        {
            mParent = parent;
            if (IsVirtual)
                return;
            if ( this.GameObject != null )
            {
                this.GameObject.transform.SetParent(parent);
                this.GameObject.transform.localRotation = mRotation;
                this.GameObject.transform.localPosition = mPos;
                this.GameObject.transform.localScale = mScale;
            }
        }
        private Transform GetParent()
        {
            if ( this.GameObject != null )
            {
                return this.GameObject.transform.parent;
            }
            else
            {
                return mParent;
            }
        }
        protected void SetVisible( bool bVisible )
        {
            bool scenevisible = true;
            if ( Scene != null )
            {
                scenevisible = true;//Scene.Visible;
            }
            if ( this.GameObject != null )
            {
                this.GameObject.SetActive( bVisible & scenevisible );
            }
            mVisible = bVisible;
        }		
        
        private void SetPos( Vector3 pos )
        {
            if ( this.GameObject != null )
            {
                this.GameObject.transform.position = pos;
            }
            //else
            {
                mPos = pos;
            }
        }
        
        private Vector3 GetPos()
        {
            if ( this.GameObject != null )
            {
                return this.GameObject.transform.position;
            }
            else
            {
                return mPos;
            }
        }
        
        private void SetOrientation( Vector3 ori )
        {
            mOri = ori;
            Rotation = Quaternion.LookRotation(ori);
        }
        
        private Vector3 GetOrientation()
        {
            if ( this.GameObject != null )
            {
                return this.GameObject.transform.TransformDirection(Vector3.forward);
            }
            else
            {
                return mOri;
            }
        }
        
        private void SetRotation( Quaternion rotation )
        {
            if ( this.GameObject != null )
            {
                this.GameObject.transform.rotation = rotation;
            }
            {
                mRotation = rotation;
            }
        }
        
        private Quaternion GetRotation()
        {
            if ( this.GameObject != null )
            {
                return this.GameObject.transform.rotation;
            }
            else
            {
                return mRotation;
            }
        }
        
        private void SetScale( Vector3 scale )
        {
            if ( this.GameObject != null )
            {
                this.GameObject.transform.localScale = scale;
            } 
            {
                mScale = scale;
            }

            this.OnScaleChg();
        }
        
        private Vector3 GetScale()
        {
            if ( this.GameObject != null )
            {
                return this.GameObject.transform.localScale;
            }
            else
            {
                return mScale;
            }
        }
        

        public void DeletePhysics()
        {
            GameObject obj = this.GameObject;
            if ( obj != null )
            {
                Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
                foreach (Collider collider in colliders)
                {
                    UnityEngine.Object.DestroyObject( collider );                    
                }

                CharacterJoint[] joints = obj.GetComponentsInChildren<CharacterJoint>( true );
                foreach ( CharacterJoint joint in joints )
                {
                    UnityEngine.Object.DestroyObject( joint );
                }

                Rigidbody[] righdbodeis = obj.GetComponentsInChildren<Rigidbody>(true);
                foreach (Rigidbody body in righdbodeis)
                {
                    UnityEngine.Object.DestroyObject( body );
                }
            }
        }

        public void SetSpeed( float speed )
        {
            effSpeed = speed;
            if (this.GameObject == null)
                return;
            ParticleSystem[] pses = this.GameObject.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in pses)
            {
                ps.playbackSpeed = speed;
            }
            GOEEntityEffectMove[] ems = this.GetComponents<GOEEntityEffectMove>();
            foreach (GOEEntityEffectMove em in ems)
            {
                em.MaxTime /= speed;
            }
        }

        public virtual void Pause()
        {
            if (!pause)
            {
                pause = true;
                if (this.GameObject == null)
                    return;
                ParticleSystem[] pses = this.GameObject.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem ps in pses)
                {
                    ps.Pause();
                }
                GOEEntityEffectMove[] ems = this.GetComponents<GOEEntityEffectMove>();
                foreach (GOEEntityEffectMove em in ems)
                {
                    em.Pause();
                }
            }
        }

        public virtual void Restart()
        {
            if (pause)
            {
                pause = false;
                if (this.GameObject == null)
                    return;
                ParticleSystem[] pses = this.GameObject.GetComponentsInChildren<ParticleSystem>();
                foreach (ParticleSystem ps in pses)
                {
                    ps.Play();
                }
                GOEEntityEffectMove[] ems = this.GetComponents<GOEEntityEffectMove>();
                foreach (GOEEntityEffectMove em in ems)
                {
                    em.Restart();
                }
            }
        }

        internal void SetPhysics( bool enable)
        {
            mEnablePhysics = enable;

            if ( this.GameObject == null )
            {
                return;
            }

            this.OnEnablePhysics( enable );		
 
        }

        internal void SetCollid(bool enable)
        {
            mEnableCollid = enable;

            if (this.GameObject == null)
            {
                return;
            }

            this.OnEnableCollid(enable);

        }

        private void OnEnableCollid( bool enable )
        {
            GameObject obj = this.GameObject;
            Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
            foreach (Collider collider in colliders)
            {
                collider.enabled = enable;
            }
        }

        private void OnEnablePhysics( bool enable )
        {
            GameObject obj = this.GameObject;
            if ( obj != null )
            {
                if ( obj.GetComponent<Animation>() != null )
                {
                    obj.GetComponent<Animation>().animatePhysics = enable;
                }

                OnEnableCollid(enable);

                Rigidbody[] righdbodeis = obj.GetComponentsInChildren<Rigidbody>( true );
                foreach ( Rigidbody r in righdbodeis )
                {
                    if ( !enable )
                    {
                        r.detectCollisions = false;
                        r.constraints = RigidbodyConstraints.FreezeAll;
                        r.useGravity = false;
                        r.Sleep();
                    }
                    else
                    {
                        r.detectCollisions = true;
                        r.constraints = RigidbodyConstraints.None;
                        r.useGravity = true;
                        r.collisionDetectionMode = CollisionDetectionMode.Discrete;//.Continuous;//.Discrete;//.ContinuousDynamic;
                        r.WakeUp();
                    }
                }
            }
        }

        private Action _onLoadResource;
        public Action OnLoadResource
        {
            get { return _onLoadResource; }
            set { _onLoadResource = value; }
        }

        protected override void OnLoadRes (string name)
        {
            base.OnLoadRes (name);
            
            this.GameObject.SetActive ( false );
            
            if ( !mCustomBound )
            {
                this.mBounds = GameObjectUtil.GetGameObjectBounds( mGameObject );

                Bounds temp = new Bounds();
                Vector3 center = mBounds.center;
                center.y /= Scale.x;
                temp.center = center;
                temp.size /= Scale.x;

                this.mOriBounds = temp;
            }

            SetParent(mParent);

            this.SetWeight();
            
            this.Visible	= mVisible;

            this.SetPhysics(mEnablePhysics);

            this.SetCollid(mEnableCollid);
            
            if ( Layer > 0 )
            {
                GameObjectUtil.SetLayer(this.GameObject, Layer);
            }
            
            foreach( GOEBaseComponent comp in mListComponent )
            {
                (comp as GOEEntityComponent).OnLoadGameObject();
            }
            setAutoDestory();
            checkAutoDestory();

            SetSpeed(effSpeed);

            onLoadPost();
            this.SetCastShadows(mCastShadows);

            this.SetAniCullingType(mAniCullingType);
            
            //addFakeShadow();
        }

        //public virtual void AddFakeShadow()
        //{ 
        
        //}

        protected virtual void onLoadPost()
        {
            renderers = GameObject.GetComponentsInChildren<Renderer>(false);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = true;
            }
            if (_onLoadResource != null)
                _onLoadResource();
        }

        private float _destoryTime = -1;
        internal void AutoDestory(float time)
        {
            _destoryTime = time;
            setAutoDestory();
        }

        private void setAutoDestory()
        {
            if (_destoryTime <= 0)
                return;
            if (mGameObject == null)
                return;
            ResConfig config = mGameObject.GetComponent<ResConfig>();
            if( config == null )
                config = mGameObject.AddComponent<ResConfig>();
            config.AutoDestory = true;
            config.LifeTime = _destoryTime;
            config.enabled = true;
            checkAutoDestory();
        }

        private ResConfig checkAutoDestory()
        {
            ResConfig config = mGameObject.GetComponent<ResConfig>();
            if (config != null && config.AutoDestory)
                config.OnDestroyHandler += Destroy;
            return config;
        }

        private void OnScaleChg()
        {            
            Bounds temp = mOriBounds;
            Vector3 center = temp.center;
            center.y *= Scale.x;
            temp.center = center;
            temp.size *= Scale.x;
            
            mBounds = temp;
        }

        protected virtual void SetAniCullingType( AnimationCullingType type )
        {            
            mAniCullingType = type;
            if ( this.ResStatus != ResStatus.OK )
            {
                return;
            }

            Animation[] anis = this.GameObject.GetComponentsInChildren<Animation>( true );
            foreach ( Animation ani in anis )
            {
                ani.cullingType = type;
            }
        }

        private void SetCastShadows( CastShadows cast )
        {
            mCastShadows = cast;

            if ( cast == CastShadows.Default )
            {
                return;
            }

            if ( this.ResStatus != ResStatus.OK )
            {
                return;
            }

            foreach ( Renderer render in renderers )
            {
                render.castShadows = cast == CastShadows.Yes;
            }
        }

        private void SetWeight()
        {
            Rigidbody[] righdbodeis = this.GameObject.GetComponentsInChildren<Rigidbody>( true );
            foreach ( Rigidbody body in righdbodeis )
            {
                body.mass *= (Weight / 100);
            }
        }

        public Transform GetBindPoint ( string name )
        {
            if ( this.ResStatus != ResStatus.OK )
            {
                return null;
            }
            return GameObjectUtil.GetBindPoint(GameObject, name);
        }

        public void SetColor(string name, Color color, string nodeName = "")
        {
            GOEEntityMaterial matComponent = GOEEntityComponent.Add<GOEEntityMaterial>(this);
            matComponent.SetColor(name, color, nodeName);
        }

        public void ReplaceShader(MaterialEffectInfo info)
        {
            GOEEntityMaterial matComponent = GOEEntityComponent.Add<GOEEntityMaterial>(this);
            matComponent.ReplaceShader(info);
        }

        public void ReplaceOriginalShader(MaterialEffectInfo info)
        {
            GOEEntityMaterial matComponent = GOEEntityComponent.Add<GOEEntityMaterial>(this);
            matComponent.ReplaceShader(info);
            matComponent.ChangeSharedMaterials();
        }
        public void UpdateSharedMaterials(string[] repalceShaderName)
        {
            GOEEntityMaterial matComponent = GOEEntityComponent.Add<GOEEntityMaterial>(this);
            matComponent.UpdateSharedMaterials(repalceShaderName);
        }

        public void OriginalShader()
        {
            GOEEntityMaterial matComponent = GOEEntityComponent.Add<GOEEntityMaterial>(this);
            matComponent.OriginalShader();
        }

        public void StopShader(string name, bool destory)
        {
            GOEEntityMaterial matComponent = GOEEntityComponent.Add<GOEEntityMaterial>(this);
            matComponent.StopShader(name, destory);
        }

        public bool IsOriginalShader()
        {
            GOEEntityMaterial matComponent = GetComponent<GOEEntityMaterial>();
            if (matComponent == null)
            {
                return true;
            }
            return matComponent.IsOriginalshader;
        }

        public string CurShaderName()
        {
            GOEEntityMaterial matComponent = GetComponent<GOEEntityMaterial>();
            if (matComponent == null)
            {
                return "GOE";
            }
            return matComponent.CurShaderName;
        }

        protected override void DestoryObject()
        {
            GOEEntityMaterial matComponent = GetComponent<GOEEntityMaterial>();
            if (matComponent != null)
            {
                matComponent.OriginalShader();
                matComponent.ClearEffect();
                DelComponent(matComponent);
            }
            base.DestoryObject();
        }

        public void ApplyForce(float forceMagtitude, Vector3 localForceDir, float mass =20 )
        {
            Animation animation = mGameObject.GetComponent<Animation>();
            animation.enabled = false;
            RagdollBuilder ragdollBuilder = mGameObject.GetComponent<RagdollBuilder>();
            if (ragdollBuilder == null)
            {
                ragdollBuilder = mGameObject.AddComponent<RagdollBuilder>();
            }
            ragdollBuilder.totalMass = mass;
            ragdollBuilder.RagdollBuild();
            Vector3 worldForceDir = mGameObject.transform.TransformDirection(localForceDir);
            Vector3 force = Vector3.Normalize(worldForceDir) * forceMagtitude;   
            Transform[] parts;
            parts = ragdollBuilder.parts;
            foreach (Transform part in parts)
            {
                Rigidbody rigidbody;
                rigidbody = part.gameObject.GetComponent<Rigidbody>();
                rigidbody.AddForce(force, ForceMode.Impulse);
            }
        }
       
         void RagdollCleanUpImmediately()
        {
            //RagdollBuilder ragdollBuilder = mGameObject.GetComponent<RagdollBuilder>();
            //if (ragdollBuilder == null)
            //    return;
            //ragdollBuilder.Cleanup();
        }

        public bool SetSortingOrder(int order)
        {
            if (renderers == null)
            {
                return false;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].sortingOrder = order;
            }
            return true;

        }
    }
}

