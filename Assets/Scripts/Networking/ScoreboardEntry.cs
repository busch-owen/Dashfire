using TMPro;
using Unity.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class ScoreboardEntry : NetworkBehaviour
{
    [SerializeField] private Image colorImage;
    [SerializeField] private TMP_Text numberText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text fragText;
    [SerializeField] private TMP_Text deathText;
    [SerializeField] private TMP_Text winsText;
    [SerializeField] private TMP_Text pingText;

    private PlayerController _assignedPlayer;
    public PlayerData PlayerData { get; private set; }
    public int playerFrags;
    
    public void AssignPlayer(GameObject player)
    {
        _assignedPlayer = player.GetComponent<PlayerController>();
        PlayerData = _assignedPlayer.GetComponent<PlayerData>();
        PlayerData.PlayerNumber.OnValueChanged += OnNumberChanged;
        PlayerData.PlayerName.OnValueChanged += OnNameChanged;
        PlayerData.PlayerFrags.OnValueChanged += OnFragsChanged;
        PlayerData.PlayerDeaths.OnValueChanged += OnDeathsChanged;
        PlayerData.PlayerWins.OnValueChanged += OnWinsChanged;
        PlayerData.PlayerPingMs.OnValueChanged += OnPingChanged;
        
        OnColorChanged(Color.black, PlayerData.PlayerColor.Value);
        OnNumberChanged(0, PlayerData.PlayerNumber.Value);
        OnNameChanged("", PlayerData.PlayerName.Value);
        OnFragsChanged(0, PlayerData.PlayerFrags.Value);
        OnDeathsChanged(0, PlayerData.PlayerDeaths.Value);
        OnWinsChanged(0, PlayerData.PlayerWins.Value);
        OnPingChanged(0, PlayerData.PlayerPingMs.Value);
    }

    private void OnColorChanged(Color previousValue, Color newValue)
    {
        colorImage.color = newValue;
    }
    
    private void OnNumberChanged(int previousValue, int newValue)
    {
        numberText.text = newValue.ToString();
        switch (previousValue)
        {
            case > 1 when newValue <= 1:
                VoiceOverHandler.Instance.PlayTakenLeadClip(_assignedPlayer);
                break;
            case <= 1 when newValue > 1:
                VoiceOverHandler.Instance.PlayLostLeadClip(_assignedPlayer);
                break;
        }
    }

    private void OnNameChanged(FixedString128Bytes previousValue, FixedString128Bytes newValue)
    {
        nameText.text = newValue.ToString();
    }
    
    private void OnFragsChanged(int previousValue, int newValue)
    {
        fragText.text = newValue.ToString();
        gameObject.name = newValue.ToString();
        playerFrags = newValue;
        NetworkUI.Instance.SortScoreboardOrderRpc();
    }
    
    private void OnDeathsChanged(int previousValue, int newValue)
    {
        deathText.text = newValue.ToString();
    }
    
    private void OnWinsChanged(int previousValue, int newValue)
    {
        winsText.text = newValue.ToString();
    }
    
    private void OnPingChanged(ulong previousValue, ulong newValue)
    {
        pingText.text = newValue.ToString();
    }
}
