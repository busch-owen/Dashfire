using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ScoreSaver : NetworkBehaviour
{
    public static ScoreSaver Instance;

    private static Dictionary<ulong, PlayerData> _storedData = new();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
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
        foreach (var id in NetworkManager.ConnectedClients)
        {
            var playerObj = id.Value.PlayerObject;
            if (playerObj == null) return;
            var currentData = playerObj.GetComponent<PlayerData>();
            if (_storedData.Count == 0) return;

            _storedData.TryGetValue(playerObj.OwnerClientId, out var newData);
            if (!newData) return;
            currentData.PlayerWins.Value = newData.PlayerWins.Value;
        }
    }

    public void SaveStats()
    {
        _storedData.Clear();
        foreach (var id in NetworkManager.ConnectedClients)
        {
            var playerObj = id.Value.PlayerObject;
            if (playerObj == null) return;
            var currentData = playerObj.GetComponent<PlayerData>();
            _storedData?.Add(playerObj.OwnerClientId, currentData);
        }
    }
}