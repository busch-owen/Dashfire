using UnityEngine;
using Unity.Netcode;

public class ScoreboardEntry : NetworkBehaviour
{
    private NetworkVariable<ulong> _playerNumber;
    private NetworkVariable<int> _playerFrags;
    private NetworkVariable<int> _playerDeaths;
    private NetworkVariable<int> _playerWins;
    private NetworkVariable<int> _playerPingMs;
}
