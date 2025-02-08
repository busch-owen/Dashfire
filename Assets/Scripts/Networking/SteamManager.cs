using System;
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
using Image = Steamworks.Data.Image;

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
        SteamMatchmaking.OnLobbyMemberJoined += LobbyJoined;
        SteamMatchmaking.OnLobbyMemberLeave += LobbyLeft;
        SteamMatchmaking.OnLobbyMemberDisconnected += LobbyLeft;
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
    
    private void LobbyJoined(Lobby lobby, Friend friend)
    {
        CheckUI();
    }
    
    private void LobbyLeft(Lobby arg1, Friend arg2)
    {
        CheckUI();
    }
    
    private async void RefreshLobbyEntries(Lobby lobby)
    {
        foreach (var entry in _spawnedEntries)
        {
            Destroy(entry.gameObject);
        }
        _spawnedEntries.Clear();
        foreach (var member in lobby.Members)
        {
            var newEntry = Instantiate(lobbyScreenEntry, lobbyLayout);
            _spawnedEntries.Add(newEntry);
            newEntry.GetComponentInChildren<TMP_Text>().text = member.Name;
            var avatar = GetAvatar(member.Id);
            await Task.WhenAll(avatar);
            if (avatar.Result == null) return;
            newEntry.GetComponentInChildren<RawImage>().texture = Covert(avatar.Result.Value);
        }
    }

    private static async Task<Image?> GetAvatar(SteamId id)
    {
        try
        {
            // Get Avatar using await
            return await SteamFriends.GetLargeAvatarAsync(id);
        }
        catch (Exception e)
        {
            // If something goes wrong, log it
            Debug.Log( e );
            return null;
        }
    }
    
    private static Texture2D Covert(Image image)
    {
        // Create a new Texture2D
        var avatar = new Texture2D((int)image.Width, (int)image.Height, TextureFormat.ARGB32, false)
            {
                // Set filter type, or else its really blury
                filterMode = FilterMode.Trilinear
            };

        // Flip image
        for (var x = 0; x < image.Width; x++)
        {
            for (var y = 0; y < image.Height; y++)
            {
                var p = image.GetPixel(x, y);
                avatar.SetPixel(x, (int)image.Height - y,
                    new UnityEngine.Color(p.r / 255.0f, p.g / 255.0f, p.b / 255.0f, p.a / 255.0f));
            }
        }

        avatar.Apply();
        return avatar;
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
