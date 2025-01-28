using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Vector3 _originalRotation;

    [SerializeField] private bool onlyY;

    private void Start()
    {
        _originalRotation = transform.rotation.eulerAngles;
    }
    
    private void Update()
    {
        if(!Camera.main) return;
        transform.LookAt(Camera.main.transform);

        var rotation = transform.rotation.eulerAngles;
        if (onlyY)
        {
            rotation.x = _originalRotation.x;
            rotation.z = _originalRotation.z;
            transform.rotation = Quaternion.Euler(rotation);
        }
    }
}
