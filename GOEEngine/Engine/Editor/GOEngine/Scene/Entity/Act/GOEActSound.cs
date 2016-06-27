using UnityEngine;
using System;
using System.Collections.Generic;

namespace GOEngine.Implement
{
    [ActTypeName(ActType.ActorSound)]
    [System.Reflection.Obfuscation(Exclude = true)]
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class GOEActSound : GOEActComponent
    {
        
       //名字//
        private string mSoundName;
        private string[] mSoundNameList = null;
        private static char[] mcSplit= new char[]{','};
        
        //触发概率//
        private float mPlayProbability = 1f;
        
        //音量//
        private float mVolFix = 1f;
        private float mVolRand = 0f;
        //音高//
        private float mPitchFix = 1f;
        private float mPitchRand = 0f;

        private bool mLoop = false;
        
        private AudioSource mAudioSource = null;
        //
        public string SoundName
        {
            get { return mSoundName;}
            set {
                mSoundName = value;
                mSoundNameList = mSoundName.Split(mcSplit);
            }
        }
        public float PlayProbability
        {
            get { return mPlayProbability; }
            set { mPlayProbability = value; }
        }

        public float VolFix
        {
            get { return mVolFix; }
            set { mVolFix = value; }
        }

        public float VolRand
        {
            get { return mVolRand; }
            set { mVolRand = value; }
        }

        public float PitchFix
        {
            get { return mPitchFix; }
            set { mPitchFix = value; }

        }
        public float PitchRand
        {
            get { return mPitchRand; }
            set { mPitchRand = value; }

        }
        public bool SoundLoop
        {
            get { return mLoop; }
            set { mLoop = value; }
        }

        internal override void OnTrigger()
        {
            base.OnTrigger ();

            float f = UnityEngine.Random.Range(0f, 1f);
            if (0 != f && f <= mPlayProbability)
            {
                if( mAudioSource != null )
                {
                    GOERoot.GOEAudioMgr.RemoveSound(mAudioSource);
                }
                int idx = UnityEngine.Random.Range(0, mSoundNameList.Length);
                mAudioSource = GOERoot.GOEAudioMgr.AddSound(mSoundNameList[idx], this.Entity.GameObject);
                if (mAudioSource == null)
                {
                    OnDestroy();
                    return;
                }
                mAudioSource.loop = mLoop;
                mAudioSource.volume = mVolFix + UnityEngine.Random.Range(0, mVolRand);
                mAudioSource.pitch = mPitchFix + UnityEngine.Random.Range(0, mPitchRand);
            }
            this.Enable = false;
        }

        internal override void GetResAsset(HashSet<string> setRes)
        {
            if ( mSoundName != string.Empty)
            {
                setRes.Add( mSoundName );
            }
        }

        internal override void OnDestroy()
        {
            base.OnDestroy();

            if ( !mLoop && mAudioSource != null )
            {
                GOERoot.GOEAudioMgr.RemoveSound(mAudioSource);
            }
        }

        internal override void PreLoad()
        {
            (GOERoot.ResMgrImp as ProjectResource).GetAsset(mSoundName, null);
        }
    }
}

