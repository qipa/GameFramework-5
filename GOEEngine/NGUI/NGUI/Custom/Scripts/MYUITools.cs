using UnityEngine;
using System.Collections.Generic;
using System;
public class MYUITools
{
    const string PrefabExt = ".prefab";
    const string OggExt = ".ogg";
    static Dictionary<string, UIAtlas> mGrayAtlasDic = new Dictionary<string, UIAtlas>();
    static Dictionary<string, Texture2D> maskTexDic = new Dictionary<string, Texture2D>();
    static public UIAtlas GetGrayAtlas(UIAtlas atlas)
    {
        UIAtlas mGrayAtlas;
        mGrayAtlasDic.TryGetValue(atlas.name, out mGrayAtlas);
        if (mGrayAtlas == null)
        {
            mGrayAtlas = UnityEngine.Object.Instantiate(atlas) as UIAtlas;
            mGrayAtlas.name = atlas.name + "(Gray)";

            UnityEngine.Object.DontDestroyOnLoad(mGrayAtlas);

            string sn = atlas.spriteMaterial.shader.name;
            Shader sh = Shader.Find(sn + "(Gray)");
            Material mat = new Material(sh);
            mat.mainTexture = atlas.spriteMaterial.mainTexture;

            mGrayAtlas.spriteMaterial = mat;

            mGrayAtlasDic[atlas.name] = mGrayAtlas;
        }

        return mGrayAtlas;
    }

    public static UIAtlas GetMaskAtlas(UIAtlas atlas, string mask)
    {
        UIAtlas mMaskAtlas;
        string atlasName = atlas.name + "_Mask_" + mask;
        if (!mGrayAtlasDic.TryGetValue(atlasName, out mMaskAtlas))
        {
            mMaskAtlas = UnityEngine.Object.Instantiate(atlas) as UIAtlas;
            mMaskAtlas.name = atlasName;

            UnityEngine.Object.DontDestroyOnLoad(mMaskAtlas);

            string sn = atlas.spriteMaterial.shader.name;
            Shader sh = Shader.Find(sn + "(Mask)");
            Material mat = new Material(sh);
            mat.mainTexture = atlas.spriteMaterial.mainTexture;

            Texture2D maskTex;
            if (!maskTexDic.TryGetValue(mask, out maskTex))
            {
                maskTex = Resources.Load<Texture2D>(mask);
                UnityEngine.Object.DontDestroyOnLoad(maskTex);
                maskTexDic[mask] = maskTex;
            }
            mat.SetTexture("_MaskTex", maskTex);

            mMaskAtlas.spriteMaterial = mat;

            mGrayAtlasDic[atlasName] = mMaskAtlas;
        }

        return mMaskAtlas;
    }

    static public void ClearAtlasRes(Component com)
    {
        UIAtlas atlas = com as UIAtlas;
        if (atlas != null)
        {
            Resources.UnloadAsset(atlas.spriteMaterial.mainTexture);
            Resources.UnloadAsset(atlas.spriteMaterial);
        }
        else
        {
            return;
        }
        UIAtlas mGrayAtlas;
        mGrayAtlasDic.TryGetValue(atlas.name, out mGrayAtlas);
        if ((mGrayAtlas != null) && (mGrayAtlas.spriteMaterial != null))
        {
            mGrayAtlasDic.Remove(atlas.name);
            Texture2D.Destroy(mGrayAtlas.spriteMaterial.mainTexture);
            Material.Destroy(mGrayAtlas.spriteMaterial);

            UnityEngine.Object.Destroy(mGrayAtlas.gameObject);
            mGrayAtlas = null;
        }
    }

    static string GetFontName(object font)
    {
        if (font != null)
        {
            if (font is UIFont)
                return ((UIFont)font).name + PrefabExt;
            if (font is Font)
                return ((Font)font).name + PrefabExt;
        }
        return null;
    }

