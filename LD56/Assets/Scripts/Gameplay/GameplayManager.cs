using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameResult
{
    None,
    Won,
    Lost
}

public enum WinCondition
{
    ZeroBalls,
    MinBichis
}

public enum LoseCondition
{
    PlayerDeath,
    LostBichis,
    Timeout,
}

public class GameplayManager : MonoBehaviour
{
    [SerializeField] BallsManager ballsManager;
    [SerializeField] BichisManager bichisManager;
    [SerializeField] PlayerLogic player;

    [SerializeField] WinCondition winCondition = WinCondition.ZeroBalls;
    [SerializeField] LoseCondition loseCondition = LoseCondition.PlayerDeath;
    [SerializeField] int bichisToSpawn = 4;
    [SerializeField] int winBichis = 2;
    [SerializeField] int masterBichis = 4;

    bool testRestartPressed;
    bool testPopPressed;
    GameResult gameResult;
    
    private bool inputReady;

    private void Awake()
    {
        gameResult = GameResult.None;
    }

    private void Start()
    {
        testRestartPressed = testPopPressed = false;
        StartGame();
    }

    private void StartGame()
    {
        inputReady = Keyboard.current != null || Gamepad.current != null;
        var livingBalls = new List<BallLogic>(FindObjectsByType<BallLogic>(findObjectsInactive: FindObjectsInactive.Exclude, sortMode: FindObjectsSortMode.None));
        ballsManager.StartGame(livingBalls);

        var livingBichis = new List<BichiLogic>(FindObjectsByType<BichiLogic>(findObjectsInactive: FindObjectsInactive.Exclude, sortMode: FindObjectsSortMode.None));
        bichisManager.StartGame(livingBichis, bichisToSpawn);
        
        gameResult = GameResult.None;
        
        player.StartGame();
        player.Died += OnPlayerDied;
    }

    private void OnPlayerDied(PlayerLogic logic)
    {
        SetResult(GameResult.Lost);
    }

    private bool ReadPopTest()
    {
        return (Keyboard.current != null && Keyboard.current.shiftKey.isPressed)
            || (Gamepad.current != null && Gamepad.current.selectButton.isPressed);
    }

    private bool ReadRestart()
    {
        return (Keyboard.current != null && Keyboard.current.escapeKey.isPressed)
            || (Gamepad.current != null && Gamepad.current.startButton.isPressed);
    }

    // Update is called once per frame
    void Update()
    {

#if UNITY_WEBGL
        if (!inputReady)
        {
            if (Keyboard.current != null || Gamepad.current != null)
            {
                inputReady = true;
            }
            return;
        }
#endif

        if (gameResult != GameResult.None)
        {
            if(ReadAnything())
            {
                Restart();
            }
            return;
        }

        bool wasRestartPressed = testRestartPressed;
        testRestartPressed = ReadRestart();
        bool restartReleased = wasRestartPressed && !testRestartPressed;
        if (restartReleased)
        {
            Restart();
            return;
        }

        bool wasPopPressed = testPopPressed;
        testPopPressed = ReadPopTest();
        bool popReleased = wasPopPressed && !testPopPressed;
        if (popReleased)
        {
            ballsManager.PopFirst();
            return;
        }

        if (ballsManager.TotalBalls == 0 && !player.Dead)
        {
            SetResult(GameResult.Won);
        }
    }

    private void SetResult(GameResult result)
    {
        gameResult = result;
        switch (gameResult)
        {
            case GameResult.None:
                break;
            case GameResult.Won:
                Debug.Log("<color=yellow>[RESULT]</color> WON! 😎");
                break;
            case GameResult.Lost:
                Debug.Log("<color=yellow>[RESULT]</color> LOST! 😭");
                break;
        }
        player.Deactivate();
        ballsManager.ClearAll();
    }

    private bool ReadAnything()
    {
        return (Keyboard.current != null && Keyboard.current.anyKey.isPressed)
            || (Gamepad.current != null && (Gamepad.current.aButton.isPressed
            || Gamepad.current.bButton.isPressed
            || Gamepad.current.xButton.isPressed
            || Gamepad.current.yButton.isPressed));
    }

    private void Restart()
    {
        SceneManager.LoadScene(0);
    }
}
