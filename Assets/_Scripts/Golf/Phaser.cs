using System.Collections;
using UnityEngine;

public class Phaser : MonoBehaviour
{
    [SerializeField] private EffectType effectType;
    [SerializeField] private float animTime;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Transform sphereTransform;
    [SerializeField] private HotSwapColor hotSwapColor;

    private Coroutine phaseInCoroutine;
    private Coroutine phaseOutCoroutine;

    public float AnimTime => animTime;


    private void Start()
    {
        meshRenderer.enabled = false;
    }

    public void PhaseIn()
    {
        switch (effectType)
        {
            case EffectType.Appear:
                phaseInCoroutine = StartCoroutine(Appear(true));
                break;
            case EffectType.Fade:
                meshRenderer.enabled = true;
                hotSwapColor.SetAlpha(0f);
                phaseInCoroutine = StartCoroutine(Fade(0, 1));
                break;
            case EffectType.Zoom:
                sphereTransform.localScale = Vector3.zero;
                meshRenderer.enabled = true;
                phaseInCoroutine = StartCoroutine(Zoom(Vector3.zero, Vector3.one));
                break;
        }
    }

    public void Disappear()
    {
        if(phaseInCoroutine!=null) StopCoroutine(phaseInCoroutine);
        if(phaseOutCoroutine!=null) StopCoroutine(phaseOutCoroutine);
        StopAllCoroutines();
        meshRenderer.enabled = false;
    }

    public void PhaseOut()
    {
        switch (effectType)
        {
            case EffectType.Appear:
                phaseOutCoroutine = StartCoroutine(Appear(false));
                break;
            case EffectType.Fade:
                phaseOutCoroutine = StartCoroutine(Fade(1, 0, () => meshRenderer.enabled = false));
                break;
            case EffectType.Zoom:
                phaseOutCoroutine = StartCoroutine(Zoom(Vector3.one, Vector3.zero, () => meshRenderer.enabled = false));
                break;
        }
    }

    private IEnumerator Appear(bool state)
    {
        if (state) meshRenderer.enabled = true;
        yield return new WaitForSeconds(animTime);
        if (!state) meshRenderer.enabled = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, System.Action onComplete = null)
    {
        float elapsedTime = 0;
        while (elapsedTime < animTime/2)
        {
            if(startAlpha < 1) hotSwapColor.SetAlpha(Mathf.Lerp(startAlpha, endAlpha, 2*elapsedTime / animTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        elapsedTime = 0;
        while (elapsedTime < animTime/2)
        {
            if(startAlpha > 0) hotSwapColor.SetAlpha(Mathf.Lerp(startAlpha, endAlpha, 2*elapsedTime / animTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        hotSwapColor.SetAlpha(endAlpha);
        onComplete?.Invoke();
    }

    private IEnumerator Zoom(Vector3 startScale, Vector3 endScale, System.Action onComplete = null)
    {
        float elapsedTime = 0;
        while (elapsedTime < animTime/2)
        {
            if(startScale.x < 1) sphereTransform.localScale = Vector3.Lerp(startScale, endScale, 2*elapsedTime / animTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        elapsedTime = 0;
        while (elapsedTime < animTime/2)
        {
            if(startScale.x > 0)sphereTransform.localScale = Vector3.Lerp(startScale, endScale, 2*elapsedTime / animTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        sphereTransform.localScale = endScale;
        onComplete?.Invoke();
    }
}

public enum EffectType
{
    Appear = 0,
    Fade = 1,
    Zoom = 2
}
