using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace GOEngine.Implement
{
    [ActTypeName(ActType.ChangeWeather)]
    [DisplayName("天气控制")]
    [System.Reflection.Obfuscation(Exclude = true)]
#if UNITY_EDITOR
    public
#else
    internal
#endif
 class GOEActChangeWeather : GOEActComponent
    {
        [DisplayName("变化时间")]
        public float ChangeTime
        { get; set; }
        
        [DisplayName("最大持续时间")]
        public float MaxTime
        { get; set; }

        [DisplayName("是否下雨")]
        public bool EnableRain { get; set; }

        [DisplayName("是否闪电")]
        public bool EnableLighting { get; set; }

        [DisplayName("光线强度")]
        public float ChangeLightBrightness { get; set; }

        float time, oriLight;
        bool oriRain, oriLightning;
        bool lightningChanged, rainChanged;

        internal override void Start()
        {
            base.Start();
            time = 0;
            rainChanged = false;
            lightningChanged = false;
            
        }
        internal override void OnTrigger()
        {
            base.OnTrigger();
            rainChanged = false;
            lightningChanged = false;
            time = 0;
            oriLight = GOERoot.Scene.LightBrightness;
            oriRain = GOERoot.Scene.RainEnabled;
            oriLightning = GOERoot.Scene.FlashEnabled;
        }

        internal override void Update()
        {
            base.Update();
            time += Time.deltaTime;
            if (time >= MaxTime && MaxTime > 0)
            {
                LifeOver = true;
            }
            else
            {
                float progress = ChangeTime > 0 ? time / ChangeTime : 1;
                if (oriLight != ChangeLightBrightness)
                    Shader.SetGlobalFloat("light", Mathf.Lerp(oriLight, ChangeLightBrightness, Mathf.Min(progress, 1)));
                if (oriLightning != EnableLighting && progress > 1 && !lightningChanged)
                {
                    lightningChanged = true;
                    GOERoot.Scene.FlashEnabled = EnableLighting;                    
                }
                if (oriRain != EnableRain && progress > 1 && !rainChanged)
                {
                    rainChanged = true;
                    GOERoot.Scene.RainEnabled = EnableRain;
                }
            }
        }

        internal override void OnDestroy()
        {
            base.OnDestroy();

            if (oriLight != ChangeLightBrightness)
                Shader.SetGlobalFloat("light", oriLight);
            if (lightningChanged)
                GOERoot.Scene.FlashEnabled = oriLightning;
            if (rainChanged)
                GOERoot.Scene.RainEnabled = oriRain;
        }

        internal override void PreLoad()
        {
            
        }

        internal override void GetResAsset(HashSet<string> setRes)
        {
            
        }
    }
}
