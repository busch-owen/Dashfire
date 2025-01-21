using System.Collections;
using UnityEngine;
using DG.Tweening;

public class CameraShake : MonoBehaviour
{
    public void Shake(float magnitude, float duration)
    {
        transform.DOShakePosition(duration, magnitude);
        transform.DOShakeRotation(duration, magnitude);
    }
}
