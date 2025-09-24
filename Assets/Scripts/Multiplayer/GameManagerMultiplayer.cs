using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class GameManagerMultiplayer : SimulationBehaviour, INetworkRunnerCallbacks
{
    public static GameManagerMultiplayer instance { get; private set; }

    public LevelLoaderAsync levelLoaderMenu;
    public LevelLoaderGame levelLoader;

    public bool isPlayerReadying = false;
    public Button singlePlayerButton;
    public Button multiplayerButton;
    public Button configButton;
    public Button exitButton;
    public Button SinglePlayer;

    public TextMeshProUGUI roomCountPlayers;
    public Button readyButton;
    private Dictionary<PlayerRef, bool> _playersReady = new();

    private string GetSceneInfo()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneRef currentSceneRef = SceneRef.FromIndex(currentScene.buildIndex);
        return currentSceneRef.ToString();
    }

    [Header("Configurações de Cena")]
    [SerializeField] private string _roomName = "SubstratoNEXT";
    // Scene 0 é a cena atual onde este script está.
    [SerializeField] private int _lobbySceneIndex = 1;
    [SerializeField] private int _gameplaySceneIndex = 2;

    [Header("Configurações de Gameplay")]
    public NetworkObject _playerPrefab;
    public NetworkObject _playerPrefabLvl01;
    private int _minPlayersToStartGame = 1;

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    [SerializeField] private bool _isLoadingGameplayScene = false;

    void Awake()
    {
        if (GetSceneInfo() == SceneRef.FromIndex(0).ToString())
        {
            singlePlayerButton = GameObject.Find("SinglePlayer")?.GetComponent<Button>();
            multiplayerButton = GameObject.Find("MultiPlayer")?.GetComponent<Button>();
            configButton = GameObject.Find("Configuracoes")?.GetComponent<Button>();
            exitButton = GameObject.Find("Sair")?.GetComponent<Button>();
        }
    }

    void Start()
    {
        levelLoaderMenu = FindFirstObjectByType<LevelLoaderAsync>();
        if (singlePlayerButton)
        {
            singlePlayerButton.onClick.AddListener(() => OnGameModeSelected(GameMode.Single));
        }
        if (multiplayerButton)
        {
            multiplayerButton.onClick.AddListener(() => OnGameModeSelected(GameMode.AutoHostOrClient));
        }
        if (configButton)
        {
            configButton.onClick.AddListener(() => Debug.Log("Configurações clicadas!"));
        }
        if (exitButton)
        {
            exitButton.onClick.AddListener(() => Application.Quit());
        }
    }

    private async void OnGameModeSelected(GameMode selectedMode)
    {
        if (Runner == null || !Runner.IsRunning)
        {
            await StartNetwork(selectedMode);
        }
        else
        {
            Debug.Log("Network já está rodando.");
        }
    }

    public async Task StartNetwork(GameMode mode)
    {
        var runner = GetComponent<NetworkRunner>();
        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
        }

        DontDestroyOnLoad(gameObject);
        runner.ProvideInput = true;

        var sceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null)
        {
            sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        runner.AddCallbacks(this);
        Debug.Log($"Iniciando jogo no modo: {mode} com a sala: {_roomName}");

        if (levelLoaderMenu != null)
        {
            levelLoaderMenu.TransicaoComCallback(async () =>
            {
                var result = await runner.StartGame(new StartGameArgs()
                {
                    GameMode = mode,
                    SessionName = _roomName,
                    Scene = SceneRef.FromIndex(_lobbySceneIndex),
                    PlayerCount = 4,
                    SceneManager = sceneManager
                });

                if (result.Ok)
                {
                    Debug.Log("Jogo iniciado com sucesso!");
                }
                else
                {
                    Debug.LogError("Erro ao iniciar o jogo: " + result.ShutdownReason);
                }
            });
        }
        else
        {
            Debug.LogWarning("LevelLoader não encontrado. Iniciando jogo direto.");

            var result = await runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = _roomName,
                Scene = SceneRef.FromIndex(_lobbySceneIndex),
                PlayerCount = 4,
                SceneManager = sceneManager
            });

            if (result.Ok)
            {
                Debug.Log("Jogo iniciado com sucesso!");
            }
            else
            {
                Debug.LogError("Erro ao iniciar o jogo: " + result.ShutdownReason);
            }
        }
    }

    public void RequestButtonReady()
    {
        if (GetSceneInfo() == SceneRef.FromIndex(1).ToString())
        {
            if (readyButton != null)
            {
                readyButton = GameObject.FindGameObjectWithTag("BtnReady").GetComponent<Button>();
            }
            else
            {
                readyButton = GameObject.FindGameObjectWithTag("BtnReady").GetComponent<Button>();
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner == null) return;
        if (!Runner.IsServer) return;

        if (GetSceneInfo() == SceneRef.FromIndex(1).ToString())
        {
            roomCountPlayers = GameObject.Find("TextoConnect")?.GetComponent<TextMeshProUGUI>();
            InvokeRepeating(nameof(RequestButtonReady), 2f, 1f);
            levelLoader = FindFirstObjectByType<LevelLoaderGame>();

            roomCountPlayers.text = $"{Runner.ActivePlayers.Count()} / 4";

            if (readyButton != null)
            {
                readyButton.onClick.RemoveAllListeners();
                readyButton.onClick.AddListener(IsplayerReadying);
                Debug.Log("Botão Ready configurado.");
            }
        }
    }

    public void IsplayerReadying()
    {
        isPlayerReadying = true;
        _playersReady[Runner.LocalPlayer] = true;
        Debug.Log($"Jogador {Runner.LocalPlayer} está pronto.");

        if (Runner.IsSceneAuthority)
        {
            Debug.Log($"Jogador {Runner.LocalPlayer} marcou como pronto. Total prontos: {_playersReady.Count}/{Runner.ActivePlayers.Count()}");
            // Verifica se todos estão prontos
            if (_playersReady.Count >= _minPlayersToStartGame)
            {
                if (!_isLoadingGameplayScene)
                {
                    Debug.Log("Todos os jogadores prontos. Iniciando cena de gameplay.");
                    _isLoadingGameplayScene = true;
                    levelLoader.Transicao(SceneRef.FromIndex(_gameplaySceneIndex), Runner);
                }
            }
        }
    }

    // --- Implementação de INetworkRunnerCallbacks ---
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Jogador {player} entrou. Cena atual: {GetSceneInfo()}");
        if (runner.IsServer)
        {
            if (GetSceneInfo() == SceneRef.FromIndex(_lobbySceneIndex).ToString())
            {
                if (_playerPrefab != null)
                {
                    Vector3 spawnPosition = new Vector3(0.16f, 0.8f, -10f);
                    NetworkObject networkPlayerObject = runner.Spawn(_playerPrefab, spawnPosition, Quaternion.identity, player);
                    _spawnedCharacters[player] = networkPlayerObject;
                    Debug.Log($"Prefab do jogador spawnado para {player} na cena {GetSceneInfo()}.");
                }
                else
                {
                    Debug.LogError("Player Prefab não atribuído.");
                }
            }
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Jogador {player} saiu.");
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            if (networkObject != null)
            {
                runner.Despawn(networkObject);
            }
            _spawnedCharacters.Remove(player);
        }
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        var currentSceneRef = SceneRef.FromIndex(currentScene.buildIndex);
        Debug.Log($"OnSceneLoadDone: Cena {currentSceneRef} carregada.");

        //Se for a cena de gameplay, spawna aqui.
        if (runner.IsServer && currentSceneRef == SceneRef.FromIndex(_gameplaySceneIndex))
        {
            foreach (var player in runner.ActivePlayers)
            {
                Vector3 spawnPos = new Vector3(-16.4f, 1f, 8.04f);
                var obj = runner.Spawn(_playerPrefabLvl01, spawnPos,
                                       Quaternion.identity, player);
                _spawnedCharacters[player] = obj;
                _spawnedCharacters[player].GetComponent<Animator>().SetBool("StartGame", true);
                Debug.Log($"Prefab do Level01 spawnado para {player}");
            }
        }

        //Configuração de UI do lobby continua igual.
        if (currentSceneRef == SceneRef.FromIndex(_lobbySceneIndex))
        {
            if (readyButton != null)
            {
                readyButton.onClick.RemoveAllListeners();
                readyButton.onClick.AddListener(IsplayerReadying);
            }
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        data.moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        data.moveInputLvl01 = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        data.jumpPressed = Input.GetKey(KeyCode.Space);
        data.attackPressed = Input.GetMouseButton(0);
        data.prepareArremessoPressed = Input.GetMouseButton(1);
        data.useItemPressed = Input.GetMouseButtonDown(0);
        data.aimActive = Input.GetMouseButtonDown(1);
        data.interactPressed = Input.GetKeyDown(KeyCode.E);
        data.stealthPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        data.dropItemPressed = Input.GetKeyDown(KeyCode.Q);
        data.arremessarPressed = Input.GetMouseButtonUp(1);

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { Debug.Log($"Runner Shutdown: {shutdownReason}"); }
    public void OnConnectedToServer(NetworkRunner runner) { Debug.Log("Connected to server."); }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { Debug.Log($"Disconnected from server: {reason}"); }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { Debug.LogError($"Connect failed: {reason}"); }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadStart(NetworkRunner runner) { Debug.Log($"OnSceneLoadStart: Carregando cena {GetSceneInfo()}..."); }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}

public struct NetworkInputData : INetworkInput
{
    public Vector2 moveInput;
    public Vector2 moveInputLvl01;
    public bool jumpPressed;
    public bool attackPressed;
    public bool aimActive;
    public bool interactPressed;
    public bool stealthPressed;
    public bool dropItemPressed;
    public bool useItemPressed;

    public bool prepareArremessoPressed;
    public bool arremessarPressed;
}
