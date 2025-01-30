using System;
using Unity.Netcode;
using UnityEngine;

public class VoiceOverHandler : NetworkBehaviour
{
    public static VoiceOverHandler Instance { get; private set; }

    private SoundHandler _globalHandler;
    
    [SerializeField] private AudioClip 
        headshot,
        leadLost,
        leadTaken,
        remains1,
        remains2,
        remains3,
        remains5,
        remains10,
        roundStart,
        roundOver,
        winner;

    private void Awake()
    {
        _globalHandler = GetComponent<SoundHandler>();
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void PlayHeadshotClip(PlayerController targetPlayer)
    {
        targetPlayer.GetComponent<SoundHandler>().PlayClipWithStaticPitch(headshot);
    }
    
    public void PlayLostLeadClip(PlayerController targetPlayer)
    {
        targetPlayer.GetComponent<SoundHandler>().PlayClipWithStaticPitch(leadLost);
    }
    
    public void PlayTakenLeadClip(PlayerController targetPlayer)
    {
        targetPlayer.GetComponent<SoundHandler>().PlayClipWithStaticPitch(leadTaken);
    }
    
    public void PlayWinnerClip(PlayerController targetPlayer)
    {
        targetPlayer.GetComponent<SoundHandler>().PlayClipWithStaticPitch(winner);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void PlayRemainsClipRpc(int remains)
    {
        switch (remains)
        {
            case 1:
                _globalHandler.PlayClipWithStaticPitch(remains1);
                break;
            case 2:
                _globalHandler.PlayClipWithStaticPitch(remains2);
                break;
            case 3:
                _globalHandler.PlayClipWithStaticPitch(remains3);
                break;
            case 5:
                _globalHandler.PlayClipWithStaticPitch(remains5);
                break;
            case 10:
                _globalHandler.PlayClipWithStaticPitch(remains10);
                break;
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void PlayRoundStartClipRpc()
    {
        _globalHandler.PlayClipWithStaticPitch(roundStart);
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    public void PlayRoundEndClipRpc()
    {
        _globalHandler.PlayClipWithStaticPitch(roundStart);
    }
}
