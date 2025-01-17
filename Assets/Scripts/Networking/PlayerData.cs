using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using Steamworks;

public class PlayerData : NetworkBehaviour
{
    public NetworkVariable<Color> PlayerColor = new();
    public NetworkVariable<int> PlayerNumber = new();
    public NetworkVariable<FixedString128Bytes> PlayerName = new();
    public NetworkVariable<int> PlayerFrags = new();
    public NetworkVariable<int> PlayerDeaths = new();
    public NetworkVariable<int> PlayerWins = new();
    public NetworkVariable<ulong> PlayerPingMs = new();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;
        PlayerColor.Value = Random.ColorHSV(0f, 1f, 0.35f, 0.5f, 1f, 1f);
        PlayerNumber.Value = 0;
        PlayerName.Value = "Player";
        PlayerFrags.Value = 0;
        PlayerDeaths.Value = 0;
        PlayerPingMs.Value = 0;
        PlayerFrags.Value = Mathf.Clamp(PlayerFrags.Value, 0, 999999);
        PlayerDeaths.Value = Mathf.Clamp(PlayerDeaths.Value, 0, 999999);

        PlayerFrags.OnValueChanged += CheckScore;
        PlayerFrags.OnValueChanged += RoundHandler.Instance.CheckRoundEnded;
        
        GetNameClientRpc(new ClientRpcParams()
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] {GetComponent<NetworkObject>().OwnerClientId}
            }
        });
    }

    [ClientRpc]
    private void GetNameClientRpc(ClientRpcParams rpcParams = default)
    {
        GetNameServerRpc(SteamClient.Name);
    }

    [ServerRpc]
    private void GetNameServerRpc(string serverName)
    {
        PlayerName.Value = serverName;
    }

    private void CheckScore(int oldValue, int newValue)
    {
        if (newValue == RoundHandler.Instance.PointLimit)
        {
            RoundHandler.Instance.SetWinningPlayer(NetworkObjectId);
        }
    }
}
