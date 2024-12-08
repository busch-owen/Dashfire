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
    
    public override void OnNetworkDespawn()
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= SceneLoaded;
    }


    private void SceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedout)
    {
        if (!IsHost || SceneManager.GetActiveScene().name != sceneName) return;
        _currentPlayerIndex = 1;
        _spawnPoints = FindObjectsByType<SpawnPoint>(sortMode: FindObjectsSortMode.None);
        Debug.Log(clientsCompleted.Count);
        var oldPlayerObjects = FindObjectsByType<PlayerController>(sortMode: FindObjectsSortMode.None);
        foreach (var id in clientsCompleted)
        {
            Debug.LogFormat($"spawned player {_currentPlayerIndex}");
            var newPlayer =  NetworkManager.SpawnManager.InstantiateAndSpawn(player.GetComponent<NetworkObject>(), id, false, true,
                false, GetPlayerSpawnPosition());
            NetworkManager.Singleton.ConnectedClients[id].PlayerObject = newPlayer;
            //AssignPlayerPositionsRpc(newPlayer.GetComponent<NetworkObject>().NetworkObjectId, GetPlayerSpawnPosition());
            newPlayer.GetComponent<PlayerData>().PlayerNumber.Value = _currentPlayerIndex;
            _currentPlayerIndex++;
        }

        foreach (var obj in oldPlayerObjects)
        {
            obj.GetComponent<NetworkObject>().Despawn();
            Destroy(obj);
        }
        
        ScoreSaver.Instance.ApplyScoresToPlayers();
    }
    
    private Vector3 GetPlayerSpawnPosition()
    {
        var spawnPoint = _spawnPoints[_currentSpawnPoint].transform.position;
        _currentSpawnPoint++;
        if (_currentSpawnPoint > _spawnPoints.Length) _currentSpawnPoint = 0;
        return spawnPoint;
    }
}
