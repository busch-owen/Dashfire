using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreSaver : NetworkBehaviour
{
    public static ScoreSaver Instance;

    private List<ulong> _connectedClients = new();
    private List<PlayerData> _storedData = new();

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.SceneManager.OnLoadEventCompleted += ApplyScoresToPlayers;
    }

    private void ApplyScoresToPlayers(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientstTimedOut)
    {
        if (IsHost && SceneManager.GetActiveScene().name == sceneName)
        {
            _connectedClients = clientsCompleted;
            foreach (var id in clientsCompleted)
            {
                NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(id, out var client);
                if (!client) return;
                var currentData = client.GetComponent<PlayerData>();
                currentData.PlayerWins = _storedData.Find(client.GetComponent<Predicate<PlayerData>>()).PlayerWins;
                Debug.Log("Loaded data for player: " + currentData.gameObject.name);
                
            }
        }
    }

    public void SaveStats()
    {
        _storedData?.Clear();
        foreach (var id in _connectedClients)
        {
            NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(id, out var client);
            if (!client) return;
            _storedData?.Add(client.GetComponent<PlayerData>());
            Debug.Log("Stored data for player" + client.gameObject.name);
        }
    }
}