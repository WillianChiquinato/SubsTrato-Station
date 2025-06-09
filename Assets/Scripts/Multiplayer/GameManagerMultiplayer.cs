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
    public bool isPlayerReadying = false;
    public Button singlePlayerButton;
    public Button multiplayerButton;
    public Button configButton;
    public Button exitButton;
    public Button SinglePlayer;

    public TextMeshProUGUI roomCountPlayers;
    public Button readyButton;

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
    [SerializeField] private NetworkObject _playerPrefab;
    [SerializeField] private int _minPlayersToStartGame = 1;

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    private bool _isLoadingGameplayScene = false;

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
        // Se este GameObject com PlayersSpawn deve persistir e iniciar a conexão
        // Idealmente, este script está na Cena 0 e inicia a conexão.
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

        // Log
        Debug.Log($"Iniciando jogo no modo: {mode} com a sala: {_roomName}");

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = _roomName,
            Scene = SceneRef.FromIndex(_lobbySceneIndex),
            PlayerCount = 4,
            SceneManager = sceneManager
        });

        if (!result.Ok)
        {
            Debug.LogError($"Falha ao iniciar o jogo: {result.ShutdownReason} - {result.ErrorMessage}");
        }
        else
        {
            Debug.Log("StartGame iniciado com sucesso!");
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner == null) return;
        if (!Runner.IsServer) return;

        if (GetSceneInfo() == SceneRef.FromIndex(1).ToString())
        {
            roomCountPlayers = GameObject.Find("TextoConnect")?.GetComponent<TextMeshProUGUI>();
            readyButton = GameObject.Find("readyButton")?.GetComponent<Button>();
        }

        if (Runner.IsSceneAuthority)
        {
            roomCountPlayers.text = $"{Runner.ActivePlayers.Count()} / 4";

            Scene currentScene = SceneManager.GetActiveScene();
            SceneRef currentSceneRef = SceneRef.FromIndex(currentScene.buildIndex);

            if (currentSceneRef == SceneRef.FromIndex(_lobbySceneIndex) && !_isLoadingGameplayScene)
            {
                if (readyButton != null)
                {
                    readyButton.onClick.AddListener(IsplayerReadying);
                }
                else
                {
                    Debug.LogWarning("ReadyButton não encontrado na cena.");
                }
                int playerCount = Runner.ActivePlayers.Count();
                if (playerCount >= _minPlayersToStartGame)
                {
                    Debug.Log($"Jogadores mínimos ({_minPlayersToStartGame}) alcançados. Carregando cena de gameplay ({_gameplaySceneIndex}).");
                    _isLoadingGameplayScene = true;
                    if (isPlayerReadying)
                    {
                        Runner.LoadScene(SceneRef.FromIndex(_gameplaySceneIndex));
                    }
                }
            }
        }
    }

    public void IsplayerReadying()
    {
        isPlayerReadying = !isPlayerReadying;
        Debug.Log($"Jogador está {(isPlayerReadying ? "pronto" : "não pronto")}. Cena atual: {GetSceneInfo()}");

        Button readyButton = GameObject.Find("ReadyButton")?.GetComponent<Button>();
        if (readyButton != null)
        {
            readyButton.interactable = false; // Desabilita o botão após clicar
        }
    }

    // --- Implementação de INetworkRunnerCallbacks ---
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Jogador {player} entrou. Cena atual: {GetSceneInfo()}");
        if (runner.IsServer)
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
        SceneRef currentSceneRef = SceneRef.FromIndex(currentScene.buildIndex);
        Debug.Log($"OnSceneLoadDone: Cena {GetSceneInfo()} carregada.");

        // Reseta a flag se a cena de gameplay carregou, ou se voltamos ao lobby.
        if (currentSceneRef == SceneRef.FromIndex(_gameplaySceneIndex))
        {
            _isLoadingGameplayScene = false;
            // Aqui você pode querer reposicionar os jogadores ou fazer setup específico da cena de gameplay
            // Você pode iterar por eles e ajustar suas posições, se necessário.
            Debug.Log("Cena de Gameplay carregada!");
        }
        else if (currentSceneRef == SceneRef.FromIndex(_lobbySceneIndex))
        {
            _isLoadingGameplayScene = false; // Estar no lobby significa que não estamos carregando o gameplay
            Debug.Log("Cena de Lobby carregada!");
        }
        // Se os jogadores foram spawnados ANTES desta cena carregar (ex: se este é o primeiro OnSceneLoadDone para o lobby)
        // e você precisa fazer algo com eles após a cena estar pronta, pode fazer aqui.
        // Mas OnPlayerJoined já deve ter cuidado do spawn.
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }
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
