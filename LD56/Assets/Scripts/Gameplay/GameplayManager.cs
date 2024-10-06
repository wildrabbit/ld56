using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] BallsManager ballsManager;
    [SerializeField] PlayerLogic player;

    bool testRestartPressed;
    bool testPopPressed;

    private void Awake()
    {
    }

    private void Start()
    {
        testRestartPressed = ReadRestart();
        testPopPressed = ReadPopTest();
        StartGame();
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

    private void StartGame()
    {
        var livingBalls = new List<BallLogic>(FindObjectsByType<BallLogic>(findObjectsInactive: FindObjectsInactive.Exclude, sortMode: FindObjectsSortMode.None));
        ballsManager.StartGame(livingBalls);
        player.StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        bool wasRestartPressed = testRestartPressed;
        testRestartPressed = ReadRestart();
        bool restartReleased = wasRestartPressed && !testRestartPressed;
        if (restartReleased)
        {
            SceneManager.LoadScene(0);
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


    }
}
