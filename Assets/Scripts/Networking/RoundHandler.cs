using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class RoundHandler : NetworkBehaviour
{
    public static RoundHandler Instance;

    [SerializeField] private string[] mapPool;
    [SerializeField] private float endGameDuration;

    private WaitForSeconds _waitForEndDuration;
    
    [field: SerializeField] public int PointLimit { get; private set; }

    private ulong _winningPlayerId;

    //public event Action RoundEndedEvent;
 
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;

        _waitForEndDuration = new WaitForSeconds(endGameDuration);
    }

    public void CheckRoundEnded(int oldValue, int newValue)
    {
        if (newValue >= PointLimit)
        {
            StartCoroutine(RoundEnded());
        }
    }
    
    private IEnumerator RoundEnded()
    {
        if (!NetworkManager.Singleton.IsHost) yield return null;
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(_winningPlayerId, out var winningPlayer);

        if (winningPlayer)
        {
            var playerData = winningPlayer.GetComponent<PlayerData>();
            playerData.PlayerWins.Value++;
        }

        var allPlayers = FindObjectsByType<PlayerController>(sortMode: FindObjectsSortMode.None);

        foreach (var player in allPlayers)
        {
            player.GetComponent<PlayerInputHandler>().DisableInput();
        }
        
        FindFirstObjectByType<NetworkUI>().OpenScoreBoard();
        
        ScoreSaver.Instance.SaveStats();
        Debug.Log("Round Ended");
        yield return _waitForEndDuration;
        NetworkManager.SceneManager.LoadScene(PickRandomLevel(), LoadSceneMode.Single);
    }

    public void SetWinningPlayer(ulong winningPlayerId)
    {
        _winningPlayerId = winningPlayerId;
    }

    public string PickRandomLevel()
    {
        var randomLevel = Random.Range(0, mapPool.Length);
        return mapPool[randomLevel];
    }
}
