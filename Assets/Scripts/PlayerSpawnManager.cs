using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawnManager : NetworkBehaviour
{
    [SerializeField] private GameObject player;
    
    private SpawnPoint[] _spawnPoints;
    private int _currentSpawnPoint;

    private int _currentPlayerIndex = 1;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoaded;
    }

    private void SceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedout)
    {
        if (IsHost && SceneManager.GetActiveScene().name == sceneName)
        {
            _currentPlayerIndex = 1;
            _spawnPoints = FindObjectsByType<SpawnPoint>(sortMode: FindObjectsSortMode.None);
            foreach (var id in clientsCompleted)
            {
                var newPlayer = Instantiate(player);
                AssignPlayerPositionsRpc(newPlayer.GetComponent<NetworkObject>().NetworkObjectId, GetPlayerSpawnPosition());
                newPlayer.GetComponent<PlayerData>().PlayerNumber.Value = _currentPlayerIndex;
                newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
                _currentPlayerIndex++;
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void AssignPlayerPositionsRpc(ulong playerToMoveId, Vector3 positionToMove)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerToMoveId, out var newPlayer);
        if (!newPlayer) return;
        newPlayer.transform.position = positionToMove;
        Debug.Log("Placed "+ newPlayer.name + "at " + positionToMove);
    }
    
    private Vector3 GetPlayerSpawnPosition()
    {
        var spawnPoint = _spawnPoints[_currentSpawnPoint].transform.position;
        _currentSpawnPoint++;
        if (_currentSpawnPoint > _spawnPoints.Length) _currentSpawnPoint = 0;
        return spawnPoint;
    }
}
