using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using Steamworks;

public class PlayerData : NetworkBehaviour
{
    public NetworkVariable<ulong> PlayerNumber = new();
    public NetworkVariable<FixedString128Bytes> PlayerName = new();
    public NetworkVariable<int> PlayerFrags = new();
    public NetworkVariable<int> PlayerDeaths = new();
    public NetworkVariable<int> PlayerWins = new();
    public NetworkVariable<ulong> PlayerPingMs = new();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;
        PlayerNumber.Value = 0;
        PlayerName.Value = "Player";
        PlayerFrags.Value = 0;
        PlayerDeaths.Value = 0;
        PlayerWins.Value = 0;
        PlayerPingMs.Value = 0;
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
    private void GetNameServerRpc(string name)
    {
        PlayerName.Value = name;
    }
}
