using System;
using UnityEngine;
namespace GOEngine
{
    /// <summary>
    /// 全屏后效通用接口
    /// </summary>
    public interface IGOEPostEffect : IDisposable
    {
        /// <summary>
        /// 是否激活
        /// </summary>
        bool Enabled { get; set; }
        /// <summary>
        /// 用于此后效的材质
        /// </summary>
        Material EffectMaterial { get; }
        /// <summary>
        /// 初始化后效
        /// </summary>
        /// <param name="lfe">摄像机上的LightfaceEffect</param>
        void Initialize(LightFaceEffect lfe);

        /// <summary>
        /// 设置材质参数
        /// </summary>
        void UpdateMaterialParameter();

        /// <summary>
        /// 更新后效
        /// </summary>
        /// <param name="lfe">摄像机上的LightfaceEffect</param>
        void Update(LightFaceEffect lfe);
    }
}
