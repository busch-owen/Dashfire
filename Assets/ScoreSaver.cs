using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreSaver : NetworkBehaviour
{
    public static ScoreSaver Instance;
    
    private List<PlayerData> _storedData = new();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.SceneManager.OnLoadEventCompleted += SaveStats;
        NetworkManager.SceneManager.OnLoadEventCompleted += ApplyScoresToPlayers;
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.SceneManager.OnLoadEventCompleted -= SaveStats;
        NetworkManager.SceneManager.OnLoadEventCompleted -= ApplyScoresToPlayers;
    }

    private void ApplyScoresToPlayers(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientstTimedOut)
    {
        if (!IsHost || SceneManager.GetActiveScene().name != sceneName) return;
        
        foreach (var id in clientsCompleted)
        {
            NetworkManager.ConnectedClients.TryGetValue(id, out var client);
            if (client == null) return;
            var playerObj = client.PlayerObject;
            var currentData = playerObj.GetComponent<PlayerData>();
            currentData.PlayerWins = _storedData.Find(playerObj.GetComponent<Predicate<PlayerData>>()).PlayerWins;
            Debug.Log("Loaded data for player: " + currentData.gameObject.name);
                
        }
    }

    public void SaveStats(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        _storedData?.Clear();
        foreach (var id in clientsCompleted)
        {
            NetworkManager.ConnectedClients.TryGetValue(id, out var client);
            if (client == null) return;
            var playerObj = client.PlayerObject;
            _storedData?.Add(playerObj.GetComponent<PlayerData>());
            playerObj.GetComponent<PlayerData>().ClearValues();
            Debug.Log("Stored data for player" + playerObj.gameObject.name);
        }
    }
}