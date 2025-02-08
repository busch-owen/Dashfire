using System.Collections.Generic;
using System.Threading.Tasks;
using Netcode.Transports.Facepunch;
using UnityEngine;
using Steamworks;
using TMPro;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SteamManager : NetworkBehaviour
{
    [SerializeField] private TMP_InputField lobbyIdInputField;

    [SerializeField] private TMP_Text lobbyIdText;

    [SerializeField] private GameObject mainMenu;

    [SerializeField] private GameObject inLobbyMenu;
    
    [SerializeField] private GameObject lobbyScreenEntry;
    [SerializeField] private Transform lobbyLayout;
    
    private RoundHandler _roundHandler;

    private int _playerCount = 8;
    private bool _publicLobby = true;

    private List<GameObject> _spawnedEntries = new();
    
    private void OnEnable()
    {
        SteamMatchmaking.OnLobbyCreated += LobbyCreated;
        SteamMatchmaking.OnLobbyEntered += LobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested += GameLobbyJoinRequest;

        _roundHandler ??= FindFirstObjectByType<RoundHandler>();
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
        
        if (NetworkManager.Singleton.IsHost) return;
        NetworkManager.Singleton.gameObject.GetComponent<FacepunchTransport>().targetSteamId = lobby.Owner.Id;
        NetworkManager.Singleton.StartClient();
    }


    private void RefreshLobbyEntries(Lobby lobby)
    {
        Debug.Log("hello");
        foreach (var entry in _spawnedEntries)
        {
            Destroy(entry);
        }
        _spawnedEntries.Clear();
        foreach (var member in lobby.Members)
        {
            var newEntry = Instantiate(lobbyScreenEntry, lobbyLayout);
            _spawnedEntries.Add(newEntry);
            newEntry.GetComponentInChildren<TMP_Text>().text = member.Name;
        }
    }
    
    private void LobbyCreated(Result result, Lobby lobby)
    {
        if (result == Result.OK)
        {
            if (_publicLobby)
            {
                lobby.SetPublic();
            }
            else
            {
                lobby.SetPrivate();
            }
            
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
        await SteamMatchmaking.CreateLobbyAsync(_playerCount);
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
            inLobbyMenu.SetActive(false);
            return;
        }
        
        RefreshLobbyEntries(LobbySaver.Instance.CurrentLobby.Value);
        mainMenu.SetActive(false);
        inLobbyMenu.SetActive(true);
    }

    public void StartGameServer()
    {
        if (!NetworkManager.Singleton.IsHost) return;
        NetworkManager.Singleton.SceneManager.LoadScene(RoundHandler.Instance.PickRandomLevel(), LoadSceneMode.Single);
    }

    public void ChangeLobbyType(bool type)
    {
        _publicLobby = type;
    }

    public void ChangePlayerCount(string input)
    {
        _playerCount = int.Parse(input);
    }

    public void ChangePointLimit(string amount)
    {
        _roundHandler.ChangePointLimit(int.Parse(amount));
    }
}
