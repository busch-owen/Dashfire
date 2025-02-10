using UnityEngine;

public class MenuCharacter : MonoBehaviour
{
    [SerializeField] private GameObject menuCharacterObject;
    [SerializeField] private SpriteRenderer fireObject;

    [SerializeField] private GameObject[] ignore;
    
    private void Start()
    {
        var meshes = menuCharacterObject.GetComponentsInChildren<MeshRenderer>();

        var color = Random.ColorHSV(0f, 1f, 0.35f, 0.5f, 1f, 1f);
        fireObject.color = color;
        
        foreach (var mesh in meshes)
        {
            mesh.material.color = color;
        }

        foreach (var ignoreObject in ignore)
        {
            var ignoreMeshes = ignoreObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var mesh in ignoreMeshes)
            {
                mesh.material.color = Color.white;
            }
        }
        
    }
}
