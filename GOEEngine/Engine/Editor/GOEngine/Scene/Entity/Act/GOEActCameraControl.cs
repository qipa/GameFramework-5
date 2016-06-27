using System;
using UnityEngine;

namespace GOEngine.Implement
{
    [ActTypeName(ActType.CameraControl)]
    [System.Reflection.Obfuscation(Exclude = true)]
    [DisplayName("摄像机控制")]
#if UNITY_EDITOR
    public
#else
    internal
#endif
 class GOEActCameraControl : GOEActComponent
    {

        float oriRadius, oriY, oriAngel, oriFov;
        float revertTime = 0f;
        float slowStart = 0f;
        float slowEnd = 0f;
        float moveTime = 0.5f;
        UnityEngine.Vector3 oriPos, oriHeightOffset;
        IEntity oriLookAt;
        bool initialized = false;
        private bool mSelfActive = false;

        [DisplayName("是否只对主角有效")]
        public bool SelfActive
        {
            get { return mSelfActive; }
            set { mSelfActive = value; }
        }
        [DisplayName("半径")]
        public float Radius { get; set; }

        [DisplayName("高度")]
        public float Y { get; set; }

        [DisplayName("角度")]
        public float Angle { get; set; }

        [DisplayName("高度偏移")]
        public float HeightOffset { get; set; }

        [DisplayName("角度是否相对于角色")]
        public bool AngleIsRelative { get; set; }

        [DisplayName("聚焦自身")]
        public bool FocusSelf { get; set; }

        [DisplayName("聚焦目标")]
        public bool FocusTarget { get; set; }

        [DisplayName("聚焦中心点")]
        public bool FocusMiddle { get; set; }

        [DisplayName("FOV")]
        public float Fov { get; set; }

        [DisplayName("是否瞬间切换")]
        public bool InstantChange { get; set; }

        [DisplayName("相机移动时间")]
        public float MoveTime { get { return moveTime; } set { moveTime = value; } }

        [DisplayName("相机复位时间")]
        public float RevertTime { get { return revertTime; } set { revertTime = value; } }

        [DisplayName("慢镜头开始时间")]
        public float SlowStart { get { return slowStart; } set { slowStart = value; } }

        [DisplayName("慢镜头结束时间")]
        public float SlowEnd { get { return slowEnd; } set { slowEnd = value; } }

        [DisplayName("结束时还原原始设置")]
        public bool RestoreToOriginal { get; set; }

        private float lifeTime = 0f;
        internal override void OnTrigger()
        {
            base.OnTrigger();
            if (SelfActive)
            {
                if (GOERoot.SceneImp == null || this.Entity != GOERoot.SceneImp.Hero)
                {
                    this.Enable = false;
                    return;
                }
            } 
            oriRadius = GOERoot.GOECamera.Radius;
            oriY = GOERoot.GOECamera.Y;
            oriHeightOffset = GOERoot.GOECamera.Offset;
            oriAngel = GOERoot.GOECamera.Angle;
            oriFov = GOERoot.GOECamera.Fov;
            oriLookAt = GOERoot.GOECamera.LookAtTarget;
            oriPos = oriLookAt.Position;

            if (Radius != 0)
            {
                if (!InstantChange)
                    GOERoot.GOECamera.SmoothRadius(Radius, 0, moveTime);
                else
                    GOERoot.GOECamera.Radius = Radius;
            }
            if (Y != 0)
            {
                if (!InstantChange)
                    GOERoot.GOECamera.SmoothY(Y, 0, moveTime);
                else
                    GOERoot.GOECamera.Y = Y;
            }
            if (Angle != 0)
            {
                float angle = Angle;

                if (AngleIsRelative)
                {
                    Vector3 tarPos = Camera.main.WorldToScreenPoint(Entity.Position + Entity.GameObject.transform.forward);
                    Vector3 selfPos = Camera.main.WorldToScreenPoint(Entity.Position);
                    if (tarPos.x < selfPos.x)
                        angle = Entity.Rotation.eulerAngles.y - Angle;
                    else
                        angle = Entity.Rotation.eulerAngles.y + Angle;
                }
                if (!InstantChange)
                    GOERoot.GOECamera.SmoothAngle(angle, 0, moveTime);
                else
                {
                    angle = (float)((angle / 180) * Math.PI);
                    GOERoot.GOECamera.Angle = angle;
                }
            }
            if (HeightOffset != 0)
            {
                GOERoot.GOECamera.Offset = new UnityEngine.Vector3(0, HeightOffset, 0);
            }
            if (Fov != 0)
            {
                GOERoot.GOECamera.Fov = Fov;
            }
            if (FocusSelf)
            {
                GOERoot.GOECamera.ChangeLookAt(Entity);
            }
            if (FocusTarget && EntityAct.TargetList.Count > 0)
                GOERoot.GOECamera.ChangeLookAt(EntityAct.TargetList[0]);
            if (FocusMiddle && (EntityAct.TargetList.Count > 0 || EntityAct.TargetPos != Vector3.zero))
            {

                UnityEngine.Vector3 pos;
                if (EntityAct.TargetList.Count > 0)
                    pos = (Entity.Position + EntityAct.TargetList[0].Position) / 2;
                else
                {
                    pos = (Entity.Position + EntityAct.TargetPos) / 2;
                }
                oriLookAt.Position = pos;
                GOERoot.GOECamera.LookAt(oriLookAt);

            }
            initialized = true;
            this.Enable = true;
            if (RevertTime <= float.Epsilon)
                this.Enable = false;
        }

        internal override void Update()
        {
            base.Update();
            if (SlowEnd > 0f )
            {
                if (LocalTime > SlowStart)
                {
                    if (LocalTime < slowEnd)
                        Time.timeScale = 0.2f;
                    else
                        Time.timeScale = 1.0f;
                }
            }
            if (LocalTime > Math.Max(moveTime + revertTime,slowEnd))
            {
                this.LifeOver = true ;
            }
        }
        internal override void OnDestroy()
        {
            base.OnDestroy();
            if (initialized)
            {
                if (SlowEnd > 0f && lifeTime > SlowStart)
                    Time.timeScale = 1.0f;
                lifeTime = 0f;
            }
            if (RestoreToOriginal && initialized)
            {
                if (!InstantChange)
                {
                    GOERoot.GOECamera.SmoothRadius(oriRadius, 0, 0.5f);
                    GOERoot.GOECamera.SmoothY(oriY, 0, 0.5f);
                    GOERoot.GOECamera.SmoothAngle(oriAngel, 0, 0.5f);
                }
                else
                {
                    GOERoot.GOECamera.Radius = oriRadius;
                    GOERoot.GOECamera.Y = oriY;
                    GOERoot.GOECamera.Angle = oriAngel;
                }
                GOERoot.GOECamera.Fov = oriFov;
                GOERoot.GOECamera.Offset = oriHeightOffset;
                oriLookAt.Position = oriPos;
                GOERoot.GOECamera.ChangeLookAt(oriLookAt);
            }
                Enable = false;
        }

        internal override void Pause()
        {
            base.Pause();
        }

        internal override void Restart()
        {
            base.Pause();
        }

        internal override void GetResAsset(System.Collections.Generic.HashSet<string> setRes)
        {

        }

        internal override void PreLoad()
        {

        }

    }
}

