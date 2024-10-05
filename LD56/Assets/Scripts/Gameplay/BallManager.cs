using System.Collections.Generic;
using UnityEngine;


public class BallsManager:MonoBehaviour
{
    public List<BallLogic> balls = new();

    public void StartGame(List<BallLogic> initial)
    {
        DestroyPending();        
        foreach(var b in initial)
        {
            balls.Add(b);
            b.Activate();
        }
    }

    //public void SpawnWave(List<BallData> ballWaveData, bool additive = true)
    //{

    //}

    private void DestroyPending()
    {
        foreach(var ball in balls)
        {
            Destroy(ball.gameObject);
        }
        balls.Clear();
    }
}