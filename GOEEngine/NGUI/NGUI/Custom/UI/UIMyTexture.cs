using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class UIMyTexture : UITexture
{
    [HideInInspector]
    [SerializeField]
    public string mTextureName;
    public string TextureName
    {
        get { return mTextureName; }
        set
        {
            mTextureName = value;
            ReloadTexture();
        }
    }

    public void OnLoadTexture(string name, UnityEngine.Object obj)
    {            
        mainTexture = (obj as GameObject).GetComponent<UITexture>().mainTexture;
    }

    void ReloadTexture()
    {
        if (!string.IsNullOrEmpty(TextureName))
        {
            MYUITools.GetAssetFunc(TextureName, OnLoadTexture, 0);
        }
    }
}