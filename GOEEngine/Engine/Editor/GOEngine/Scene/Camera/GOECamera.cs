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
 class GOECamera : ComponentObject, IGOECamera
    {
        List<IGOEPostEffect> postEffects;
        private Camera _camera = null;

        private const int CLIP_NONE = 0;
        private const int CLIP_LEFT = 1;
        private const int CLIP_RIGHT = 1 << 1;
        private const int CLIP_TOP = 1 << 2;
        private const int CLIP_BOTTOM = 1 << 3;
        private const int CLIP_FAR = 1 << 4;
        private const int CLIP_NEAR = 1 << 5;

        private CameraControl _cameraControl = null;
        private LightFaceEffect _lightFaceEffect = null;
        IEntity mLookAt;


        public GOECamera()
        {
            _camera = this.CreateDefCamera();
            Start();
        }

        private Action<Camera> _onMainCameraReseted;
        public Action<Camera> OnMainCameraReseted
        {
            set { _onMainCameraReseted = value; }
            get { return _onMainCameraReseted; }
        }
        public Camera Camera
        {
            get { return _camera; }
            set { SetCamera(value); }
        }

        public Vector3 Position
        {
            get { return _camera.transform.position; }
            set { _camera.transform.position = value; }
        }

        public float Fov
        {
            get
            {
                if (!_cameraControl)
                    return 0;
                return _cameraControl.Fov;
            }
            set
            {
                if (!_cameraControl)
                    return;
                _cameraControl.Fov = value;
            }
        }


        public float Near
        {
            get
            {
                if (!_cameraControl)
                    return 0;
                return _camera.nearClipPlane;
            }
            set
            {
                if (!_cameraControl)
                    return;
                _camera.nearClipPlane = value;
            }
        }

        public float Far
        {
            get
            {
                if (!_cameraControl)
                    return 0;
                return _camera.farClipPlane;
            }
            set
            {
                if (!_cameraControl)
                    return;
                _camera.farClipPlane = value;
            }
        }

        public float Radius
        {
            get
            {
                if (!_cameraControl)
                    return 0;
                return _cameraControl.radius;
            }
            set
            {
                if (!_cameraControl)
                    return;
                _cameraControl.radius = value;
            }
        }

        public float Y
        {
            get
            {
                if (!_cameraControl)
                    return 0;
                return _cameraControl.Y;
            }
            set
            {
                if (!_cameraControl)
                    return;
                _cameraControl.Y = value;
            }
        }

        public float YOffset
        {
            set
            {
                if (!_cameraControl)
                    return;
                _cameraControl.YOffset = value;
            }
        }


        public float Angle
        {
            get
            {
                if (!_cameraControl)
                    return 0;
                return _cameraControl.angle;
            }
            set
            {
                if (!_cameraControl)
                    return;
                _cameraControl.angle = value;
            }
        }

        public Vector3 Offset
        {
            get { 
                if (!_cameraControl)
                {
                    return Vector3.zero;
                }
                return _cameraControl.offSet; }
            set
            {
                if (!_cameraControl)
                    return;
                _cameraControl.offSet = value;
            }
        }

        public Vector3 OffsetSmooth
        {
            set
            {
                if (!_cameraControl)
                    return;
                _cameraControl.OffsetSmoothTo = value;
            }

        }

        public void CacheValue(float radius, float y, float angle)
        {
            _cameraControl.CacheValue(radius, y, angle);
        }

        public IEntity LookAtTarget
        {
            get { return mLookAt; }
        }

        public float LookAtOffset
        {
            get { return _cameraControl.lookatOffset; }
            set
            {
                if (!_cameraControl)
                    return;
                _cameraControl.lookatOffset = value;
            }
        }

        private void SetCamera(Camera cam)
        {
            if (cam == null)
            {
                return;
            }

            if (cam == _camera)
            {
                return;
            }

            if (this._camera != null)
            {
                UnityEngine.Object.Destroy(this._camera.gameObject);
            }

            this._camera = cam;
        }

        public void EnterScene()
        {
            if (this._camera != null)
            {
                UnityEngine.Object.Destroy(this._camera.gameObject);
            }

            Camera cam = Camera.main;
            if (cam == null)
            {
                cam = CreateDefCamera();
            }


            int tempLayer = 0;

            tempLayer |= 1 << LayerDef.COLLIDER;

            tempLayer |= 1 << LayerDef.Walk_Surface;
            tempLayer |= 1 << LayerDef.LIGHT_FACE;
            tempLayer |= 1 << LayerDef.UI;
            tempLayer |= 1 << LayerDef._3DUI;
            tempLayer |= 1 << LayerDef.UIMODEL;

            cam.cullingMask = ~tempLayer;

            UnityEngine.Object.DontDestroyOnLoad(cam.gameObject);
            if (_camera != cam)
            {
                this._camera = cam;
                if (OnMainCameraReseted != null)
                    OnMainCameraReseted(_camera);
            }
            _cameraControl = _camera.gameObject.GetComponent<CameraControl>() as CameraControl;
            if (_lightFaceEffect)
                _lightFaceEffect.RemoveImageEffect(OnRenderPostEffect);
            if (postEffects != null)
            {
                foreach (var i in postEffects)
                {
                    i.Dispose();
                }
                postEffects.Clear();
            }
            _lightFaceEffect = _camera.gameObject.GetComponent<LightFaceEffect>() as LightFaceEffect;
            if (_lightFaceEffect)
                _lightFaceEffect.AddImageEffect(OnRenderPostEffect);

            GOECameraComponent comp;
            for (int i = 0; i < mListComponent.Count; i++)
            {
                comp = mListComponent[i] as GOECameraComponent;
                comp.EnterScene();
            }


            this._camera.depthTextureMode |= DepthTextureMode.None;

            if (GOERoot.SceneImp.Hero != null && GOERoot.SceneImp.Hero.ResStatus == ResStatus.OK)
                LookAt(GOERoot.SceneImp.Hero);
        }

        public void SetShift(float radius, float y, float angle)
        {
            _cameraControl.radius = radius;
            _cameraControl.Y = y;
            _cameraControl.angle = angle;
        }

        public void SmoothRadius(float value, float begin, float time)
        {
            _cameraControl.SmoothRadius(value, begin, time);
        }

        public void SmoothY(float value, float begin, float time)
        {
            _cameraControl.SmoothY(value, begin, time);
        }

        public void SmoothAngle(float value, float begin, float time)
        {
            if (_cameraControl != null)
                 _cameraControl.SmoothAngle(value, begin, time);
        }

        public void InterruptSmooth()
        {
            if (_cameraControl != null)
                _cameraControl.InterruptSmooth = true;
        }

        public bool IsSmooth
        {
            get {
                if (_cameraControl != null)
                    return _cameraControl.isSmooth;
                return false;
            }
            set {
                if (_cameraControl != null)
                    _cameraControl.isSmooth = value;
            }
        }

        public void Shake(float force, float spring, float attenuation, float existTime)
        {
            GOECameraShake shake = GOECameraShake.AddCameraShake();
            if (shake != null)
            {
                shake.Enable = false;
                shake.force = force;
                shake.spring = spring;
                shake.attenuation = attenuation;
                shake.ExistTime = shake.ExistTime;
                shake.Enable = true;
            }
            else
                UnityEngine.Debug.LogWarning("Shake component == null");
        }

        internal void LeaveScene()
        {
            UnityEngine.Object.DestroyImmediate(this._camera.gameObject);

            this._camera = CreateDefCamera();

        }

        public bool IsPosVisible(Vector3 pos)
        {
            int flag = 0xFF;
            flag &= ClipWorldPos(pos);
            if (flag == CLIP_NONE)
                return true;

            return false;
        }

        public bool IsPosZVisible(Vector3 pos)
        {
            Vector3 vpPos = WorldToViewportPoint(pos);
            if (vpPos.y > 0 && vpPos.y < 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsAABBVisible(Bounds aabb)
        {
            if (MathUtil.IsBoundsInvalide(aabb)) return false;

            int flag = 0xFF;

            Vector3 pos = aabb.min;
            flag &= ClipWorldPos(pos);
            if (flag == CLIP_NONE)
                return true;

            pos.z = aabb.max.z;
            flag &= ClipWorldPos(pos);
            if (flag == CLIP_NONE)
                return true;

            pos.y = aabb.max.y;
            flag &= ClipWorldPos(pos);
            if (flag == CLIP_NONE)
                return true;

            pos.z = aabb.min.z;
            flag &= ClipWorldPos(pos);
            if (flag == CLIP_NONE)
                return true;

            pos.x = aabb.max.x;
            flag &= ClipWorldPos(pos);
            if (flag == CLIP_NONE)
                return true;

            pos.z = aabb.max.z;
            flag &= ClipWorldPos(pos);
            if (flag == CLIP_NONE)
                return true;

            pos.y = aabb.min.y;
            flag &= ClipWorldPos(pos);
            if (flag == CLIP_NONE)
                return true;

            pos.z = aabb.min.z;
            flag &= ClipWorldPos(pos);
            if (flag == CLIP_NONE)
                return true;

            return false;
        }

        public Vector3 WorldToViewportPoint(Vector3 point)
        {
            return _camera.WorldToViewportPoint(point);
        }

        public void LookAt(Vector3 pos, Vector3 up)
        {
            _camera.transform.LookAt(pos, up);
        }


        public void LookAt(IEntity entity)
        {
            if (_cameraControl != null)
            {
                mLookAt = entity;
                _cameraControl.SetTarget(entity.GameObject.transform);
            }

        }

        public void ChangeLookAt(GOEngine.IEntity entity, float speed = 0.5f)
        {
            if (_cameraControl != null)
            {
                mLookAt = entity;
                _cameraControl.cameraSmoothing = speed;
                _cameraControl.ChangeTarget(entity.GameObject.transform);
            }

            if (_lightFaceEffect != null)
            {
                //Debug.Log("----------------changelook at " + entity.GameObject.GetHashCode());
                _lightFaceEffect.LookAtTarget = entity.GameObject.transform;
            }
        }
        private int ClipWorldPos(Vector3 pos)
        {
            int flag = CLIP_NONE;
            Vector3 vpPos = WorldToViewportPoint(pos);

            if (vpPos.x < 0)
                flag |= CLIP_LEFT;
            else if (vpPos.x > 1)
                flag |= CLIP_RIGHT;

            if (vpPos.y < 0)
                flag |= CLIP_BOTTOM;
            else if (vpPos.y > 1)
                flag |= CLIP_TOP;

            if (vpPos.z < Near)
                flag |= CLIP_NEAR;
            else if (vpPos.z > Far)
                flag |= CLIP_FAR;

            return flag;
        }

        private Camera CreateDefCamera()
        {
            GameObject go = new GameObject();

            go.name = "default_camera";

            Camera cam = go.AddComponent<Camera>();
            cam.tag = "MainCamera";

            AudioListener listener = go.GetComponent<AudioListener>();
            if (listener != null)
            {
                listener.enabled = false; ;
            }

            cam.cullingMask = 0;

            UnityEngine.Object.DontDestroyOnLoad(go);

            return cam;
        }

        public void DisableMoveCameras()
        {
            if (_cameraControl != null)
            {
                _cameraControl.enabled = false;
            }

        }

        public void ResetToDefault()
        {
            if (_cameraControl != null)
            {
                _cameraControl.enabled = true;
            }
        }

        public void PlayIntroAnimation()
        {
            if (_cameraControl != null)
            {
                _cameraControl.PlayAnimation();
            }
        }

        public T GetPostEffect<T>() where T : class, IGOEPostEffect, new()
        {
            if (postEffects == null)
                postEffects = new List<IGOEPostEffect>();

            T res = null;
            for (int i = 0; i < postEffects.Count; i++)
            {
                var pe = postEffects[i];
                res = pe as T;
                if (res != null)
                    return res;
            }

            res = new T();
            postEffects.Add(res);
            return res;
        }

        public void RemovePostEffect<T>() where T : class, IGOEPostEffect, new()
        {
            if (postEffects == null)
                postEffects = new List<IGOEPostEffect>();
            
            T res = null; 
            
            for (int i = 0; i < postEffects.Count; i++)
            {
                var pe = postEffects[i];
                res = pe as T;
                if (res != null)
                {
                    postEffects.RemoveAt(i);
                    res.Dispose();
                    return;
                }
            }
        }

        internal override void Update()
        {
            base.Update();

            if (postEffects != null && _lightFaceEffect)
            {
                for (int i = 0; i < postEffects.Count; i++)
                {
                    var pe = postEffects[i];
                    if (pe.Enabled)
                    {
                        if (!pe.EffectMaterial)
                            pe.Initialize(_lightFaceEffect);

                        pe.Update(_lightFaceEffect);
                    }
                }
            }
        }

        void OnRenderPostEffect(RenderTexture sourceTexture, RenderTexture destTexture)
        {
            if (postEffects != null && _lightFaceEffect)
            {
                for (int i = 0; i < postEffects.Count; i++)
                {
                    var pe = postEffects[i];

                    if (pe.Enabled)
                    {
                        if (!pe.EffectMaterial)
                            pe.Initialize(_lightFaceEffect);
                        else
                        {
                            pe.UpdateMaterialParameter();
                            // @xl 解决pc上后效不能正常显示的bug //
#if UNITY_IPHONE
                            Graphics.Blit(sourceTexture, sourceTexture, pe.EffectMaterial);
#else
                            RenderTexture mTempRt = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
                            Graphics.Blit(sourceTexture, mTempRt, pe.EffectMaterial);
                            Graphics.Blit(mTempRt, destTexture);
                            RenderTexture.ReleaseTemporary(mTempRt);
#endif                      
                        }
                    }
                }
            }
        }
    }
}

