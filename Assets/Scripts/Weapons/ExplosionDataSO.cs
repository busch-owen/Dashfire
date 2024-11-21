using UnityEngine;

[CreateAssetMenu(fileName = "ExplosionData", menuName = "Scriptable Objects/ExplosionData")]
public class ExplosionDataSO : ScriptableObject
{
    [field: SerializeField] public float ExplosionRadius { get; private set; }
    [field: SerializeField] public float ExplosionForce { get; private set; }
}
