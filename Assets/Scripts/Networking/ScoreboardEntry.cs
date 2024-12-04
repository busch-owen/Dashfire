using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class ScoreboardEntry : NetworkBehaviour
{
    [SerializeField] private TMP_Text numberText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text fragText;
    [SerializeField] private TMP_Text deathText;
    [SerializeField] private TMP_Text winsText;
    [SerializeField] private TMP_Text pingText;

    private PlayerController _assignedController;
    
    private NetworkVariable<ulong> _playerNumber = new(value: default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    //private NetworkVariable<string> _playerName = new(value: default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> _playerFrags = new(value: default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> _playerDeaths = new(value: default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> _playerWins = new(value: default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<ulong> _playerPingMs = new(value: default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public void AssignController(PlayerController controller)
    {
        _assignedController = controller;
        _playerNumber.Value = _assignedController.NetworkObjectId;
        numberText.text = _playerNumber.Value.ToString();
        //_playerName.Value = "Player " + _playerNumber;
    }

    public void IncreaseFragCount()
    {
        _playerFrags.Value++;
        fragText.text = _playerFrags.Value.ToString();
    }
    
    public void IncreaseDeathCount()
    {
        _playerDeaths.Value++;
        deathText.text = _playerFrags.Value.ToString();
    }
    
    public void IncreaseWinCount()
    {
        _playerWins.Value++;
        winsText.text = _playerFrags.Value.ToString();
    }

    public void UpdatePingPreview()
    {
        _playerPingMs.Value = NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(_assignedController.OwnerClientId);
        pingText.text = _playerPingMs.ToString();
    }
}
