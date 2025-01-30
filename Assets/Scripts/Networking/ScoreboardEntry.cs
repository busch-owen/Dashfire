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

    public int playerFrags;
    
    public void AssignPlayer(GameObject player)
    {
        var playerData = player.GetComponent<PlayerData>();
        playerData.PlayerNumber.OnValueChanged += OnNumberChanged;
        playerData.PlayerName.OnValueChanged += OnNameChanged;
        playerData.PlayerFrags.OnValueChanged += OnFragsChanged;
        playerData.PlayerDeaths.OnValueChanged += OnDeathsChanged;
        playerData.PlayerWins.OnValueChanged += OnWinsChanged;
        playerData.PlayerPingMs.OnValueChanged += OnPingChanged;
        
        OnColorChanged(Color.black, playerData.PlayerColor.Value);
        OnNumberChanged(0, playerData.PlayerNumber.Value);
        OnNameChanged("", playerData.PlayerName.Value);
        OnFragsChanged(0, playerData.PlayerFrags.Value);
        OnDeathsChanged(0, playerData.PlayerDeaths.Value);
        OnWinsChanged(0, playerData.PlayerWins.Value);
        OnPingChanged(0, playerData.PlayerPingMs.Value);
    }

    private void OnColorChanged(Color previousValue, Color newValue)
    {
        colorImage.color = newValue;
    }
    
    private void OnNumberChanged(int previousValue, int newValue)
    {
        numberText.text = newValue.ToString();
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
