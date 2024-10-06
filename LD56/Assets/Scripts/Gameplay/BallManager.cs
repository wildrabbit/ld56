﻿using System.Collections.Generic;
using UnityEngine;


public class BallsManager:MonoBehaviour
{
    public List<BallLogic> balls = new();

    public int TotalBalls => balls.Count;

    public void StartGame(List<BallLogic> initial)
    {
        DestroyPending();
        RegisterBalls(initial);        
    }

    private void RegisterBalls(IEnumerable<BallLogic> newBalls)
    {
        foreach(var ball in newBalls)
        {
            ball.Destroyed += OnBallDestroyed;
            ball.Split += OnBallSplit;
            ball.GeneratedBichi += OnBichiSpawned;

            balls.Add(ball);
            ball.Activate();
        }
    }

    public void OnBallDestroyed(BallLogic ball)
    {
        ball.Destroyed -= OnBallDestroyed;
        ball.Split -= OnBallSplit;
        ball.GeneratedBichi -= OnBichiSpawned;
        balls.Remove(ball);
    }

    private void OnBallSplit(BallLogic ball, BallLogic[] splitBalls)
    {
        ball.Kill();
        RegisterBalls(splitBalls);        
    }

    private void OnBichiSpawned(BallLogic ball, BichiLogic bichiLogic)
    {
        ball.Kill();
    }

    private void DestroyPending()
    {
        foreach (var ball in balls)
        {
            if (ball != null)
            {
                ball.Destroyed -= OnBallDestroyed;
                ball.Split -= OnBallSplit;
                ball.GeneratedBichi -= OnBichiSpawned;
                ball.Kill(notify: false);
            }
        }
        balls.Clear();
    }

    private void OnDestroy()
    {
        DestroyPending();
    }

    public void PopFirst()
    {
#if UNITY_EDITOR
        if(balls.Count > 0)
        {
            var ball = balls[0];
            ball.Pop(); // Eventually leave this to a system, rather than letting the ball do it
        }
#endif
    }

    public void ClearAll()
    {
        DestroyPending();
    }
}