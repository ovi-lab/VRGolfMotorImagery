using UnityEngine;

public class HotSwapColor : MonoBehaviour
{
    [SerializeField] private Color color;
    [SerializeField] private MeshRenderer mr;
    
    private MaterialPropertyBlock mpb;
    private static readonly int ShaderProp = Shader.PropertyToID("_Color");

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

    public void SetColor(Color color)
    {
        this.color = color;
        ApplyColor();
    }

    private void ApplyColor()
    {
        Mpb.SetColor(ShaderProp, color);
        if(mr != null){
            mr.SetPropertyBlock(Mpb);
        }
    }
}