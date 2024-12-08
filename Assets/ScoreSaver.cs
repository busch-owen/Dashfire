using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ScoreSaver : NetworkBehaviour
{
    public static ScoreSaver Instance;

    private Dictionary<ulong, PlayerData> _storedData = new();

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
        Debug.Log("trying to apply scores");
        foreach (var id in NetworkManager.ConnectedClients)
        {
            var playerObjId = id.Value.PlayerObject.GetComponent<NetworkObject>().NetworkObjectId;
            NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerObjId, out var client);
            if (client == null) return;
            var currentData = client.GetComponent<PlayerData>();
            if (_storedData.Count == 0)
            {
                Debug.Log("no stored data, no stats loaded");
                return;
            }

            _storedData.TryGetValue(client.OwnerClientId, out var newData);
            if (!newData) return;
            currentData.PlayerWins.Value = newData.PlayerWins.Value;
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
            _storedData?.Add(playerObj.OwnerClientId, playerObj.GetComponent<PlayerData>());
            playerObj.GetComponent<PlayerData>().ClearValues();
            Debug.Log("Stored data for player" + playerObj.gameObject.name);
        }
    }
}