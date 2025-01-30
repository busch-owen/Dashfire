using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkUI : NetworkBehaviour
{
    public static NetworkUI Instance { get; private set; }
    
    [SerializeField] private GameObject scoreboard;
    [SerializeField] private Transform entryLayout;
    [SerializeField] private GameObject scoreBoardEntry;
    [SerializeField] private ScoreboardHandler scoreboardHandler;

    private List<ScoreboardEntry> _spawnedEntries = new();
    
    private NetworkManager _localNetworkManager;
    
    private void OnEnable()
    {
        PlayerController.OnPlayerSpawned += AddPlayerEntry;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerSpawned -= AddPlayerEntry;
    }

    private void Awake()
    {
        Instance = this;
        scoreboard?.SetActive(false);
    }

    public void OpenScoreBoard()
    {
        scoreboard.SetActive(true);
    }
    public void CloseScoreBoard()
    {
        scoreboard.SetActive(false);
    }

    private void AddPlayerEntry(GameObject player)
    {
        var newEntry = Instantiate(scoreBoardEntry, entryLayout).GetComponent<ScoreboardEntry>();
        newEntry.AssignPlayer(player);
        _spawnedEntries.Add(newEntry);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SortScoreboardOrderRpc()
    {
        var sortedList = scoreboardHandler.SortEntriesByScore(_spawnedEntries);
        for(var i = 0; i < sortedList.Count; i++)
        {
            sortedList[i].transform.SetSiblingIndex(i);
        }
    }
}
