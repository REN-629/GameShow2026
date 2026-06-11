using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get; private set; }

    [Header("ゲームモード")]
    public GameMode currentMode = GameMode.MainRun;

    [Header("MainRun")]
    public bool mainRunUsesRoomGeneration = true;
    public bool mainRunUsesTimers = true;
    public bool mainRunUsesItemPhase = true;
    public bool mainRunUsesRandomPuzzles = true;
    public bool mainRunRecordsRoomProgress = true;

    [Header("Tutorial")]
    public bool tutorialUsesRoomGeneration = false;
    public bool tutorialUsesTimers = false;
    public bool tutorialUsesItemPhase = false;
    public bool tutorialUsesRandomPuzzles = false;
    public bool tutorialRecordsRoomProgress = true;

    [Header("Test")]
    public bool testUsesRoomGeneration = false;
    public bool testUsesTimers = false;
    public bool testUsesItemPhase = false;
    public bool testUsesRandomPuzzles = false;
    public bool testRecordsRoomProgress = false;

    void Awake()
    {
        Instance = this;
    }

    public static GameMode CurrentMode
    {
        get
        {
            if (Instance == null)
                return GameMode.MainRun;

            return Instance.currentMode;
        }
    }

    public static bool IsMainRun => CurrentMode == GameMode.MainRun;
    public static bool IsTutorial => CurrentMode == GameMode.Tutorial;
    public static bool IsTest => CurrentMode == GameMode.Test;

    public static bool UsesRoomGeneration
    {
        get
        {
            if (Instance == null)
                return true;

            switch (Instance.currentMode)
            {
                case GameMode.MainRun:
                    return Instance.mainRunUsesRoomGeneration;
                case GameMode.Tutorial:
                    return Instance.tutorialUsesRoomGeneration;
                case GameMode.Test:
                    return Instance.testUsesRoomGeneration;
            }

            return true;
        }
    }

    public static bool UsesTimers
    {
        get
        {
            if (Instance == null)
                return true;

            switch (Instance.currentMode)
            {
                case GameMode.MainRun:
                    return Instance.mainRunUsesTimers;
                case GameMode.Tutorial:
                    return Instance.tutorialUsesTimers;
                case GameMode.Test:
                    return Instance.testUsesTimers;
            }

            return true;
        }
    }

    public static bool UsesItemPhase
    {
        get
        {
            if (Instance == null)
                return true;

            switch (Instance.currentMode)
            {
                case GameMode.MainRun:
                    return Instance.mainRunUsesItemPhase;
                case GameMode.Tutorial:
                    return Instance.tutorialUsesItemPhase;
                case GameMode.Test:
                    return Instance.testUsesItemPhase;
            }

            return true;
        }
    }

    public static bool UsesRandomPuzzles
    {
        get
        {
            if (Instance == null)
                return true;

            switch (Instance.currentMode)
            {
                case GameMode.MainRun:
                    return Instance.mainRunUsesRandomPuzzles;
                case GameMode.Tutorial:
                    return Instance.tutorialUsesRandomPuzzles;
                case GameMode.Test:
                    return Instance.testUsesRandomPuzzles;
            }

            return true;
        }
    }

    public static bool RecordsRoomProgress
    {
        get
        {
            if (Instance == null)
                return true;

            switch (Instance.currentMode)
            {
                case GameMode.MainRun:
                    return Instance.mainRunRecordsRoomProgress;
                case GameMode.Tutorial:
                    return Instance.tutorialRecordsRoomProgress;
                case GameMode.Test:
                    return Instance.testRecordsRoomProgress;
            }

            return true;
        }
    }
}
