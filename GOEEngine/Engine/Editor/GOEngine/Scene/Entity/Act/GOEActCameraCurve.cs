using System;
using UnityEngine;

namespace GOEngine.Implement
{
    [ActTypeName(ActType.CameraCurve)]
    [System.Reflection.Obfuscation(Exclude = true)]
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class GOEActCameraCurve : GOEActComponent
    {

        public string mCurveName = "";

        public string CurveName
        {
            get { return mCurveName; }
            set { mCurveName = value; }
        }

        private GOECameraCurve _camCurve;
        internal override void OnTrigger()
		{
			base.OnTrigger ();
			
            _camCurve = GOECameraCurve.AddCameraCurve();
            if(_camCurve!= null)
            {
                _camCurve.Init(mCurveName, LocalTime - TriggerTime);
            }
            Enable = false;
		}

        internal override void OnDestroy()
		{
			base.OnDestroy ();

            if (_camCurve != null)
            {
                _camCurve.Stop();
            }
		}

        internal override void GetResAsset(System.Collections.Generic.HashSet<string> setRes)
        {
            
        }

        internal override void PreLoad()
        {
            
        }

	}
}


