using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class CameraShake : MonoBehaviour
{
    private CameraController _controller;

    private void OnEnable()
    {
        _controller ??= GetComponentInParent<CameraController>();
    }

    public void Shake(float magnitude, float duration)
    {
        CancelInvoke(nameof(ResetShake));
        //transform.DOShakePosition(duration, magnitude);
        transform.DOShakeRotation(duration, magnitude);
        Invoke(nameof(ResetShake), duration);
    }

    private void ResetShake()
    {
        //_controller.ResetCameraTransform();
    }
}
