using System;
using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

public class KillBanner : MonoBehaviour
{
    private Vector3 _startScale;

    [SerializeField] private float bannerAppearTime;
    [SerializeField] private float bannerMaintainTime;

    private TMP_Text _bannerText;

    private WaitForSeconds _waitForMaintain;

    private void Start()
    {
        _startScale = transform.localScale;
        transform.localScale = Vector3.zero;
        _bannerText = GetComponentInChildren<TMP_Text>();
        _waitForMaintain = new WaitForSeconds(bannerMaintainTime);
        StartCoroutine(DisplayKillBanner("test name"));
    }


    public IEnumerator DisplayKillBanner(string playerName)
    {
        _bannerText.text = $"You fragged: {playerName}!";
        transform.DOScale(_startScale, bannerAppearTime);
        yield return _waitForMaintain;
        transform.DOScale(Vector3.zero, bannerAppearTime);
    }
}
