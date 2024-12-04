using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NetworkUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text playersCountText;
    [SerializeField] private GameObject hostMenu;
    [SerializeField] private GameObject scoreboard;
    [SerializeField] private Transform entryLayout;
    [SerializeField] private GameObject scoreBoardEntry;

    private NetworkManager _localNetworkManager;

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

    public ScoreboardEntry AddPlayerToScoreboard(PlayerController controller)
    {
        scoreboard.SetActive(true);
        var entry = Instantiate(scoreBoardEntry, entryLayout).GetComponent<ScoreboardEntry>();
        entry.transform.SetParent(entryLayout);
        entry.AssignController(controller);
        Debug.Log("Entry Created");
        scoreboard.SetActive(false);
        return entry;
    }
    
    public void IncreaseEntryFragCount(ScoreboardEntry entry)
    {
        entry.IncreaseFragCount();
    }
    
    public void IncreaseEntryDeathCount(ScoreboardEntry entry)
    {
        entry.IncreaseDeathCount();
    }
    
    public void IncreaseEntryWinCount(ScoreboardEntry entry)
    {
        entry.IncreaseWinCount();
    }

    public void UpdateEntryPingPreview(ScoreboardEntry entry)
    {
        entry.UpdatePingPreview();
    }
}
