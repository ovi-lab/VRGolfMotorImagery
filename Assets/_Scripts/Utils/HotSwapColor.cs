using System.Collections.Generic;
using UnityEngine;

public class HotSwapColor : MonoBehaviour
{
    [SerializeField] private List<Color> colors;
    [SerializeField] private MeshRenderer mr;

    private MaterialPropertyBlock mpb;
    private static readonly int ShaderProp = Shader.PropertyToID("_BaseColor");

    private MaterialPropertyBlock Mpb => mpb ??= new MaterialPropertyBlock();

    private void OnEnable()
    {
        if (mr == null) mr = GetComponent<MeshRenderer>();
        ApplyColor();
    }

    private void OnValidate()
    {
        ApplyColor();
    }

    public void SetAlpha(float alpha, int index = 0)
    {
        colors[index] = new Color(colors[index].r, colors[index].g, colors[index].b, alpha);
        ApplyColor();
    }

    public void SetColor(Color color, int index = 0)
    {
        colors[index] = color;
        ApplyColor();
    }

    private void ApplyColor()
    {
        for (int i = 0; i < mr.sharedMaterials.Length; i++)
        {
            Color targetColor;
            try { targetColor = colors[i]; }
            catch { targetColor = new Color(0,0,0); }
            Mpb.SetColor(ShaderProp, targetColor);
            if (mr != null)
            {
                mr.SetPropertyBlock(Mpb, i);
            }
        }
    }
}