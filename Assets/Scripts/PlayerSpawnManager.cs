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
            _spawnPoints = FindObjectsByType<SpawnPoint>(sortMode: FindObjectsSortMode.None);
            foreach (var id in clientsCompleted)
            {
                var newPlayer = Instantiate(player);
                newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
                newPlayer.transform.position = GetPlayerSpawnPosition();
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
