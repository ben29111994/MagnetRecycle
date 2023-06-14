using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveAnimation : MonoBehaviour
{
    public Color _currentColor;
    public Color targetColor;
    public Renderer meshRenderer;
    public Animator waveAnimator;
    public bool isColorWave;

    private IEnumerator OutColor_End;
    private bool isColorWaveAnimation;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        waveAnimator = transform.parent.GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            RunAnimation();
        }
    }

    public void RunAnimation()
    {
        waveAnimator.SetTrigger("Wave");

        if (isColorWaveAnimation == false)
        {
            StartCoroutine(C_ColorWaveAnimation());
        }
    }

    private IEnumerator C_ColorWaveAnimation()
    {
        // animation wave = 1.0s => color animation = 1s .
        isColorWaveAnimation = true;

        if (OutColor_End != null)
        {
            StopCoroutine(OutColor_End);
        }

        if (isColorWave)
        {
            // 1s = 0.5s (color fade out) + 0.5f (color fade in)
            yield return StartCoroutine(C_ColorOut(0.5f, 0.0f));
            yield return StartCoroutine(C_ColorIn(0.5f));
        }
        else
        {
            isColorWave = true;
            yield return StartCoroutine(C_ColorIn(1.0f));
        }

        // after end , check fade color wave to current color .

        OutColor_End = C_ColorOut(1.0f, 0.5f);
        StartCoroutine(OutColor_End);

        isColorWaveAnimation = false;
    }

    private IEnumerator C_ColorIn(float time)
    {
        Color _waveColor = targetColor;

        float t = 0.0f;

        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();

        while (t < 1.0f)
        {
            t += Time.deltaTime * (1.0f / time);

            Color _color = Color.Lerp(_currentColor, _waveColor, t);

            meshRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", _color);
            meshRenderer.SetPropertyBlock(propBlock);

            yield return null;
        }
    }

    private IEnumerator C_ColorOut(float time, float timeDelay)
    {
        yield return new WaitForSeconds(timeDelay);

        Color _waveColor = targetColor;

        float t = 0.0f;

        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();

        while (t < 1.0f)
        {
            t += Time.deltaTime * (1.0f / time);

            Color _color = Color.Lerp(_waveColor, _currentColor, t);

            meshRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", _color);
            meshRenderer.SetPropertyBlock(propBlock);

            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pivot"))
        {
            RunAnimation();
        }
    }
}
