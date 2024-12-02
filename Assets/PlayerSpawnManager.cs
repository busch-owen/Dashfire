using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    private SpawnPoint[] _spawnPoints;

    private NetworkManager _manager;
    
    private void Start()
    {
        _spawnPoints = FindObjectsByType<SpawnPoint>(sortMode: FindObjectsSortMode.None);
        _manager = GetComponent<NetworkManager>();
        _manager.ConnectionApprovalCallback += ConnectionApprovalCallback;
    }
    
    private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
        response.CreatePlayerObject = true;
        response.Position = GetPlayerSpawnPosition();
    }
    
    private Vector3 GetPlayerSpawnPosition()
    {
        var randomPoint = Random.Range(0, _spawnPoints.Length);
        return _spawnPoints[randomPoint].transform.position;
    }
}
