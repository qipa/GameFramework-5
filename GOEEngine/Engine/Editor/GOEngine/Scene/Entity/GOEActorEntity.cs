using System;
using UnityEngine;
using System.Collections.Generic;

namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class GOEActorEntity : Entity, IGOEActorEntity
    {
        private const string ANIM_SUFFIX = ".anim";
        protected int mMotionAilityFlags = 0;
        private Animator _animator;
        private float _moveSpeed = EngineDelegate.DefaultMoveSpeed;
        private float _dfMoveSpeed = EngineDelegate.DefaultMoveSpeed;
        private string _moveAni = "run";
        private string _standAni = "stand";
        private string _slowMoveAni = null;
        private string _breakAni = null;
        private string _animPrefix = null;
        float rotationSpeed = 0.3f;
        bool isRotationSpeedUniform = true;
        bool nextAnimNoCrossFade = false;
        bool rotateUp = false;
        float moveBlendTime = 0.3f;
        private GameObject _fakeShadowObject = null;
        protected List<string> _gBendAnime = null;
        private string _waitAni = "";
        bool isPathfinding = false;
        private string _blendPoint = null;
        public float MoveSpeed
        {
            set
            {
                if (_moveSpeed != value)
                {
                    _moveSpeed = value;
                    if (IsMoving)
                    {
                        setAniSpeed(MoveAnimation);
                    }
                }
            }
            get { return _moveSpeed; }
        }

        public float DefaultMoveSpeed { set { _dfMoveSpeed = value; } get { return _dfMoveSpeed; } }

        public bool RotateUpVector { get { return rotateUp; } set { rotateUp = value; } }

        public bool RotateSmooth { set; get; }
        public float RotationSpeed { get { return rotationSpeed; } set { rotationSpeed = value; } }
        public bool IsRotationSpeedUniform { get { return isRotationSpeedUniform; } set { isRotationSpeedUniform = value; } }

        public Quaternion RealRotation
        {
            get
            {
                Quaternion curRot = Rotation;
                if (RotateSmooth)
                {
                    var smooth = GetComponent<GOEEntityRotation>();
                    if (smooth != null && smooth.IsBegin)
                        curRot = smooth.Target;
                }
                return curRot;
            }
        }

        public bool IsRotateSmoothing
        {
            get
            {
                if (RotateSmooth)
                {
                    var smooth = GetComponent<GOEEntityRotation>();
                    if (smooth != null && smooth.IsBegin)
                        return true;
                }
                return false;
            }
        }
        public string MoveAnimation { set { _moveAni = value; } get { return _moveAni; } }

        public string AnimationPrefix { get { return _animPrefix; } set { _animPrefix = value; } }

        public float MoveBlendTime { get { return moveBlendTime; } set { moveBlendTime = value; } }

        public bool MoveSmooth { set; get; }

        public string StandAnimation { set { _standAni = value; } get { return _standAni; } }

        public string SlowMoveAnimation { get { return _slowMoveAni; } set { _slowMoveAni = value; } }

        public string BreakAnimation { get { return _breakAni; } set { _breakAni = value; } }

        public bool NextAnimationNoCrossfade { get { return nextAnimNoCrossFade; } set { nextAnimNoCrossFade = value; } }

        public bool IgnoreMoveAnimation { get; set; }
        public bool AutoPlaySubAnimation { get; set; }
        public string BlendPoint { 
            get {
                return string.IsNullOrEmpty(_blendPoint) ? "Bip001 Spine" : _blendPoint; 
            } 
            set { _blendPoint = value; }
        }

        public void AddBlendedAnime(string name)
        {
            if (_gBendAnime == null)
            {
                _gBendAnime = new List<string>();
            }
            if (!_gBendAnime.Contains(name))
            {
                _gBendAnime.Add(name);
            }
        }
        private float _aniSpeed = 1f;
        private Animation animation;
        public float AnimationSpeed 
        { 
            set 
            { 
                if(_aniSpeed != value)
                {
                    _aniSpeed = value;
                    foreach (Entity ent in mAttaches)
                    {
                        if (ent is GOEActorEntity)
                        {
                            (ent as GOEActorEntity).AnimationSpeed = value;
                        }
                    }
                }
            } 
            get { return _aniSpeed; } 
        }

        private bool _detour;
        /// <summary>
        /// 客户端寻路
        /// </summary>
        public bool ClientDetour {set{_detour = value;} get{ return _detour;}}
        
        public GOEActorEntity() : base()
        {
            //Layer = LayerDef.Player;
            this.AddComponent<GOEEntityMoveTo>();
            mMotionAilityFlags = (int)SamplePolyFlags.SAMPLE_POLYFLAGS_WALK
                | (int)SamplePolyFlags.SAMPLE_POLYFLAGS_SWIM
                | (int)SamplePolyFlags.SAMPLE_POLYFLAGS_DOOR
                | (int)SamplePolyFlags.SAMPLE_POLYFLAGS_JUMP;
        }

        public void SetMoveFlags(int flag)
        {
            mMotionAilityFlags = flag;
        }

        public int GetMotionAilityFlags()
        {
            return mMotionAilityFlags;
        }

        public void PlayAudio(string name, bool bLoop)
        {
            AudioSource source = GOERoot.GOEAudioMgr.AddSound(name, this.GameObject, false, false, AudioType.AUDIO_TYPE_EFFECT);
            source.loop = bLoop;
        }
        
        public void StopAudio( string name )
        {
            AudioSource[] sources = this.GameObject.GetComponents<AudioSource>();
            foreach (AudioSource audio in sources)
            {
                if (audio.name == name || audio.name + EngineFileUtil.m_oggExt == name)
                {
                    audio.Stop();
                    return;
                }
            }
        }

        public IEntity AddEffect( string name, string BindPoint, float time = -1.0f )
        {
            IEntity effect = AddAttach(name, BindPoint);
            if( time > 0 )
                (effect as Entity).AutoDestory(time);
            return effect;
        }
        
        public void DelEffect( string name )
        {
            IEntity effect = this.GetAttchByName(name);
            RemoveAttach(effect);
        }

        public void PlayAnimation(AnimationType type)
        {
            if (_animator == null)
            {
                CrossFade(type.ToString().ToLower());
            }
            AnimatorParam para = AnimatorConfig.GetAnimatorParam(type);
            if (para != null)
                para.ExecuteParam(_animator);
        }

        public void InterruptRotation(Quaternion target)
        {
            GOEEntityRotation rotate = this.GetComponent<GOEEntityRotation>();
            if (rotate != null)
            {
                rotate.InterruptRotation();
            }
            this.Rotation = target;
        }

        public void StartSmoothRotation(Quaternion target, float rotationSpeed = -1)
        {
            GOEEntityRotation rotate = this.GetComponent<GOEEntityRotation>();
            if (rotate == null)
            {
                rotate = this.AddComponent<GOEEntityRotation>();
            }
            rotate.IsRotationSpeedUniform = isRotationSpeedUniform;

            if (rotationSpeed < 0)
                rotationSpeed = this.rotationSpeed;
            rotate.StartRotation(target, rotationSpeed);
        }

        public void AbortSmoothRotation()
        {
            GOEEntityRotation rotate = this.GetComponent<GOEEntityRotation>();
            if (rotate != null)
            {
                rotate.Abort();
            }
        }

        public void PlayAnimation(string name, PlayMode mode = PlayMode.StopSameLayer, bool backToStand = false, float time = 0f)
        {
            if (this.GameObject == null || animation == null || string.IsNullOrEmpty(name))
                return;
            name = GetAniName(name);
            if (this.ResStatus != ResStatus.OK)
            {
                return;
            }
            AnimationClip animClip = animation.GetClip(name);
            if( animClip == null)
            {
                _waitAni = name;
                string animName = name + ANIM_SUFFIX;
                GOERoot.ResMgrImp.GetAsset(animName, (string cbName, UnityEngine.Object cbObject) =>
                    {
                        OnLoadAnim(name, cbObject);
                        if( _waitAni == name )
                        {
                            _PlayAnimation(name, mode, backToStand, time);
                        }
                    });
            }
            else
            {
                _PlayAnimation(name, mode, backToStand, time);
            }
            foreach (Entity ent in mAttaches)
            {
                if (ent is GOEActorEntity)
                {
                    GOEActorEntity ae = ent as GOEActorEntity;
                    if (!ae.IsVirtual)
                        continue;
                    ae.PlayAnimation(name, mode, backToStand, time);
                }
            }
        }
        private void _PlayAnimation(string name, PlayMode mode = PlayMode.StopSameLayer, bool backToStand = false, float time = 0f)
        {
            if (time > float.Epsilon)
            {
                AnimationState state = animation[name];
                state.time = time;
            }
            animation.Play(name, mode);
            setAniSpeed(name);
            appendStand(backToStand);
        }

        public bool CheckBlendAnim()
        {
            if (this.GameObject && animation)
            {
                if (_gBendAnime != null)
                {
                    for (int i=0; i<_gBendAnime.Count; i++)
                    {
                        if (animation.IsPlaying(_gBendAnime[i]))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        //public bool PlayBlend(string name, float weight)
        //{
        //    if (this.GameObject == null || animation == null || string.IsNullOrEmpty(name))
        //        return false;
        //    name = GetAniName(name);
        //    setAniSpeed(name);
        //    AnimationState state = animation[name];
        //    state.AddMixingTransform();
        //    animation.Blend(name, 1f);
        //    return true;
        //}
        private Dictionary<string,string> _nblendPoint;
        public bool PlayBlend(string name, float weight, string point)
        {
            if (this.GameObject == null || animation == null || string.IsNullOrEmpty(name))
                return false;
            name = GetAniName(name);
            AnimationClip animClip = animation.GetClip(name);
            if (animClip == null)
            {
                string animName = name + ANIM_SUFFIX;
                GOERoot.ResMgrImp.GetAsset(animName, (string cbName, UnityEngine.Object cbObject) =>
                {
                    OnLoadAnim(name, cbObject);
                });
                return false;
            }

            if (_nblendPoint == null)
              _nblendPoint = new Dictionary<string, string>();
            setAniSpeed(name);
            AnimationState state = animation[name];
            Transform pos = GetBindPoint(point);
            if (state == null || pos == null)
                return true;

            if (_nblendPoint.ContainsKey(name))
                _nblendPoint[name] = point;
            else
                _nblendPoint.Add(name, point);
            state.AddMixingTransform(pos,true);
            state.weight = weight;
            state.wrapMode = WrapMode.Once;
            state.blendMode = AnimationBlendMode.Blend;
            state.layer = 2;
            animation.Play(name, PlayMode.StopSameLayer);
            return true;
        }
        public void ClearBlend(string nblendName,string nPoint)
        {
            if (!string.IsNullOrEmpty(nblendName) && animation != null)
            {
                AnimationState state = animation[nblendName];
                if (state != null)
                {
                    if (_nblendPoint != null && _nblendPoint.ContainsKey(nblendName) 
                        && !string.IsNullOrEmpty(_nblendPoint[nblendName]))
                    {
                        _nblendPoint.Remove(nblendName);
                        state.RemoveMixingTransform(GetBindPoint(nPoint));
                    }
                    state.layer = 0;
                }
            }
        }

        public bool CrossFade(string name, float fadeLength = 0.3F, PlayMode mode = PlayMode.StopSameLayer, bool backToStand = true)
        {
            if (nextAnimNoCrossFade)
            {
                nextAnimNoCrossFade = false;
                PlayAnimation(name, mode, backToStand);
                return true;
            }
            if (this.GameObject == null || animation == null || string.IsNullOrEmpty(name))
                return false;
            name = GetAniName(name);
            AnimationClip animClip = animation.GetClip(name);
            if( animClip == null )
            {
                _waitAni = name;
                string animName = name + ANIM_SUFFIX;
                GOERoot.ResMgrImp.GetAsset(animName, (string cbName, UnityEngine.Object cbObject) =>
                    {
                        OnLoadAnim(name, cbObject);
                        if (_waitAni == name)
                        {
                            _CrossFade(name, fadeLength, mode, backToStand);
                        }
                    });
            }
            else
            {
                _CrossFade(name, fadeLength, mode, backToStand);
            }
            
            foreach(Entity ent in mAttaches)
            {
                if (ent is GOEActorEntity)
                {
                    GOEActorEntity ae = ent as GOEActorEntity;
                    if (!ae.IsVirtual)
                        continue;
                    ae.CrossFade(name, fadeLength, mode, backToStand);
                }
            }
            return true;
        }

        private void _CrossFade(string name, float fadeLength = 0.3F, PlayMode mode = PlayMode.StopSameLayer, bool backToStand = true)
        {
            animation.CrossFade(name, fadeLength, mode);
            setAniSpeed(name);
            appendStand(backToStand);
        }

        public void CrossFadeQueued(string name, float fadeLength = 0.3F, UnityEngine.QueueMode queue = QueueMode.CompleteOthers, UnityEngine.PlayMode mode = PlayMode.StopSameLayer, bool backToStand = false)
        {
            if (this.GameObject == null || animation == null || string.IsNullOrEmpty(name))
                return;
            name = GetAniName(name);
            if (nextAnimNoCrossFade)
            {
                nextAnimNoCrossFade = false;
                PlayQueued(name, queue, mode, backToStand);
                return;
            }
            foreach (Entity ent in mAttaches)
            {
                if (ent is GOEActorEntity)
                {
                    GOEActorEntity ae = ent as GOEActorEntity;
                    if (!ae.IsVirtual)
                        continue;
                    ae.CrossFadeQueued(name, fadeLength, queue, mode);
                }
            }
            AnimationClip animClip = animation.GetClip(name);
            if( animClip == null )
            {
            	string animName = name + ANIM_SUFFIX;
                GOERoot.ResMgrImp.GetAsset(animName, (string cbName, UnityEngine.Object cbObject) =>
                    {
                        OnLoadAnim(name, cbObject);
                    });
                return;
                //Debug.LogError("CrossFadeQueued must make sure that the animation clip is on the character, animation is -> " + name);
            }
            
            animation.CrossFadeQueued(name, fadeLength, queue, mode);
            setAniSpeed(name);
            appendStand(backToStand);
            
        }

        public void PlayQueued(string name, QueueMode queue = QueueMode.CompleteOthers, PlayMode mode = PlayMode.StopSameLayer, bool backToStand = false)
        {

            if (this.GameObject == null || animation == null || string.IsNullOrEmpty(name))
                return;
            name = GetAniName(name);
            if (this.ResStatus != ResStatus.OK)
            {
                return;
            }
            foreach (Entity ent in mAttaches)
            {
                if (ent is GOEActorEntity)
                {
                    GOEActorEntity ae = ent as GOEActorEntity;
                    if (!ae.IsVirtual)
                        continue;
                    ae.PlayQueued(name, queue, mode, backToStand);
                }
            }
            AnimationClip animClip = animation.GetClip(name);
            if (animClip == null)
            {
            	string animName = name + ANIM_SUFFIX;
                GOERoot.ResMgrImp.GetAsset(animName, (string cbName, UnityEngine.Object cbObject) =>
                {
                    OnLoadAnim(name, cbObject);
                });
                return;
                //Debug.LogError("CrossFadeQueued must make sure that the animation clip is on the character, animation is -> " + name);
            }
            animation.PlayQueued(name, queue, mode);
            setAniSpeed(name);
            appendStand(backToStand);
            
        }

        private void OnLoadAnim( string name, UnityEngine.Object clipObject )
        {
            AnimationClip ac = clipObject as AnimationClip;
            if (ac == null)
            {
                Debug.LogError("animation loaded: " + name + " but animation clip is null ......");
                return;
            }
            animation.AddClip(ac, name);
        }

        private void appendStand(bool backToStand)
        {
            if (backToStand)
                PlayQueued(StandAnimation);
        }

        public void Stop()
        {
            if (this.GameObject == null || animation == null)
            {
                return; 
            }
            animation.Stop();
            foreach (Entity ent in mAttaches)
            {
                if (ent is GOEActorEntity)
                {
                    if (!ent.IsVirtual)
                        continue;                    
                    (ent as GOEActorEntity).Stop();
                }
            }
        }

        private List<AnimationState> _playingStates = new List<AnimationState>();
        public void PauseAnimation()
        {
            if (this.GameObject == null || animation == null)
                return;
            foreach (AnimationState st in animation)
            {
                if (st.enabled)
                {
                    st.speed = 0;
                    _playingStates.Add(st);
                }
            }
            foreach (Entity ent in mAttaches)
            {
                if (ent is GOEActorEntity)
                {
                    if (!ent.IsVirtual)
                        continue;                    
                    (ent as GOEActorEntity).PauseAnimation();
                }
            }
        }

        public void RestartAnimation()
        {
            foreach (AnimationState st in _playingStates)
            {
                if (st)
                    st.speed = _aniSpeed;
            }
            _playingStates.Clear();
            foreach (Entity ent in mAttaches)
            {
                if (ent is GOEActorEntity)
                {
                    if (!ent.IsVirtual)
                        continue;                    
                    (ent as GOEActorEntity).RestartAnimation();
                }
            }
        }

        private void setAniSpeed(string name)
        {
            if (animation == null)
                return;
            var anim = animation[name];
            if (anim)
            {
                if (name == GetAniName(_moveAni))
                {
                    float sp = _moveSpeed / _dfMoveSpeed;
                    if (anim.speed != sp)
                        anim.speed = sp;
                }
                else if (anim.speed != _aniSpeed)
                {
                    anim.speed = _aniSpeed;
                }
            }
        }

        private string GetAniName( string name )
        {
            string pname =null;
            if (string.IsNullOrEmpty(_animPrefix))
                pname = Name.Replace(EngineFileUtil.m_prefabExt, "") + "_";
            else
                pname = _animPrefix + "_";
            if (name.IndexOf("_") != -1)
                name = name.Substring(name.LastIndexOf("_") + 1);
            return pname + name;
        }


        public IGOEEntityAct PlayAct(string name, ActType mask = ActType.None, float speed = 1, bool immediatelyRun = false)
        {
            GOEEntityAct mMainAct;
            GOEEntityAct[] acts = this.GetComponents<GOEEntityAct>();
            if (!GOERoot.IsEditor || acts.Length == 0 || acts[0].Enable)
            {
                mMainAct = GOEEntityAct.GetActInstance(name);
                this.AddComponent(mMainAct);
            }
            else
            {
                mMainAct = acts[0];
            }
            mMainAct.TypeFilters = mask;
            mMainAct.ActSpeed = speed;
            mMainAct.ImmediatelyRun = immediatelyRun;
            if(mMainAct.ActName != name )
            {
                mMainAct.ActName = name;
                mMainAct.LoadAndPlay();
            }
            else
            {
                mMainAct.play();
            }
            return mMainAct;
        }

        public override void Pause()
        {
            base.Pause();
            PauseAnimation();
            PauseAct();
            PauseMove();
        }

        public override void Restart()
        {
            base.Restart();
            RestartAnimation();
            RestartAct();
            RestartMove();
        }
        
        public void PauseMove()
        {
            GOEEntityMoveTo mt = this.GetComponent<GOEEntityMoveTo>();
            if (mt != null)
                mt.PauseMove();
            GOEEntityMoveToDirect md = this.GetComponent<GOEEntityMoveToDirect>();
            if (md != null)
                md.Pause();
        }

        public void RestartMove()
        {
            GOEEntityMoveTo mt = this.GetComponent<GOEEntityMoveTo>();
            if (mt != null)
                mt.RestartMove();
            GOEEntityMoveToDirect md = this.GetComponent<GOEEntityMoveToDirect>();
            if (md != null)
                md.Restart();
        }

        public void PauseAct()
        {
            GOEEntityAct[] acts = this.GetComponents<GOEEntityAct>();
            foreach (GOEEntityAct act in acts)
            {
                act.Pause();
            }
        }

        public void RestartAct()
        {
            GOEEntityAct[] acts = this.GetComponents<GOEEntityAct>();
            foreach (GOEEntityAct act in acts)
            {
                if (!act.LifeOver)
                    act.Restart();
            }
        }
        public float GetActLifeTime(string name)
        {
            GOEEntityAct[] acts = this.GetComponents<GOEEntityAct>();
            for (int i = 0; i < acts.Length;i++ )
            {
                if (acts[i].ActName == name)
                {
                    return Math.Max(acts[i].LiftTime - acts[i].LocalTime, 0f);
                }
            }
            return 0f;
        }

        public void StopAct(string name)
        {
            GOEEntityAct[] acts = this.GetComponents<GOEEntityAct>();
            foreach (GOEEntityAct act in acts)
            {
                if (act.ActName == name)
                {
                    act.Stop();
                }
            }
        }

        public void StopAct(HashSet<string> exclude = null)
        {
            GOEEntityAct[] acts = this.GetComponents<GOEEntityAct>();
            foreach (GOEEntityAct act in acts)
            {
                if (exclude != null && exclude.Contains(act.ActName))
                    continue;
                act.Stop();
            }
        }

        internal override void Destroy()
        {
            StopAct();
            if (_fakeShadowObject != null)
            {
                _fakeShadowObject.transform.parent = null;
                UnityEngine.GameObject.Destroy(_fakeShadowObject);
                _fakeShadowObject = null;
            }
            base.Destroy();

        }
        public bool IsPathfinding
        {
            get
            {
                return isPathfinding && IsMoving;
            }
        }

        public void MoveDir(float degree)
        {
            isPathfinding = false;
            this.GetComponent<GOEEntityMoveTo>().MoveDir(degree);
        }

        public void MoveStraight(Vector3 end, float speed = -1, bool keeFace = false, bool keepAni = false, Action onReach = null)
        {
            isPathfinding = true;
            this.GetComponent<GOEEntityMoveTo>().MoveStraight(end, speed, keeFace, keepAni, onReach);
        }

        public void MoveStraightByTime(Vector3 end, float time, bool keeFace, bool keepAni)
        {
            isPathfinding = true;
            GOEEntityMoveToDirect move = this.AddComponent<GOEEntityMoveToDirect>();
            move.EndPos = end;
            move.MaxTime = time;
            move.StartPos = Position;
        }

        public List<Vector3> MoveTo(Vector3 pos, Action onReach = null)
        {
            isPathfinding = true;
            return GetComponent<GOEEntityMoveTo>().MoveTo(pos, onReach);
        }

        public void MoveTo(List<Vector3> path, Action onReach)
        {
            isPathfinding = true;
            GetComponent<GOEEntityMoveTo>().MoveTo(path, onReach);
        }

        public List<Vector3> MoveClose(Vector3 pos, Action onClose, float dis = 1.0f)
        {
            isPathfinding = true;
            return GetComponent<GOEEntityMoveTo>().MoveClose(pos, onClose, dis);
        }

        public void StopMove()
        {
            this.GetComponent<GOEEntityMoveTo>().StopMove();
            var move = this.GetComponent<GOEEntityMoveToDirect>();
            if (move != null)
                move.StopMove();
        }

        public List<Vector3> MoveTo(float x, float y, Action onReach = null)
        {
            isPathfinding = true;
            return GetComponent<GOEEntityMoveTo>().MoveTo(x, y, onReach);
        }

        public void ResetRotation()
        {
            GetComponent<GOEEntityMoveTo>().ResetRotation();
        }

        protected override void OnLoadRes(string name)
        {
            base.OnLoadRes(name);
        }

        public IGOEActorEntity AddActorAttach(string name, string bd)
        {
            Transform tm = GetBindPoint(bd);
            return AddActorAttach(name, tm) as IGOEActorEntity;
        }


        public IEntity AddActorAttach(string name, Transform tm)
        {
            Entity attach = this.Scene.AddActor(name) as Entity;
            attach.Parent = tm;
            mAttaches.Add(attach);
            return attach;
        }

        protected override void onLoadPost()
        {
            _animator = GameObject.GetComponent<Animator>();
            animation = GameObject.GetComponent<Animation>();
            if (AutoPlaySubAnimation)
            {
                Transform[] trans = GameObject.GetComponentsInChildren<Transform>();
                foreach (Transform tran in trans)
                {
                    if (tran.gameObject == GameObject)
                        continue;
                    if (tran.gameObject.GetComponent<Animation>() != null && tran.gameObject.GetComponent<Animation>().GetClipCount() > 1)
                    {
                        GOEActorEntity part = this.AddActorAttach(tran.gameObject.name, tran.parent) as GOEActorEntity;
                        part.IsVirtual = true;
                        part.Rotation = tran.localRotation;
                        part.GameObject = tran.gameObject;
                    }
                }
            }
            string sname = GetAniName(_standAni);
            if (animation != null && animation[sname] != null && !animation[sname].enabled)
                CrossFade(_standAni);
            base.onLoadPost();
        }

        protected override void DestoryObject()
        {
            if(!IsVirtual)
                base.DestoryObject();
            removeAllAttach();
            animation = null;
            if (_fakeShadowObject != null)
                GameObject.Destroy(_fakeShadowObject);
        }

        private Action<Vector3, Vector3> _onUserMoveTo;
        public Action<Vector3, Vector3> OnUserMoveTo
        {
            get { return _onUserMoveTo; }
            set { _onUserMoveTo = value; }
        }

        private Action _onUserStopMove;
        public Action OnUserStopMove
        {
            get { return _onUserStopMove; }
            set { _onUserStopMove = value; }
        }

        public Action<IEntity> OnCollision { get; set; }

        public bool IsMoving
        {
            get { return this.GetComponent<GOEEntityMoveTo>().IsMoving; }
        }

        public void AddFakeShadow()
        {
            if (GameObject.GetComponentsInChildren<Renderer>().Length == 0)
            {
                return;
            }

            UnityEngine.Object FakeShadowobject = Resources.Load("FakeShadow/FakeShadow") as UnityEngine.Object;
            if (FakeShadowobject != null)
            {
                _fakeShadowObject = GameObject.Instantiate(FakeShadowobject) as GameObject;
                //_fakeShadowObject.layer = LayerDef.FakeShadow;
                _fakeShadowObject.transform.parent = GameObject.transform;
                _fakeShadowObject.transform.localPosition = Vector3.zero;
                _fakeShadowObject.transform.localScale = Vector3.one * 2;
            }

        }
    }
}

