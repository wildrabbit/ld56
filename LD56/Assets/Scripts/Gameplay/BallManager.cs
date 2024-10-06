using System;
using System.Collections.Generic;
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

            balls.Add(ball);
            ball.Activate();
        }
    }

    public void OnBallDestroyed(BallLogic ball)
    {
        ball.Destroyed -= OnBallDestroyed;
        ball.Split -= OnBallSplit;
        balls.Remove(ball);
    }

    private void OnBallSplit(BallLogic ball, BallLogic[] splitBalls)
    {
        ball.Kill();
        RegisterBalls(splitBalls);        
    }

    private void DestroyPending()
    {
        foreach(var ball in balls)
        {
            ball.Kill();
        }
    }

    private void OnDestroy()
    {
        foreach (var ball in balls)
        {
            if(ball != null)
            {
                ball.Destroyed -= OnBallDestroyed;
                ball.Split -= OnBallSplit;            
                ball.Kill(notify:false);
            }
        }
        balls.Clear();
    }

    public void PopFirst()
    {
        if(balls.Count > 0)
        {
            var ball = balls[0];
            ball.Pop(); // Eventually leave this to a system, rather than letting the ball do it
        }
    }
}