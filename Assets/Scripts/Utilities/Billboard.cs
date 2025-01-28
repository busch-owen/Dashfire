using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera _currentCam;

    private Vector3 _originalRotation;

    [SerializeField] private bool onlyY;

    private void Start()
    {
        _originalRotation = transform.rotation.eulerAngles;
    }
    
    private void Update()
    {
        if (!_currentCam) return;
        transform.LookAt(Camera.current.transform);

        var rotation = transform.rotation.eulerAngles;
        if (onlyY)
        {
            rotation.x = _originalRotation.x;
            rotation.z = _originalRotation.z;
            transform.rotation = Quaternion.Euler(rotation);
        }
    }
}
