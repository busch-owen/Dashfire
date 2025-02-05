using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NetworkUI : NetworkBehaviour
{
    public static NetworkUI Instance { get; private set; }
    
    [SerializeField] private GameObject scoreboard;
    [SerializeField] private Transform entryLayout;
    [SerializeField] private GameObject scoreBoardEntry;
    [SerializeField] private ScoreboardHandler scoreboardHandler;

    [SerializeField] private TMP_Text
        scoreLimitText,
        leadingScoreText;

    private List<ScoreboardEntry> _spawnedEntries = new();
    private List<ScoreboardEntry> _sortedEntries = new();

    private RoundHandler _roundHandler;
    
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
        _roundHandler = FindFirstObjectByType<RoundHandler>();
        UpdateScoreLimitText();
    }

    public void OpenScoreBoard()
    {
        scoreboard.SetActive(true);
        UpdateLeadingScoreText();
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
        _sortedEntries = scoreboardHandler.SortEntriesByScore(_spawnedEntries);
        for(var i = 0; i < _sortedEntries.Count; i++)
        {
            ChangePlayerNumberRpc(i, i+1);
            _sortedEntries[i].transform.SetSiblingIndex(i);
        }
    }

    [Rpc(SendTo.Server)]
    private void ChangePlayerNumberRpc(int playerIndex, int newNumber)
    {
        var sortedList = scoreboardHandler.SortEntriesByScore(_spawnedEntries);
        sortedList[playerIndex].PlayerData.PlayerNumber.Value = newNumber;
    }

    private void UpdateScoreLimitText()
    {
        scoreLimitText.text = _roundHandler.PointLimit.ToString();
    }
    
    private void UpdateLeadingScoreText()
    {
        if (_sortedEntries.Count > 0)
        {
            leadingScoreText.text = _sortedEntries?[0].playerFrags.ToString();
            return;
        }

        leadingScoreText.text = "0";
    }
}
