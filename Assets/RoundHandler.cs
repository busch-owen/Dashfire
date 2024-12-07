using System;
using Unity.Netcode;
using UnityEngine;

public class RoundHandler : NetworkBehaviour
{
    public static RoundHandler Instance;

    [SerializeField] private int pointLimit;
 
    private void Awake()
    {
        Instance = this;
    }

    public void CheckRoundEnded(int oldValue, int newValue)
    {
        if (newValue >= pointLimit)
        {
            RoundEndedRpc();
        }
    }
    
    [Rpc(SendTo.Server)]
    private void RoundEndedRpc()
    {
        ScoreSaver.Instance.SaveStats();
        Debug.Log("Round Ended");
    }
}