    public static void ResetFontWhenPack(GameObject obj, Action<string> addNameAction)
    {
        PrefabConfig[] pcs = obj.GetComponentsInChildren<PrefabConfig>(true);
        foreach (PrefabConfig pc in pcs)
        {
            GameObject.DestroyImmediate(pc);
        }
        ScriptConfig[] scs = obj.GetComponentsInChildren<ScriptConfig>(true);
        foreach (ScriptConfig sc in scs)
        {
            GameObject.DestroyImmediate(sc);
        }

        /*UIFont[] fonts = obj.GetComponentsInChildren<UIFont>(true);
        foreach (UIFont i in fonts)
        {
            if (!i.isDynamic)
            {
                if (i.atlas != null)
                {
                    UIFontAtlasLoader loader = i.gameObject.AddComponent<UIFontAtlasLoader>();
                    loader.FontAtlas = i.atlas.name + PrefabExt;
                    loader.Font = i;
                    loader.Font.atlas = null;
                    addNameAction(loader.FontAtlas);
                }
                i.atlas = null;
            }
        }*/
    }

    public static void ResetFontWhenLoadInGame(GameObject mGameObject)
    {
        UIFontAtlasLoader[] loaders = mGameObject.GetComponentsInChildren<UIFontAtlasLoader>(true);
        foreach (UIFontAtlasLoader i in loaders)
        {
            if (!string.IsNullOrEmpty(i.FontAtlas) && i.Font != null && i.Font.atlas == null)
            {
                GetAssetFunc(i.FontAtlas, i.OnLoadAtlas, 0);
            }
        }
    }
    static public void ResetUIWhenPack(GameObject obj, Action<string> addNameAction)
    {
        PrefabConfig[] pcs = obj.GetComponentsInChildren<PrefabConfig>(true);
        foreach (PrefabConfig pc in pcs)
        {
            GameObject.DestroyImmediate(pc);
        }
        ScriptConfig[] scs = obj.GetComponentsInChildren<ScriptConfig>(true);
        foreach (ScriptConfig sc in scs)
        {
            GameObject.DestroyImmediate(sc);
        }

        BoxCollider[] boxs = obj.GetComponentsInChildren<BoxCollider>(true);
        foreach(BoxCollider box in boxs)
        {
            UIEventListener.Get(box.gameObject);
        }


        UIAudioClipLoader[] clips = obj.GetComponentsInChildren<UIAudioClipLoader>(true);
        foreach (UIAudioClipLoader elem in clips)
        {
            // Now we assume it's all .ogg format.
            elem.ResName = (elem.AudioClip != null) ? elem.AudioClip.name + OggExt : string.Empty;
            if (elem.AudioClip != null)
            {
                addNameAction(elem.AudioClip.name + OggExt);
            }
            elem.AudioClip = null;
        }

        UITexture[] textures = obj.GetComponentsInChildren<UITexture>(true);
        foreach (UITexture text in textures)
        {
            if (!(text is UIMyTexture))
            {
                if (text.mainTexture != null)
                {
                    text.mainTexture = null;
                    Debug.Log(obj.name + "has invalid texture " + text);
                }

                continue;
            }
            UIMyTexture texture = text as UIMyTexture;
            if (texture.mainTexture != null)
            {
                texture.mTextureName = texture.mainTexture.name + PrefabExt;
                addNameAction(texture.mainTexture.name + PrefabExt);
                texture.mainTexture = null;
            }
        }

        // delete camera and uicamera
        UICamera uiCamera = obj.GetComponentInChildren<UICamera>();
        if (uiCamera != null)
        {
            MonoBehaviour.DestroyImmediate(uiCamera);
        }

        Camera camera = obj.GetComponentInChildren<Camera>();
        if (camera != null)
        {
            MonoBehaviour.DestroyImmediate(camera);
        }

        AudioListener lis = obj.GetComponentInChildren<AudioListener>();
        if (lis != null)
        {
            MonoBehaviour.DestroyImmediate(lis);
        }

        obj.SetActive(false);
    }

    public static Action<string, Action<string, UnityEngine.Object>, int> GetAssetFunc;
    public static void ResetUiWhenLoadInGame(GameObject mGameObject)
    {
        UIMyTexture[] textures = mGameObject.GetComponentsInChildren<UIMyTexture>(true);
        foreach (UIMyTexture texture in textures)
        {
            if (texture.TextureName != string.Empty && texture.TextureName != null)
                GetAssetFunc(texture.TextureName, texture.OnLoadTexture, 0);
        }
    }
}
