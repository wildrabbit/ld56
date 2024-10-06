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

public class GameplayManager : MonoBehaviour
{
    [SerializeField] BallsManager ballsManager;
    [SerializeField] PlayerLogic player;

    bool testRestartPressed;
    bool testPopPressed;
    GameResult gameResult;

    private void Awake()
    {
        gameResult = GameResult.None;
    }

    private void Start()
    {
        testRestartPressed = ReadRestart();
        testPopPressed = ReadPopTest();
        StartGame();
    }

    private void StartGame()
    {
        var livingBalls = new List<BallLogic>(FindObjectsByType<BallLogic>(findObjectsInactive: FindObjectsInactive.Exclude, sortMode: FindObjectsSortMode.None));
        gameResult = GameResult.None;
        ballsManager.StartGame(livingBalls);
        player.StartGame();
        player.Died += OnPlayerDied;
    }

    private void OnPlayerDied(PlayerLogic logic)
    {
        SetResult(GameResult.Lost);
    }

    private bool ReadPopTest()
    {
        return Keyboard.current.shiftKey.isPressed ||
            Gamepad.current.selectButton.isPressed;
    }

    private bool ReadRestart()
    {
        return Keyboard.current.escapeKey.isPressed ||
            Gamepad.current.startButton.isPressed;
    }

    // Update is called once per frame
    void Update()
    {
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
    }

    private bool ReadAnything()
    {
        return Keyboard.current.anyKey.isPressed
            || Gamepad.current.aButton.isPressed
            || Gamepad.current.bButton.isPressed
            || Gamepad.current.xButton.isPressed
            || Gamepad.current.yButton.isPressed;
    }

    private void Restart()
    {
        SceneManager.LoadScene(0);
    }
}
