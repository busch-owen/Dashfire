using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class DeathFade : MonoBehaviour
{
    [SerializeField] private float startDelay;
    [SerializeField] private float fadeDuration;

    private CanvasGroup _canvasGroup;

    private WaitForSeconds _waitForStartDelay;
    private WaitForSeconds _waitForFade;
    private WaitForFixedUpdate _waitForUpdate;

    private void OnEnable()
    {
        _waitForStartDelay ??= new WaitForSeconds(startDelay);
        _waitForFade ??= new WaitForSeconds(fadeDuration);
        _waitForUpdate ??= new WaitForFixedUpdate();
        _canvasGroup = GetComponent<CanvasGroup>();

        StartCoroutine(FadeTransition());
    }

    private IEnumerator FadeTransition()
    {
        yield return _waitForStartDelay;
        _canvasGroup.DOFade(1, fadeDuration);
        yield return _waitForFade;
        _canvasGroup.DOFade(0, fadeDuration);
        yield return _waitForFade;
        gameObject.SetActive(false);
    }
}
