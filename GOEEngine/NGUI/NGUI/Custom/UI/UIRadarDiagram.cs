using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI UIRadarDiagram")]
public class UIRadarDiagram : UIMySprite
{
    [System.NonSerialized]
    [HideInInspector]
    //float[] values = new float[8] { 1, 1, 1, 1, 1, 1, 1, 1 };
    //float[] values = new float[7] { 1, 1, 1, 1, 1, 1, 1 };
    //float[] values = new float[6] { 1, 1, 1, 1, 1, 1 };
    float[] values = new float[5] { 1, 1, 1, 1, 1 };
    //float[] values = new float[4] { 1f, 1f, 1f, 1f };
    //float[] values = new float[3] { 1f, 1f, 1f };

    public float[] Values
    {
        get { return values; }
        set
        {
            values = value;
            MarkAsChanged();
        }
    }
    public override void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
    {
        //if (!isValid) return;

        Texture tex = mainTexture;
        if (tex == null) return;

        if (mSprite == null) mSprite = atlas.GetSprite(spriteName);
        if (mSprite == null) return;

        Rect outer = new Rect(mSprite.x, mSprite.y, mSprite.width, mSprite.height);
        Rect inner = new Rect(mSprite.x + mSprite.borderLeft, mSprite.y + mSprite.borderTop,
            mSprite.width - mSprite.borderLeft - mSprite.borderRight,
            mSprite.height - mSprite.borderBottom - mSprite.borderTop);

        outer = NGUIMath.ConvertToTexCoords(outer, tex.width, tex.height);
        inner = NGUIMath.ConvertToTexCoords(inner, tex.width, tex.height);
        Vector4 u;
        switch (mFlip)
        {
            case Flip.Horizontally:
                u = new Vector4(outer.xMax, outer.yMin, outer.xMin, outer.yMax);
                break;
            case Flip.Vertically:
                u = new Vector4(outer.xMin, outer.yMax, outer.xMax, outer.yMin);
                break;
            case Flip.Both:
                u = new Vector4(outer.xMax, outer.yMax, outer.xMin, outer.yMin);
                break;
            default:
                u = new Vector4(outer.xMin, outer.yMin, outer.xMax, outer.yMax);
                break;
        }
        Vector4 v = drawingDimensions;
        float width = this.width / 2;

        Vector2 center = new Vector2(0, 0);
        Color32 c = drawingColor;
        Vector2 tmp = new Vector2(0, width);
        int cnt = values.Length % 2 == 0 ? values.Length : values.Length + 1;
        for (int i = 0; i < cnt; i++)
        {
            if (((i - 1) % 2 == 0 && i > 1) || i == 0)
            {
                verts.Add(new Vector3(center.x, center.y));
                uvs.Add(new Vector2(u.x, u.y));
                cols.Add(c);

                if (i > 0)
                {
                    float val = values[(i - 1) % values.Length];
                    Vector2 uv = new Vector2(u.x + (u.z - u.x) * val, u.y + (u.w - u.y) * val);
                    float rad2 = ((float)(i - 1) / values.Length) * 2 * Mathf.PI;
                    Vector2 offset2 = RotateVec2(ref tmp, rad2) * val;

                    verts.Add(new Vector3(center.x + offset2.x, center.y + offset2.y));
                    uvs.Add(uv);
                    cols.Add(c);
                }
            }
            {
                float val = values[i % values.Length];
                Vector2 uv = new Vector2(u.x + (u.z - u.x) * val, u.y + (u.w - u.y) * val);
                float rad = ((float)i / values.Length) * 2 * Mathf.PI;
                Vector2 offset = RotateVec2(ref tmp, rad) * val;

                verts.Add(new Vector3(center.x + offset.x, center.y + offset.y));
                uvs.Add(uv);
                cols.Add(c);

                if (i + 1 >= cnt)
                {
                    verts.Add(new Vector3(center.x, center.y + tmp.y));
                    uvs.Add(uv);
                    cols.Add(c);
                }
            }
        }
        if (onPostFill != null)
            onPostFill(this, 0, verts, uvs, cols);
    }
    Color32 drawingColor
    {
        get
        {
            Color colF = color;
            colF.a = finalAlpha;
            if (premultipliedAlpha) colF = NGUITools.ApplyPMA(colF);

            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
            {
                colF.r = Mathf.Pow(colF.r, 2.2f);
                colF.g = Mathf.Pow(colF.g, 2.2f);
                colF.b = Mathf.Pow(colF.b, 2.2f);
            }
            return colF;
        }
    }
    Vector2 RotateVec2(ref Vector2 ori, float rad)
    {
        float a, b;
        float c, d;
        a = Mathf.Cos(rad);
        b = -Mathf.Sin(rad);
        c = -b;
        d = a;

        return new Vector2(ori.x * a + ori.y * c, ori.x * b + ori.y * d);
    }
}
