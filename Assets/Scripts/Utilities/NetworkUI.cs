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

    private NetworkVariable<int> _playerCount = new();

    private void Awake()
    {
        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            overviewCamera.SetActive(false);
        });
        
        clientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            overviewCamera.SetActive(false);
        });
    }

    private void Update()
    {
        playersCountText.text = $"Players: {_playerCount.Value.ToString()}";
        if (!IsServer) return;
        _playerCount.Value = NetworkManager.Singleton.ConnectedClients.Count;
    }
}
