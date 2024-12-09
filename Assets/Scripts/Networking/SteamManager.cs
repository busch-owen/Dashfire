using System;
using Netcode.Transports.Facepunch;
using UnityEngine;
using Steamworks;
using TMPro;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class SteamManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField lobbyIdInputField;

    [SerializeField] private TMP_Text lobbyIdText;

    [SerializeField] private GameObject mainMenu;

    [SerializeField] private GameObject inlobbyMenu;

    private void OnEnable()
    {
        SteamMatchmaking.OnLobbyCreated += LobbyCreated;
        SteamMatchmaking.OnLobbyEntered += LobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested += GameLobbyJoinRequest;
    }

    private async void GameLobbyJoinRequest(Lobby lobby, SteamId steamId)
    {
        await lobby.Join();
    }

    private void LobbyEntered(Lobby lobby)
    {
        LobbySaver.Instance.CurrentLobby = lobby;
        lobbyIdText.text = lobby.Id.ToString();
        CheckUI();
        
        if(NetworkManager.Singleton.IsHost) return;
        NetworkManager.Singleton.gameObject.GetComponent<FacepunchTransport>().targetSteamId = lobby.Owner.Id;
        NetworkManager.Singleton.StartClient();
    }

    private void LobbyCreated(Result result, Lobby lobby)
    {
        if (result == Result.OK)
        {
            lobby.SetPublic();
            lobby.SetJoinable(true);
            NetworkManager.Singleton.StartHost();
        }
    }

    private void OnDisable()
    {
        SteamMatchmaking.OnLobbyCreated -= LobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= LobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested -= GameLobbyJoinRequest;
    }

    public async void HostLobby()
    {
        await SteamMatchmaking.CreateLobbyAsync(8);
    }

    public async void JoinLobbyWithId()
    {
        if (!ulong.TryParse(lobbyIdInputField.text, out var ID)) return;

        var lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();

        foreach (var lobby in lobbies)
        {
            if (lobby.Id != ID) continue;
            await lobby.Join();
            return;
        }
    }

    public void CopyId()
    {
        var textEditor = new TextEditor();
        textEditor.text = lobbyIdText.text;
        textEditor.SelectAll();
        textEditor.Copy();
    }

    public void LeaveLobby()
    {
        LobbySaver.Instance.CurrentLobby?.Leave();
        LobbySaver.Instance.CurrentLobby = null;
        NetworkManager.Singleton.Shutdown();
        CheckUI();
    }

    private void CheckUI()
    {
        if (LobbySaver.Instance.CurrentLobby == null)
        {
            mainMenu.SetActive(true);
            inlobbyMenu.SetActive(false);
            return;
        }
        
        mainMenu.SetActive(false);
        inlobbyMenu.SetActive(true);
    }

    public void StartGameServer()
    {
        if (!NetworkManager.Singleton.IsHost) return;
        NetworkManager.Singleton.SceneManager.LoadScene(RoundHandler.Instance.PickRandomLevel(), LoadSceneMode.Single);
    }
}
