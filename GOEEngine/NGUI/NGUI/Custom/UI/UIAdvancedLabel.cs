using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI UIAdvancedLabel")]
public class UIAdvancedLabel : UILabel
{
    [HideInInspector]
    [SerializeField]
    string mFontName;

    [HideInInspector]
    [SerializeField]
    string mSmilyFontName;

    [HideInInspector]
    [SerializeField]
    UIFont mSmilyFont;

    UITexture mSmilyDrawObject;

    public string FontName { get { return mFontName; } set { mFontName = value; } }
    public string SmilyFontName { get { return mSmilyFontName; } set { mSmilyFontName = value; } }

    public UIFont SmilyFont { get { return mSmilyFont; } set { mSmilyFont = value; } }

    protected override bool IsCombined
    {
        get
        {
            return mSmilyFont != null && (bitmapFont == null || bitmapFont.dynamicFont != null);
        }
    }
    public void OnLoadFont(string name, UnityEngine.Object obj)
    {
        bitmapFont = (obj as GameObject).GetComponent<UIFont>();
        MYUITools.ResetFontWhenLoadInGame(obj as GameObject);
    }

    public void OnLoadSmilyFont(string name, UnityEngine.Object obj)
    {
        mSmilyFont = (obj as GameObject).GetComponent<UIFont>();
        MYUITools.ResetFontWhenLoadInGame(obj as GameObject);
    }

    public override void UpdateNGUIText()
    {
        base.UpdateNGUIText();
        if (IsCombined)
        {
            NGUIText.bitmapFont = mSmilyFont;
            NGUIText.useSymbols = true;
            if (bitmapFont != null)
                NGUIText.dynamicFont = bitmapFont.dynamicFont;
            else
                NGUIText.dynamicFont = ambigiousFont as Font;
        }
    }

    void PrintSmily(string text)
    {
        if (mSmilyDrawObject == null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            mSmilyDrawObject = NGUITools.AddWidget<UITexture>(cachedGameObject);
            mSmilyDrawObject.name = "Smily Pass";
            mSmilyDrawObject.material = mSmilyFont.material;
            mSmilyDrawObject.fillGeometry = false;
            mSmilyDrawObject.pivot = Pivot.TopLeft;
            mSmilyDrawObject.SetAnchor(cachedTransform);
        }
        else
        {
            mSmilyDrawObject.geometry.Clear();
            mSmilyDrawObject.MarkAsChanged();
            mSmilyDrawObject.SetDirty();
        }
        UIGeometry geo = mSmilyDrawObject.geometry;
        NGUIText.Print(text, geo.verts, geo.uvs, geo.cols, true, true);
    }

    public override void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
    {
        if (!isValid) return;
        int offset = verts.size;
        Color col = color;
        col.a = finalAlpha;

        if (bitmapFont != null && bitmapFont.premultipliedAlphaShader) col = NGUITools.ApplyPMA(col);

        if (QualitySettings.activeColorSpace == ColorSpace.Linear)
        {
            col.r = Mathf.Pow(col.r, 2.2f);
            col.g = Mathf.Pow(col.g, 2.2f);
            col.b = Mathf.Pow(col.b, 2.2f);
        }

        string text = processedText;
        int start = verts.size;

        UpdateNGUIText();

        NGUIText.tint = col;

        if (IsCombined)
        {
            PrintSmily(text);
        }
        NGUIText.Print(text, verts, uvs, cols, IsCombined);
        NGUIText.bitmapFont = null;
#if DYNAMIC_FONT
        NGUIText.dynamicFont = null;
#endif
        // Center the content within the label vertically
        Vector2 pos = ApplyOffset(verts, start);

        // Effects don't work with packed fonts
        if (bitmapFont != null && bitmapFont.packedFontShader) return;

        // Apply an effect if one was requested
        if (effectStyle != Effect.None)
        {
            int end = verts.size;
            pos.x = effectDistance.x;
            pos.y = effectDistance.y;

            ApplyShadow(verts, uvs, cols, offset, end, pos.x, -pos.y);

            if (effectStyle == Effect.Outline)
            {
                offset = end;
                end = verts.size;

                ApplyShadow(verts, uvs, cols, offset, end, -pos.x, pos.y);

                offset = end;
                end = verts.size;

                ApplyShadow(verts, uvs, cols, offset, end, pos.x, pos.y);

                offset = end;
                end = verts.size;

                ApplyShadow(verts, uvs, cols, offset, end, -pos.x, -pos.y);
            }
        }

        if (onPostFill != null)
            onPostFill(this, offset, verts, uvs, cols);
    }
}
