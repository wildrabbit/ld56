using System;
using System.Collections.Generic;
using UnityEngine;


public class BallsManager:MonoBehaviour
{
    public AudioSource popSrc;
    public List<BallLogic> balls = new();

    public int TotalBalls => balls.Count;

    public event Action<BallLogic, bool> BallPopped;
    public event Action<BallLogic> BallSpawned;

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
            ball.Popped += OnBallPopped;
            //ball.GeneratedBichi += OnBichiSpawned;

            balls.Add(ball);
            ball.Activate();
            BallSpawned?.Invoke(ball);
}
    }

    private void OnBallPopped(BallLogic logic, bool propagate)
    {
        if (popSrc != null)
        {
            popSrc.Play();
        }
        BallPopped?.Invoke(logic, propagate);
    }

    public void OnBallDestroyed(BallLogic ball)
    {
        ball.Destroyed -= OnBallDestroyed;
        ball.Split -= OnBallSplit;
        ball.Popped -= OnBallPopped;
        //ball.GeneratedBichi -= OnBichiSpawned;
        balls.Remove(ball);
    }

    private void OnBallSplit(BallLogic ball, BallLogic[] splitBalls)
    {
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
                ball.Popped -= OnBallPopped;
                //ball.GeneratedBichi -= OnBichiSpawned;
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