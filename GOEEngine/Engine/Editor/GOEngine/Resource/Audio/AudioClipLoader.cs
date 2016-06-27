using UnityEngine;
using GOEngine.Implement;
using GOEngine;
using System.Collections;

[ExecuteInEditMode]
//[RequireComponent( typeof( AudioSource ) )]
[AddComponentMenu( "Audio/AudioClipLoader" )]
#if UNITY_EDITOR
    public
#else
    internal
#endif
class AudioClipLoader : MonoBehaviour
{
    [SerializeField] private string mResName = string.Empty;
    [HideInInspector][SerializeField] private AudioSource mAudioSource;

    public string ResName
    {
        get { return mResName; }
        set { mResName = value; }
    }

    public AudioSource AudioSource
    {
        get { return mAudioSource; }
        set { mAudioSource = value; }
    }

    void Awake()
    {
        if( mAudioSource == null )
            mAudioSource = this.gameObject.AddComponent<AudioSource>();
    }

    public void LoadSound()
    {
        if (mResName == string.Empty || mAudioSource == null)
            return;
        GOERoot.ResMgrImp.GetAsset(mResName, OnLoadSound, LoadPriority.PostLoad);
    }

    public void OnLoadSound(string name, UnityEngine.Object clip)
    {
        if(!mAudioSource)
        {
            GameObject.DestroyImmediate(this);
            return;
        }
        mAudioSource.clip = clip as AudioClip;
        mAudioSource.spatialBlend = 0f;
        if (mAudioSource.playOnAwake && enabled && this.gameObject.activeSelf)
            mAudioSource.Play();
        GameObject.DestroyImmediate(this);
    }
}