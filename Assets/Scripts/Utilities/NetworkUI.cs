using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NetworkUI : NetworkBehaviour
{
    [SerializeField] private GameObject scoreboard;
    [SerializeField] private Transform entryLayout;
    [SerializeField] private GameObject scoreBoardEntry;

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
        var newEntry = Instantiate(scoreBoardEntry, entryLayout);
        newEntry.GetComponent<ScoreboardEntry>().AssignPlayer(player);
    }
}
