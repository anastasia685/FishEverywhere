using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fader : MonoBehaviour
{
    Renderer[] renderers;
    MaterialPropertyBlock propertyBlock;

    float currentAlpha = 1f;
    float targetAlpha = 1f;
    [SerializeField] float fadeSpeed = 5f;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    void Update()
    {
        if (Mathf.Approximately(currentAlpha, targetAlpha)) return;

        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

        foreach (var renderer in renderers)
        {
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat("_Fade", currentAlpha);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }

    public void SetAlpha(float alpha)
    {
        targetAlpha = Mathf.Clamp01(alpha);
    }
}
