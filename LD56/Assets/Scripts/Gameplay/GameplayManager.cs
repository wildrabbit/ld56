﻿using System;
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
    [SerializeField] RumbleUtils rumbleUtils;
    [SerializeField] PlayerLogic player;
    [SerializeField] HUD hud;

    // Move to LevelData SO
    [SerializeField] string levelDisplayName;
    [SerializeField] WinCondition winCondition = WinCondition.ZeroBalls;
    [SerializeField] LoseCondition loseCondition = LoseCondition.PlayerDeath;
    [SerializeField] int bichisToSpawn = 4;
    [SerializeField] int winBichis = 2;
    [SerializeField] int loseBichis = 3;
    [SerializeField] int masterBichis = 4;
    [SerializeField] float timeoutSecs;
    [SerializeField] float masterTimeSecs;
    [SerializeField] float gameOverDelay = 2f;

    [SerializeField] SceneReference nextScene;

    bool testRestartPressed;
    bool testDamagePressed;
    bool testPopPressed;
    GameResult gameResult;
    
    float elapsed;
    int bichisDead;
    int lastSecond;
    float gameOverElapsed;
    
    private bool inputReady;

    private void Awake()
    {
        gameResult = GameResult.None;
    }

    private void Start()
    {
        testRestartPressed = testPopPressed = false;
        testDamagePressed = false;
        StartGame();
    }

    private void StartGame()
    {
        inputReady = Keyboard.current != null || Gamepad.current != null;
        var livingBalls = new List<BallLogic>(FindObjectsByType<BallLogic>(findObjectsInactive: FindObjectsInactive.Exclude, sortMode: FindObjectsSortMode.None));
        ballsManager.StartGame(livingBalls);

        var livingBichis = new List<BichiLogic>(FindObjectsByType<BichiLogic>(findObjectsInactive: FindObjectsInactive.Exclude, sortMode: FindObjectsSortMode.None));
        bichisManager.StartGame(livingBichis, bichisToSpawn);
        bichisManager.BichiDied += OnBichiDied;
        bichisDead = 0;

        gameResult = GameResult.None;
        elapsed = 0f;
        lastSecond = 0;
        gameOverElapsed = -1f;
        
        player.StartGame();
        player.Died += OnPlayerDied;
        player.TookHit += OnPlayerTookDamage;
        player.LostAllHealth += OnPlayerLostHP;

        hud.StartGame(player.HP, BuildRescueStatuses(), levelDisplayName, timeoutSecs - elapsed);
    }

    private List<RescueStatus> BuildRescueStatuses()
    {
        var statuses = new List<RescueStatus>();
        int numLost = bichisDead;
        int numRescued = player.BichisRescued;
        int numActive =
            ballsManager.balls.FindAll(x => x.bichiInside).Count
            + bichisManager.AliveBichis;
        int numPending = bichisToSpawn - numLost - numRescued - numActive;
        for(int i = 0; i < numRescued; ++i)
        {
            statuses.Add(RescueStatus.Rescued);
        }
        for (int i = 0; i < numActive; ++i)
        {
            statuses.Add(RescueStatus.Active);
        }
        for (int i = 0; i < numPending; ++i)
        {
            statuses.Add(RescueStatus.Pending);
        }
        for (int i = 0; i < numLost; ++i)
        {
            statuses.Add(RescueStatus.Dead);
        }
        return statuses;
    }

    private void OnPlayerLostHP(PlayerLogic logic)
    {
        hud.SetHP(logic.HP);
        rumbleUtils.PlayStronk();
    }

    private void OnPlayerTookDamage(PlayerLogic logic, int hp)
    {
        hud.SetHP(logic.HP);
        rumbleUtils.PlaySmol();
    }

    private void OnBichiDied(BichiLogic logic)
    {
        rumbleUtils.PlaySmol();
        bichisDead++;
        if(loseCondition == LoseCondition.LostBichis && bichisDead >= loseBichis)
        {
            SetResult(GameResult.Lost);
        }
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
        var dt = Time.deltaTime;
        if (gameResult != GameResult.None)
        {
            if(gameOverElapsed < gameOverDelay)
            {
                gameOverElapsed += dt;
                return;
            }
            if (ReadAnything())
            {
                if(gameResult == GameResult.Won)
                {
                    if (!string.IsNullOrEmpty(nextScene.ScenePath))
                    {
                        NextLevel();
                    }
                    else PlayLevel(0);
                }
                else
                {
                    Restart();
                }
            }
            return;
        }

        elapsed += dt;
        int secsElapsed = Mathf.FloorToInt(elapsed);
        if(secsElapsed != lastSecond)
        {
            lastSecond = secsElapsed;
            Debug.Log($"T:{lastSecond}s");
        }

        float remaining = Mathf.Max(timeoutSecs - elapsed, 0f);
        hud.SetTimer(remaining);

        if(elapsed > timeoutSecs)
        {
            SetResult(GameResult.Lost);
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

        bool wasDamagePressed = testDamagePressed;
        testDamagePressed = ReadDamageTest();
        bool dmgReleased = wasDamagePressed && !testDamagePressed;
        if ((dmgReleased))
        {
            player.Hit(1);
        }

        hud.SetHP(player.HP);
        hud.UpdateRescuePanel(BuildRescueStatuses());

        EvaluateVictory();
    }

    private void NextLevel()
    {
        PlayLevel(nextScene.ScenePath);
    }

    private void PlayLevel(int idx)
    {
        rumbleUtils.ForceStop();
        SceneManager.LoadScene(idx);
    }


    private void PlayLevel(string path)
    {
        rumbleUtils.ForceStop();
        SceneManager.LoadScene(path);
    }

    private bool ReadDamageTest()
    {
        return (Keyboard.current != null && Keyboard.current.numpadMinusKey.isPressed)
    || (Gamepad.current != null && Gamepad.current.leftShoulder.isPressed);
    }

    private void EvaluateVictory()
    {
        if (winCondition == WinCondition.ZeroBalls)
        {
            if (ballsManager.TotalBalls == 0 && !player.Dead)
            {
                bool mastered = elapsed <= masterTimeSecs;
                SetResult(GameResult.Won, mastered);
            }
        }
        else if(winCondition == WinCondition.MinBichis)
        {
            int rescued = player.BichisRescued;
            if(rescued >= winBichis)
            {
                bool mastered = rescued > masterBichis;
                SetResult(GameResult.Won, mastered);
            }
        }
    }

    private void SetResult(GameResult result, bool mastered = false)
    {
        gameResult = result;
        switch (gameResult)
        {
            case GameResult.None:
                return;
            case GameResult.Won:
                player.PlayWin();
                string masteryString = (mastered ? "🌟" : "");
                Debug.Log($"<color=yellow>[RESULT]</color> WON! {masteryString}😎{masteryString}");
                break;
            case GameResult.Lost:
                Debug.Log("<color=yellow>[RESULT]</color> LOST! 😭");
                break;
        }
        player.Deactivate();
        ballsManager.ClearAll();
        bichisManager.ClearAll();
        gameOverElapsed = 0f;
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
        PlayLevel(SceneManager.GetActiveScene().buildIndex);
    }
}
