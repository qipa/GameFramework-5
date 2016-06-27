using System;
using UnityEngine;
namespace GOEngine
{
    public interface IGOECamera
    {
        /// <summary>
        /// 当前主镜头
        /// </summary>
        global::UnityEngine.Camera Camera { get; set; }
        float Far { get; set; }
        float Fov { get; set; }
        float Near { get; set; }
        float Radius { get; set; }
        float Y { get; set; }
        float Angle { get; set; }
        bool IsSmooth { get; set; }
        IEntity LookAtTarget { get; }

        float LookAtOffset { get; set; }
        Vector3 Offset { get; set; }
        Vector3 OffsetSmooth { set; }


        float YOffset { set; }


        /// <summary>
        /// 镜头跟随对象，对象移动，镜头会跟随移动
        /// </summary>
        /// <param name="entity">场景对象</param>
        void LookAt(global::GOEngine.IEntity entity);
        /// <summary>
        /// 镜头看向某点
        /// </summary>
        /// <param name="pos">位置</param>
        /// <param name="up">方位</param>
        void LookAt(global::UnityEngine.Vector3 pos, global::UnityEngine.Vector3 up);

        /// <summary>
        /// 跟换镜头跟随目标
        /// </summary>
        /// <param name="entity">场景对象</param>
        /// <param name="speed">移动速度</param>
        void ChangeLookAt(global::GOEngine.IEntity entity, float speed = 0.5f);
        /// <summary>
        /// 镜头位置
        /// </summary>
        global::UnityEngine.Vector3 Position { get; set; }
        /// <summary>
        /// 停止镜头跟随主角移动
        /// </summary>
        void DisableMoveCameras();
        /// <summary>
        /// 恢复镜头跟随移动
        /// </summary>
        void ResetToDefault();
        /// <summary>
        /// 主镜头变化时派发，一般发生在场景切换时，主镜头变为新场景的镜头
        /// </summary>
        Action<Camera> OnMainCameraReseted { set; get; }
        /// <summary>
        /// 根据直径和角度来设定镜头便宜
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="y"></param>
        /// <param name="angle"></param>
		void SetShift(float radius, float y, float angle);
		/// <summary>
		/// 根据直径和角度来缓动镜头半径
		/// </summary>
		/// <param name="value"></param>
		/// <param name="begin"></param>
		/// <param name="time"></param>
		void SmoothRadius(float value, float begin, float time);
		/// <summary>
		/// 根据直径和角度来缓动镜头高度
		/// </summary>
		/// <param name="value"></param>
		/// <param name="begin"></param>
		/// <param name="time"></param>
		void SmoothY(float value, float begin, float time);
		/// <summary>
		/// 根据直径和角度来缓动镜头绕Y轴旋转
		/// </summary>
		/// <param name="value"></param>
		/// <param name="begin"></param>
		/// <param name="time"></param>
		void SmoothAngle(float value, float begin, float time);
        /// <summary>
        /// 镜头抖动
        /// </summary>
        /// <param name="force"></param>
        /// <param name="spring"></param>
        /// <param name="attenuation"></param>
        /// <param name="existTime"></param>
        void Shake(float force, float spring, float attenuation, float existTime);


        void CacheValue(float radius, float y, float angle);

        /// <summary>
        /// 播放开场动画
        /// </summary>
        void PlayIntroAnimation();
        /// <summary>
        /// 下一帧频不smooth
        /// </summary>
        void InterruptSmooth();
        ////
        //void IsSmooth;

        /// <summary>
        /// 获取特定后效
        /// </summary>
        /// <typeparam name="T">后效类</typeparam>
        T GetPostEffect<T>() where T : class, IGOEPostEffect, new();

        /// <summary>
        /// 删除特定后效
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void RemovePostEffect<T>() where T : class, IGOEPostEffect, new();
    }
}
