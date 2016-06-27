using System;
using System.Collections.Generic;
using UnityEngine;

namespace GOEngine
{
    public enum QualityType
    {
        Low,
        Mid,
        Height,
    }
}

namespace GOEngine.Implement
{

#if UNITY_EDITOR
	public
#else
    internal
#endif
 class GOEQualityMgr : IGOEQualityMgr
    {
        private QualityType mQuality = QualityType.Height;
        /// <summary>
        /// 高配
        /// </summary>
        public void HeightQuality()
        {
            mQuality = QualityType.Height;
            LightFaceEffect lfe = GameObject.FindObjectOfType<LightFaceEffect>() as LightFaceEffect;
            if (lfe != null)
            {
                try
                {
                    lfe.LightEffectEnable = true;
                    lfe.GlowEnable = true;
                    lfe.TerrainXQulity = TerrainXQualityLevel.High;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.ToString());
                }
            }
        }


        /// <summary>
        /// 中配
        /// </summary>
        public void MidQuality()
        {
            mQuality = QualityType.Mid;
            LightFaceEffect lfe = GameObject.FindObjectOfType<LightFaceEffect>() as LightFaceEffect;
            if (lfe != null)
            {
                lfe.LightEffectEnable = true;
                lfe.GlowEnable = false;
                lfe.TerrainXQulity = TerrainXQualityLevel.Mid;
            }
        }


        /// <summary>
        /// 低配
        /// </summary>
        public void LowQuality()
        {
            mQuality = QualityType.Low;
            LightFaceEffect lfe = GameObject.FindObjectOfType<LightFaceEffect>() as LightFaceEffect;
            if (lfe != null)
            {
                lfe.LightEffectEnable = false;
                lfe.GlowEnable = false;
                lfe.TerrainXQulity = TerrainXQualityLevel.Low;
            }
        }

        public QualityType CurQualityType()
        {
            return mQuality;
        }

        private bool hasChacked = false;
        private bool canCache = false;
        public bool CanCacheObject
        {
            get
            {
                if (!hasChacked)
                {
                    canCache = UnityEngine.SystemInfo.systemMemorySize > 600;
                }
                return canCache;
            }
        }
    }
 
}
