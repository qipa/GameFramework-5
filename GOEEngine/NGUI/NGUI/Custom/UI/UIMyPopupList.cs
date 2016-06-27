using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI UIMyPopupList")]
public class UIMyPopupList: UIPopupList
{
    [HideInInspector]
    [SerializeField]
    string mFontName;
    [HideInInspector]
    [SerializeField]
    string mmAtlasName;

    private bool mNeedSetGray = false;
    private bool mIsGray = false;
    private UIAtlas mNormalAtlas;
    public string FontName { get { return mFontName; } set { mFontName = value; } }
    public string AtlasName
    {
        set { mmAtlasName = value; }
        get { return mmAtlasName; }
    }

    public bool SetGray
    {
        set
        {
            if (this.atlas == null)
            {
                mNeedSetGray = value;
            }
            else
            {
                if (value && !mIsGray)
                {
                    mIsGray = true;

                    mNormalAtlas = this.atlas;
                    this.atlas = MYUITools.GetGrayAtlas(mNormalAtlas);
                }
                else if (!value && mIsGray)
                {
                    this.atlas = mNormalAtlas;

                    mIsGray = false;
                }
            }
        }
    }
    public void OnLoadFont(string name, UnityEngine.Object obj )
    {
        bitmapFont = (obj as GameObject).GetComponent<UIFont>();
        MYUITools.ResetFontWhenLoadInGame(obj as GameObject);
    }
    public void OnLoadAtlas(string name, UnityEngine.Object obj)
    {
        atlas = (obj as GameObject).GetComponent<UIAtlas>();
    }
}
