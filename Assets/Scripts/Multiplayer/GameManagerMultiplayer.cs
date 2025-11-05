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
using System.Collections;

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
    public Button viewButtons;
    public Button closeButtons;

    public GameObject roomNamePanel;

    public GameObject viewObjs;

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
    [SerializeField] private int _gameplayMultiplayerSceneIndex = 2;
    [SerializeField] private int _gameplaySingleSceneIndex = 3;

    [Header("Configurações de Spawn")]
    [SerializeField] private NetworkObject _playerPrefab;
    [SerializeField] private NetworkObject _playerPrefabLvl01;

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    public Dictionary<PlayerRef, bool> _playersReady = new Dictionary<PlayerRef, bool>();
    [Networked] public int ReadyCount { get; set; }

    private bool _isLoadingGameplayScene = false;
    private bool _hasSpawnedLobbyPlayers = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (GetSceneInfo() == SceneRef.FromIndex(0).ToString())
        {
            singlePlayerButton = GameObject.Find("SinglePlayer")?.GetComponent<Button>();
            multiplayerButton = GameObject.Find("MultiPlayer")?.GetComponent<Button>();
            configButton = GameObject.Find("Configuracoes")?.GetComponent<Button>();
            exitButton = GameObject.Find("Sair")?.GetComponent<Button>();
            roomNamePanel = GameObject.Find("RoomNameContainer");
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
            multiplayerButton.onClick.AddListener(() => StartCoroutine(OpenRoomNamePanel()));
        }
        if (roomNamePanel.transform.GetChild(1).GetComponent<Button>())
        {
            roomNamePanel.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => OnGameModeSelected(GameMode.AutoHostOrClient));
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
        if (roomNamePanel.transform.GetChild(0).GetComponent<TMP_InputField>() != null && !string.IsNullOrEmpty(roomNamePanel.transform.GetChild(0).GetComponent<TMP_InputField>().text))
        {
            _roomName = roomNamePanel.transform.GetChild(0).GetComponent<TMP_InputField>().text;
        }
        else
        {
            _roomName = "SalaPadrao";
        }

        if (Runner == null || !Runner.IsRunning)
        {
            await StartNetwork(selectedMode);
        }
        else
        {
            Debug.Log("Network já está rodando.");
        }
    }

    IEnumerator OpenRoomNamePanel()
    {
        if (roomNamePanel != null)
        {
            CanvasGroup canvasGroup = roomNamePanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                // Fade in
                float duration = 0.5f;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                    yield return null;
                }
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            else
            {
                Debug.LogWarning("CanvasGroup não encontrado em roomNamePanel.");
            }
        }
        else
        {
            Debug.LogWarning("roomNamePanel não atribuído.");
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
                    SceneManager = sceneManager,
                    PlayerCount = 4,
                    CustomLobbyName = "GlobalLobby",
                    Address = NetAddress.Any(),
                    SessionProperties = new Dictionary<string, SessionProperty>()
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
                SceneManager = sceneManager,
                PlayerCount = 4,
                CustomLobbyName = "GlobalLobby",
                Address = NetAddress.Any(),
                SessionProperties = new Dictionary<string, SessionProperty>()
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
                viewButtons = GameObject.FindGameObjectWithTag("ViewButtons").GetComponent<Button>();
                closeButtons = GameObject.FindGameObjectWithTag("CloseButtons").GetComponent<Button>();
            }
            else
            {
                readyButton = GameObject.FindGameObjectWithTag("BtnReady").GetComponent<Button>();
                viewButtons = GameObject.FindGameObjectWithTag("ViewButtons").GetComponent<Button>();
                closeButtons = GameObject.FindGameObjectWithTag("CloseButtons").GetComponent<Button>();
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner == null) return;

        if (Runner.IsServer)
        {
            if (GetSceneInfo() == SceneRef.FromIndex(1).ToString())
            {
                roomCountPlayers = GameObject.Find("TextoConnect")?.GetComponent<TextMeshProUGUI>();
                levelLoader = FindFirstObjectByType<LevelLoaderGame>();

                if (roomCountPlayers != null)
                {
                    roomCountPlayers.text = $"{Runner.ActivePlayers.Count()} / 4";
                }

                SetupLobbyButtons();

                int currentReadyCount = _playersReady.Count(kvp => kvp.Value);
                if (currentReadyCount != ReadyCount)
                {
                    ReadyCount = currentReadyCount;
                }
            }
        }
    }

    private void SetupLobbyButtons()
    {
        if (readyButton == null)
        {
            RequestButtonReady();
        }

        if (readyButton != null)
        {
            readyButton.onClick.RemoveAllListeners();
            readyButton.onClick.AddListener(IsplayerReadying);
        }

        if (viewButtons != null)
        {
            viewButtons.onClick.RemoveAllListeners();
            viewButtons.onClick.AddListener(() => StartCoroutine(ViewButtonsInputs()));
        }

        if (closeButtons != null)
        {
            closeButtons.onClick.RemoveAllListeners();
            closeButtons.onClick.AddListener(() => StartCoroutine(ViewButtonsInputsClose()));
        }
    }

    public void IsplayerReadying()
    {
        if (Runner == null) return;

        var localPlayer = Runner.LocalPlayer;
        isPlayerReadying = true;

        Debug.Log($"🎯 Jogador {localPlayer} está pronto.");

        if (Runner.IsServer)
        {
            // Servidor: conta diretamente
            if (_playersReady.ContainsKey(localPlayer))
            {
                _playersReady[localPlayer] = true;
            }
            else
            {
                _playersReady.Add(localPlayer, true);
            }

            ReadyCount = _playersReady.Count(kvp => kvp.Value);
            CheckIfCanStartGame();
        }
        else
        {
            NotifyServerThroughPlayer();
        }
    }

    private void NotifyServerThroughPlayer()
    {
        var characters = FindObjectsByType<CharacterMultiplayer>(FindObjectsSortMode.None);
        foreach (var character in characters)
        {
            if (character.Object.HasInputAuthority)
            {
                character.RPC_NotifyReady();
                break;
            }
        }
    }

    private void CheckIfCanStartGame()
    {
        if (_isLoadingGameplayScene) return;

        int totalPlayers = Runner.ActivePlayers.Count();
        Debug.Log($"📊 Verificando início: {ReadyCount}/{totalPlayers} prontos");

        if (ReadyCount >= 1 && ReadyCount == totalPlayers)
        {
            _isLoadingGameplayScene = true;

            if (totalPlayers == 1)
            {
                Debug.Log("🚀 Iniciando single player...");
                levelLoader.Transicao(SceneRef.FromIndex(_gameplaySingleSceneIndex), Runner);
            }
            else
            {
                Debug.Log("🚀 Iniciando multiplayer...");
                levelLoader.Transicao(SceneRef.FromIndex(_gameplayMultiplayerSceneIndex), Runner);
            }
        }
    }

    public IEnumerator ViewButtonsInputs()
    {
        if (viewObjs != null)
        {
            CanvasGroup canvasGroup = viewObjs.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                // Fade in
                float duration = 0.5f;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                    yield return null;
                }
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            else
            {
                Debug.LogWarning("CanvasGroup não encontrado em ViewObjs.");
            }
        }
        else
        {
            Debug.LogWarning("ViewObjs não atribuído.");
        }
    }

    public IEnumerator ViewButtonsInputsClose()
    {
        if (viewObjs != null)
        {
            CanvasGroup canvasGroup = viewObjs.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                // Fade in
                float duration = 0.5f;
                float elapsed = 1f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                    yield return null;
                }
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            else
            {
                Debug.LogWarning("CanvasGroup não encontrado em ViewObjs.");
            }
        }
        else
        {
            Debug.LogWarning("ViewObjs não atribuído.");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"=== ON PLAYER JOINED === Player: {player}, IsServer: {runner.IsServer}, Scene: {GetSceneInfo()}");

        Scene currentScene = SceneManager.GetActiveScene();
        var currentSceneRef = SceneRef.FromIndex(currentScene.buildIndex);

        if (currentSceneRef == SceneRef.FromIndex(_lobbySceneIndex))
        {
            if (runner.IsServer)
            {
                Debug.Log($"🎮 Servidor spawnando jogador {player} no lobby");
                SpawnPlayerInLobby(runner, player);
            }
        }
    }

    private void SpawnPlayerInLobby(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.ContainsKey(player))
        {
            Debug.LogWarning($"⚠️ Jogador {player} já está spawnado. Ignorando.");
            return;
        }

        if (_playerPrefab == null)
        {
            Debug.LogError("❌ PlayerPrefab não atribuído!");
            return;
        }

        Vector3 spawnPosition = GetSpawnPositionForPlayer(_spawnedCharacters.Count);
        Debug.Log($"📍 Spawnando {player} em {spawnPosition}");

        NetworkObject networkPlayerObject = runner.Spawn(
            _playerPrefab,
            spawnPosition,
            Quaternion.identity,
            inputAuthority: player
        );

        if (networkPlayerObject != null)
        {
            _spawnedCharacters.Add(player, networkPlayerObject);
            _playersReady.Add(player, false); // Inicialmente não pronto

            Debug.Log($"✅ {player} spawnado: {networkPlayerObject.name}");
            Debug.Log($"   InputAuthority: {networkPlayerObject.InputAuthority}, Position: {networkPlayerObject.transform.position}");
        }
        else
        {
            Debug.LogError($"❌ Falha ao spawnar {player}");
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Jogador {player} saiu.");

        // CORREÇÃO: Usar Dictionary normal
        if (_spawnedCharacters.ContainsKey(player))
        {
            NetworkObject networkObject = _spawnedCharacters[player];
            if (networkObject != null)
            {
                runner.Despawn(networkObject);
            }
            _spawnedCharacters.Remove(player);
        }

        if (_playersReady.ContainsKey(player))
        {
            _playersReady.Remove(player);
        }

        if (runner.IsServer)
        {
            ReadyCount = _playersReady.Count(kvp => kvp.Value);
        }
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        var currentSceneRef = SceneRef.FromIndex(currentScene.buildIndex);
        Debug.Log($"OnSceneLoadDone: Cena {currentSceneRef}. IsServer: {runner.IsServer}");

        // CORREÇÃO: Resetar flag ao mudar de cena
        _hasSpawnedLobbyPlayers = false;

        if (currentSceneRef == SceneRef.FromIndex(_lobbySceneIndex))
        {
            Debug.Log("🏠 Cena do lobby carregada - spawnando jogadores...");

            if (runner.IsServer)
            {
                foreach (var player in runner.ActivePlayers)
                {
                    if (!_spawnedCharacters.ContainsKey(player))
                    {
                        SpawnPlayerInLobby(runner, player);
                    }
                }
                _hasSpawnedLobbyPlayers = true;
            }

            runner.StartCoroutine(WaitAndSetupLobbyUI());
        }
        else if (currentSceneRef == SceneRef.FromIndex(_gameplayMultiplayerSceneIndex))
        {
            _spawnedCharacters.Clear();

            if (runner.IsServer)
            {
                SpawnPlayersInGameplay(runner);
            }
        }
        else if (currentSceneRef == SceneRef.FromIndex(_gameplaySingleSceneIndex))
        {
            _spawnedCharacters.Clear();
            if (runner.IsServer)
            {
                SpawnSinglePlayer(runner);
            }
        }
    }

    private void SpawnPlayersInGameplay(NetworkRunner runner)
    {
        Debug.Log("Spawnando jogadores na gameplay multiplayer...");

        List<Vector3> spawnPoints = new List<Vector3>()
        {
            new Vector3(-16.4f, 1f, 8.04f),
            new Vector3(-0.925f, 1f, 21.662f),
            new Vector3(13.03f, 1f, 18f),
            new Vector3(16.38f, 1f, 5.9f)
        };

        // Embaralha spawn points
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            int rand = UnityEngine.Random.Range(i, spawnPoints.Count);
            (spawnPoints[i], spawnPoints[rand]) = (spawnPoints[rand], spawnPoints[i]);
        }

        int index = 0;
        foreach (var player in runner.ActivePlayers)
        {
            if (index >= spawnPoints.Count) break;

            Vector3 spawnPos = spawnPoints[index];
            var obj = runner.Spawn(_playerPrefabLvl01, spawnPos, Quaternion.identity, player);
            _spawnedCharacters.Add(player, obj);
            Debug.Log($"🎮 Jogador {player} spawnado na gameplay em {spawnPos}");
            index++;
        }
    }

    private void SpawnSinglePlayer(NetworkRunner runner)
    {
        Debug.Log("Spawnando jogador single player...");
        Vector3 spawnPos = new Vector3(-16.4f, 1f, 8.04f);
        var player = runner.ActivePlayers.First();
        var obj = runner.Spawn(_playerPrefabLvl01, spawnPos, Quaternion.identity, player);
        _spawnedCharacters.Add(player, obj);
        Debug.Log($"🎮 Jogador single spawnado em {spawnPos}");
    }

    private Vector3 GetSpawnPositionForPlayer(int playerIndex)
    {
        Vector3[] spawnPoints = new Vector3[]
        {
            new Vector3(-2f, 1f, -8f),
            new Vector3(2f, 1f, -8f),
            new Vector3(-4f, 1f, -6f),
            new Vector3(4f, 1f, -6f)
        };

        if (playerIndex >= 0 && playerIndex < spawnPoints.Length)
        {
            return spawnPoints[playerIndex];
        }

        // Fallback
        return new Vector3(0f, 1f, -8f);
    }

    private IEnumerator WaitAndSetupLobbyUI()
    {
        yield return new WaitForSeconds(0.5f);

        Debug.Log("Configurando UI do lobby...");
        RequestButtonReady();

        GameObject obj = GameObject.Find("ViewObjs");
        if (obj != null)
        {
            viewObjs = obj;
            var canvasGroup = obj.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        if (Runner != null)
        {
            roomCountPlayers = GameObject.Find("TextoConnect")?.GetComponent<TextMeshProUGUI>();
            if (roomCountPlayers != null)
            {
                roomCountPlayers.text = $"{Runner.ActivePlayers.Count()} / 4";
            }
        }

        // DEBUG: Verifica se os players estão spawnados corretamente
        Debug.Log("=== DEBUG SPAWN STATUS ===");
        foreach (var player in Runner.ActivePlayers)
        {
            bool hasSpawned = _spawnedCharacters.ContainsKey(player);
            NetworkObject playerObj = hasSpawned ? _spawnedCharacters[player] : null;
            Debug.Log($"Player {player}: Spawned={hasSpawned}, Object={(playerObj != null ? playerObj.name : "NULL")}, InputAuthority={(playerObj != null ? playerObj.InputAuthority : "N/A")}");
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
        data.runningPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        data.dancinHipHop = Input.GetKey(KeyCode.Alpha1);
        data.dancinSalsa = Input.GetKey(KeyCode.Alpha2);
        data.dancinSwing = Input.GetKey(KeyCode.Alpha3);

        // o ESC do teclado.
        data.SceneSensiPressed = Input.GetKeyDown(KeyCode.Escape);

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
    public bool runningPressed;
    public bool jumpPressed;
    public bool attackPressed;
    public bool aimActive;
    public bool interactPressed;
    public bool stealthPressed;
    public bool dropItemPressed;
    public bool useItemPressed;

    public bool prepareArremessoPressed;
    public bool arremessarPressed;
    public bool SceneSensiPressed;

    public bool dancinHipHop;
    public bool dancinSalsa;
    public bool dancinSwing;
}
