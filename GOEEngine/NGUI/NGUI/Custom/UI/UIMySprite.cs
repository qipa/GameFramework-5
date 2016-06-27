using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI UIMySprite")]
public class UIMySprite: UISprite
{
    [HideInInspector]
    [SerializeField]
    private string mmAtlasName;

    private bool mNeedSetGray = false;
    private bool mIsGray = false;
    private UIAtlas mNormalAtlas;
    string needSetMaskName;

    public string AtlasName
    {
        set { mmAtlasName = value; }
        get { return mmAtlasName; }
    }

    public bool ReleaseOnLevelLoaded { get; set; }

    public void OnLoadAtlas(string name, UnityEngine.Object obj )
    {
        atlas = (obj as GameObject).GetComponent<UIAtlas>();
        mNormalAtlas = this.atlas;                    
        if (mNeedSetGray)
            SetGray = true;
        if (needSetMaskName != null)
        {
            SetMask(needSetMaskName);
        }
        UISpriteAnimation ani = GetComponent<UISpriteAnimation>();
        if (ani != null)
        {
            ani.RebuildSpriteList();
        }
    }

    public void SetAtlas(string name,string spriteName)
    {
        this.spriteName = spriteName; 
        MYUITools.GetAssetFunc(name, OnLoadAtlas, 1);
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
                    if (mNormalAtlas == null)
                        mNormalAtlas = this.atlas;
                    mIsGray = true;
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

    public void SetMask(string mask)
    {
        needSetMaskName = mask;
        if (this.atlas != null)
        {
            if (!string.IsNullOrEmpty(mask))
            {
                if (mNormalAtlas == null)
                {
                    mNormalAtlas = this.atlas;
                }
                this.atlas = MYUITools.GetMaskAtlas(mNormalAtlas, mask);
            }
            else
            {
                this.atlas = mNormalAtlas;
            }
        }
    }
}
