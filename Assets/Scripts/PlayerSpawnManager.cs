using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
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
        if (IsHost && sceneName == "SamLevel2")
        {
            _currentPlayerIndex = 1;
            _spawnPoints = FindObjectsByType<SpawnPoint>(sortMode: FindObjectsSortMode.None);
            foreach (var id in clientsCompleted)
            {
                var newPlayer = Instantiate(player);
                newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
                AssignPlayerPositionsRpc(newPlayer.GetComponent<NetworkObject>().NetworkObjectId, GetPlayerSpawnPosition());
                newPlayer.GetComponent<PlayerData>().PlayerNumber.Value = _currentPlayerIndex;
                _currentPlayerIndex++;
                Debug.LogFormat($"Spawned Player: {newPlayer.name} at position {newPlayer.transform.position}");
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void AssignPlayerPositionsRpc(ulong playerToMoveId, Vector3 positionToMove)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerToMoveId, out var newPlayer);
        if (!newPlayer) return;
        newPlayer.transform.position = positionToMove;
    }
    
    private Vector3 GetPlayerSpawnPosition()
    {
        var spawnPoint = _spawnPoints[_currentSpawnPoint].transform.position;
        _currentSpawnPoint++;
        if (_currentSpawnPoint > _spawnPoints.Length) _currentSpawnPoint = 0;
        return spawnPoint;
    }
}
