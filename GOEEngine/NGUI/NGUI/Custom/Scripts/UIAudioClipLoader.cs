using UnityEngine;

[AddComponentMenu( "Game/Audio/UIAudioClipLoader" )]
public class UIAudioClipLoader : MonoBehaviour
{
    public AudioClip AudioClip;
    public bool PlayAfterLoaded = false;
    public bool PlayLoop = false;

    [HideInInspector][SerializeField] private string mResName = string.Empty;

    public string ResName
    {
        get { return mResName; }
        set { mResName = value; }
    }
}