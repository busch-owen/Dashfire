using System;
using System.ComponentModel;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : NetworkBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TMP_Text playersCountText;
    [SerializeField] private GameObject overviewCamera;

    private NetworkManager _localNetworkManager;

    private NetworkVariable<int> _playerCount = new();

    private void Awake()
    {
        _localNetworkManager = FindAnyObjectByType<NetworkManager>();
        
        hostButton.onClick.AddListener(() =>
        {
            try
            {
                NetworkManager.Singleton.StartHost();
            }
            catch (WarningException e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        });

        clientButton.onClick.AddListener(() =>
        {
            try
            {
                NetworkManager.Singleton.StartClient();
            }
            catch (WarningException e)
            {
                Console.WriteLine(e);
                throw;
            }
        });

        _localNetworkManager.OnServerStarted += () =>
        {
            overviewCamera.SetActive(false);
            gameObject.SetActive(false);
        };
        
        _localNetworkManager.OnClientStarted += () =>
        {
            overviewCamera.SetActive(false);
            gameObject.SetActive(false);
        };
    }

    private void Update()
    {
        playersCountText.text = $"Players: {_playerCount.Value.ToString()}";
        if (!IsServer) return;
        _playerCount.Value = NetworkManager.Singleton.ConnectedClients.Count;
    }
}
