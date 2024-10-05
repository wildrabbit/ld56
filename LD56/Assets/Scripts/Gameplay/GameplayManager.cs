using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] BallsManager ballsManager;

    bool testRestartPressed;
    bool testPopPressed;

    private void Awake()
    {
    }

    private void Start()
    {
        testRestartPressed = Keyboard.current.spaceKey.isPressed;
        testPopPressed = Keyboard.current.zKey.isPressed;
        StartGame();
    }

    private void StartGame()
    {
        var livingBalls = new List<BallLogic>(FindObjectsByType<BallLogic>(findObjectsInactive: FindObjectsInactive.Exclude, sortMode: FindObjectsSortMode.None));
        ballsManager.StartGame(livingBalls);
    }

    // Update is called once per frame
    void Update()
    {
        bool wasRestartPressed = testRestartPressed;
        testRestartPressed = Keyboard.current.spaceKey.isPressed;
        bool restartReleased = wasRestartPressed && !testRestartPressed;
        if (restartReleased)
        {
            SceneManager.LoadScene(0);
            return;
        }

        bool wasPopPressed = testPopPressed;
        testPopPressed = Keyboard.current.zKey.isPressed;
        bool popReleased = wasPopPressed && !testPopPressed;
        if (popReleased)
        {
            ballsManager.PopFirst();
            return;
        }


    }
}
