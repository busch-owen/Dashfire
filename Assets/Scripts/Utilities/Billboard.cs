using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera _currentCam;

    [SerializeField] private bool onlyY;

    private void Start()
    {
        _currentCam = Camera.main;
    }

    private void OnEnable()
    {
        _currentCam = Camera.main;
    }

    private void Update()
    {
        if (_currentCam)
        {
            transform.LookAt(_currentCam.transform);
            var newRot = transform.rotation.y;
            if (onlyY)
            {
                transform.rotation = Quaternion.Euler(0, newRot, 0);
            }
        }
    }
}
