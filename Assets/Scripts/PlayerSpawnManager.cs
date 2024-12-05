using System;
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
        Debug.Log("Loaded Scene, spawning player");
        if (IsHost && sceneName == "SamLevel2")
        {
            //_currentPlayerIndex = 1;
            _spawnPoints = FindObjectsByType<SpawnPoint>(sortMode: FindObjectsSortMode.None);
            foreach (var id in clientsCompleted)
            {
                var newPlayer = Instantiate(player);
                newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
                newPlayer.transform.position = GetPlayerSpawnPosition();
                //newPlayer.GetComponent<PlayerData>().PlayerNumber.Value = _currentPlayerIndex;
                //_currentPlayerIndex++;
                Debug.LogFormat($"Spawned Player: {newPlayer.name} at position {newPlayer.transform.position}");
            }
        }
    }
    
    private Vector3 GetPlayerSpawnPosition()
    {
        var spawnPoint = _spawnPoints[_currentSpawnPoint].transform.position;
        _currentSpawnPoint++;
        if (_currentSpawnPoint > _spawnPoints.Length) _currentSpawnPoint = 0;
        return spawnPoint;
    }
}
