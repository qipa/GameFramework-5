using System;
using GOEngine.Implement;
using UnityEngine;

namespace GOEngine
{
    public interface IGOEQualityMgr
    {
        /// <summary>
        /// 高配
        /// </summary>
        void HeightQuality();
        
        /// <summary>
        /// 低配
        /// </summary>
        void LowQuality();

        /// <summary>
        /// 中配
        /// </summary>
        void MidQuality();

        /// <summary>
        /// 当前质量
        /// </summary>
        QualityType CurQualityType();
        bool CanCacheObject { get; }
    }
}
