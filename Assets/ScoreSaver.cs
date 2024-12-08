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
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        
    }

    public override void OnNetworkDespawn()
    {
        
    }

    public void ApplyScoresToPlayers()
    {
        if (!IsHost) return;
        
        foreach (var id in NetworkManager.ConnectedClients)
        {
            NetworkManager.ConnectedClients.TryGetValue(id.Value.ClientId, out var client);
            if (client == null) return;
            var playerObj = client.PlayerObject;
            var currentData = playerObj.GetComponent<PlayerData>();
            currentData.PlayerWins = _storedData.Find(playerObj.GetComponent<Predicate<PlayerData>>()).PlayerWins;
            Debug.Log("Loaded data for player: " + currentData.gameObject.name);
        }
    }

    public void SaveStats()
    {
        _storedData?.Clear();
        foreach (var clientInfo in NetworkManager.ConnectedClients)
        {
            var id = clientInfo.Value.ClientId;
            NetworkManager.ConnectedClients.TryGetValue(id, out var client);
            if (client == null) return;
            var playerObj = client.PlayerObject;
            _storedData?.Add(playerObj.GetComponent<PlayerData>());
            playerObj.GetComponent<PlayerData>().ClearValues();
            Debug.Log("Stored data for player" + playerObj.gameObject.name);
        }
    }
}