using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI UIMyLabel")]
public class UIMyLabel: UILabel
{
    [HideInInspector]
    [SerializeField]
    string mFontName;
    public string FontName { get { return mFontName; } set { mFontName = value; } }

    public void OnLoadFont(string name, UnityEngine.Object obj )
    {
        bitmapFont = (obj as GameObject).GetComponent<UIFont>();
        MYUITools.ResetFontWhenLoadInGame(obj as GameObject);
    }
}
