using UnityEngine;
using System.Collections.Generic;

public class UIFontAtlasLoader : MonoBehaviour
{
    [SerializeField]
    public string mFontAtlas;

    [SerializeField]
    public UIFont font;
    public string FontAtlas { get { return mFontAtlas; } set { mFontAtlas = value; } }

    public UIFont Font { get { return font; } set { font = value; } }

    public void OnLoadAtlas(string name, UnityEngine.Object obj)
    {
        font.atlas = (obj as GameObject).GetComponent<UIAtlas>();
        font = null;
        enabled = false;
    }
}
