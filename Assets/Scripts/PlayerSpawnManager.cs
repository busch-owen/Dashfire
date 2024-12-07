using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawnManager : NetworkBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float spawnDelay;

    private WaitForSeconds _waitForSpawnDelay;
    
    private SpawnPoint[] _spawnPoints;
    private int _currentSpawnPoint;

    private int _currentPlayerIndex = 1;

    private GameObject _currentPlayerObj;
    private Vector3 _currentNewPosition;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        _waitForSpawnDelay = new WaitForSeconds(spawnDelay);
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
                newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(id, true);
                AssignPlayerPositionsRpc(newPlayer.GetComponent<NetworkObject>().NetworkObjectId, GetPlayerSpawnPosition());
                newPlayer.GetComponent<PlayerData>().PlayerNumber.Value = _currentPlayerIndex;
                _currentPlayerIndex++;
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void AssignPlayerPositionsRpc(ulong playerToMoveId, Vector3 positionToMove)
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerToMoveId, out var newPlayer);
        if (!newPlayer) return;
        _currentPlayerObj = newPlayer.gameObject;
        _currentNewPosition = positionToMove;
        Invoke(nameof(ApplyPositions), spawnDelay);
        Debug.Log("Placed "+ newPlayer.name + "at " + positionToMove);
    }

    private void ApplyPositions()
    {
        _currentPlayerObj.transform.position = _currentNewPosition;
    }
    
    private Vector3 GetPlayerSpawnPosition()
    {
        var spawnPoint = _spawnPoints[_currentSpawnPoint].transform.position;
        _currentSpawnPoint++;
        if (_currentSpawnPoint > _spawnPoints.Length) _currentSpawnPoint = 0;
        return spawnPoint;
    }
}
