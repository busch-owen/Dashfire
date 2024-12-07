using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class RoundHandler : NetworkBehaviour
{
    public static RoundHandler Instance;

    [SerializeField] private string[] mapPool;
    
    [field: SerializeField] public int PointLimit { get; private set; }

    private ulong _winningPlayerId;

    public event Action RoundEndedEvent;
 
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public void CheckRoundEnded(int oldValue, int newValue)
    {
        if (newValue >= PointLimit)
        {
            RoundEnded();
        }
    }
    
    private void RoundEnded()
    {
        NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(_winningPlayerId, out var winningPlayer);
        if (winningPlayer)
            winningPlayer.GetComponent<PlayerData>().PlayerWins.Value++;
        ScoreSaver.Instance.SaveStats();
        Debug.Log("Round Ended");
        if (!NetworkManager.Singleton.IsHost) return;
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
