using UnityEngine;
using System.Collections.Generic;
using System;

public class ThrowDebugLogger : MonoBehaviour
{
    public static ThrowDebugLogger Instance { get; private set; }
    
    [Header("Debug Settings")]
    public bool enableDetailedLogging = true;
    public bool enableBuildLogging = true;
    public bool showInGameUI = false;
    
    private List<string> debugMessages = new List<string>();
    private const int maxMessages = 50;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public static void LogThrow(string message, LogType logType = LogType.Log)
    {
        if (Instance == null) return;

        // No editor, sempre loga
        if (Application.isEditor && Instance.enableDetailedLogging)
        {
            Debug.Log($"[THROW_DEBUG] {message}");
        }
        // Na build, só loga se habilitado
        else if (!Application.isEditor && Instance.enableBuildLogging)
        {
            Debug.Log($"[THROW_DEBUG] {message}");
        }
        
        if (Instance.showInGameUI)
        {
            string timestampedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
            Instance.debugMessages.Add(timestampedMessage);
            
            if (Instance.debugMessages.Count > maxMessages)
            {
                Instance.debugMessages.RemoveAt(0);
            }
        }
    }
    
    public static void LogThrowError(string message)
    {
        LogThrow($"ERROR: {message}", LogType.Error);
    }
    
    public static void LogThrowWarning(string message)
    {
        LogThrow($"WARNING: {message}", LogType.Warning);
    }
    
    public static void LogInputState(string playerName, bool preparePressed, bool arremessarPressed, bool arremessando)
    {
        LogThrow($"INPUT [{playerName}] - Prepare: {preparePressed}, Arremessar: {arremessarPressed}, Arremessando: {arremessando}");
    }
    
    public static void LogNetworkEvent(string eventType, string details)
    {
        LogThrow($"NETWORK [{eventType}] - {details}");
    }
    
    public static void LogPhysicsEvent(string objectName, Vector3 position, Vector3 velocity)
    {
        LogThrow($"PHYSICS [{objectName}] - Pos: {position}, Vel: {velocity}");
    }
    
    void OnGUI()
    {
        if (!showInGameUI || debugMessages.Count == 0) return;
        
        // Cria uma caixa de debug no canto superior direito
        float boxWidth = 400f;
        float boxHeight = 300f;
        float margin = 10f;
        
        Rect boxRect = new Rect(
            Screen.width - boxWidth - margin,
            margin,
            boxWidth,
            boxHeight
        );
        
        GUI.Box(boxRect, "Throw Debug Log");
        
        // Área de scroll para as mensagens
        Rect scrollRect = new Rect(
            boxRect.x + 5,
            boxRect.y + 20,
            boxRect.width - 10,
            boxRect.height - 25
        );
        
        GUILayout.BeginArea(scrollRect);
        GUILayout.BeginVertical();
        
        // Mostra as mensagens mais recentes primeiro
        for (int i = debugMessages.Count - 1; i >= 0; i--)
        {
            GUILayout.Label(debugMessages[i], GUILayout.Width(boxWidth - 20));
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
        
        // Botão para limpar o log
        Rect clearButtonRect = new Rect(
            boxRect.x,
            boxRect.y + boxRect.height + 5,
            100,
            25
        );
        
        if (GUI.Button(clearButtonRect, "Clear Log"))
        {
            debugMessages.Clear();
        }
    }
    
    void Update()
    {
        // Atalho para ativar/desativar a UI de debug
        if (Input.GetKeyDown(KeyCode.F12))
        {
            showInGameUI = !showInGameUI;
        }
    }
}