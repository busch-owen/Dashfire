using Steamworks.Data;
using UnityEngine;

public class LobbySaver : MonoBehaviour
{
    public static LobbySaver Instance;
    public Lobby? CurrentLobby;
    private void Awake()
    {
        Instance = this;
    }
}
