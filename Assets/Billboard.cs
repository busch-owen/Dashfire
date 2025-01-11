using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera _currentCam;

    private void OnEnable()
    {
        _currentCam = Camera.main;
    }

    private void Update()
    {
        if (_currentCam)
        {
            transform.LookAt(_currentCam.transform);
        }
    }
}
