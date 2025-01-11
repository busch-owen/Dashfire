using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera _currentCam;

    private void OnEnable()
    {
        _currentCam = FindFirstObjectByType<Camera>();
    }

    private void Update()
    {
        if (_currentCam)
        {
            transform.LookAt(_currentCam.transform);
        }
    }
}
