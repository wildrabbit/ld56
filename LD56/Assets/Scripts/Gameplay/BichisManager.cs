using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BichisManager: MonoBehaviour
{
    [SerializeField] BallsManager ballsManager;
    [SerializeField] BichiLogic[] bichiPrefabs;
    [SerializeField] Transform bichisRoot;
    [SerializeField] Transform bichisSpawnRef;

    public event Action<BichiLogic> GeneratedBichi;
    public event Action<BichiLogic> BichiDied;

    public int AliveBichis => livingBichis.Count;

    List<BichiLogic> livingBichis = new();

    List<int> ballPopsSpawningBichis = new();
    int spawned = 0;
    int nextIndex;
    private int bichisToSpawn;

    public void StartGame(List<BichiLogic> startBichis, int bichisToSpawn)
    {
        ClearAll();
        foreach(var b in startBichis)
        {
            b.transform.SetParent(bichisRoot, worldPositionStays: true);
            b.Dead += OnBichiDied;
            b.Destroyed += OnBichiDestroyed;
            livingBichis.Add(b);
        }

        var startBalls = ballsManager.balls;
        this.bichisToSpawn = bichisToSpawn;

        ballPopsSpawningBichis.Clear();
        nextIndex = 0;
        int totalBalls = 0;
        foreach (var ball in startBalls)
        {
            totalBalls += ball.NumSplitBallLeaves;
        }
        var range = new List<int>(Enumerable.Range(1, totalBalls));
        for(int i = 0; i < bichisToSpawn; ++i)
        {
            var index = UnityEngine.Random.Range(0, range.Count);
            ballPopsSpawningBichis.Add(range[index]);
            range.RemoveAt(index);
        }
        ballPopsSpawningBichis.Sort();
        var spawningString = string.Join(',', ballPopsSpawningBichis);
        Debug.LogFormat($"<color=#ff8000>[BICHIS]</color>Expected: [{spawningString}]");

        foreach(var b in ballsManager.balls)
        {
            b.SetBichiInside(false);
            if (b.tier == 1)
            {
                spawned++;
                if (nextIndex < ballPopsSpawningBichis.Count &&
                    spawned == ballPopsSpawningBichis[nextIndex])
                {
                    b.SetBichiInside(true);
                    nextIndex++;
                }
            }
        }

        ballsManager.BallPopped -= OnBallPopped;
        ballsManager.BallPopped += OnBallPopped;
        ballsManager.BallSpawned -= OnBallSpawned;
        ballsManager.BallSpawned += OnBallSpawned;
    }

    private void OnBichiDied(BichiLogic logic)
    {
        BichiDied?.Invoke(logic);
    }

    private void OnBallSpawned(BallLogic logic)
    {
        if(logic.tier == 1)
        {
            spawned++;
            if(nextIndex < ballPopsSpawningBichis.Count
                && ballPopsSpawningBichis[nextIndex] == spawned)
            {
                logic.SetBichiInside(true);
                nextIndex++;
            }
        }
    }

    private void OnBichiDestroyed(BichiLogic logic)
    {
        logic.Destroyed -= OnBichiDestroyed;
        logic.Dead -= OnBichiDied;
        livingBichis.Remove(logic);
    }

    public void SpawnBichi(BallLogic ball, BichiLogic bichiPrefab)
    {
        var bichi = Instantiate<BichiLogic>(bichiPrefab, bichisRoot);
        livingBichis.Add(bichi);
        GeneratedBichi?.Invoke(bichi);

        var pos = ball.transform.position;
        bichi.transform.position = pos;
        bichi.Deactivate();
        var localScale = bichi.transform.localScale;
        bichi.transform.localScale = localScale * 0.5f;
        float delta = bichisSpawnRef.position.y - pos.y;
        float duration = delta / 10f;
        bichi.transform.DOMoveY(bichisSpawnRef.position.y, duration).SetEase(Ease.OutQuart).OnComplete(() =>
        {
            bichi.transform.localScale = localScale;
            bichi.Activate();
            bichi.Destroyed += OnBichiDestroyed;
            bichi.Dead += OnBichiDied;
        });
    }


    private void OnBallPopped(BallLogic logic, bool propagate)
    {
        if(logic.tier != 1)
        {
            return;
        }

        if (logic.bichiInside)
        {
            int randomPrefabIndex = UnityEngine.Random.Range(0, bichiPrefabs.Length);
            SpawnBichi(logic, bichiPrefabs[randomPrefabIndex]);
        }
    }

    private void IncreaseRange()
    {
        for(int i = nextIndex; i < ballPopsSpawningBichis.Count; ++i)
        {
            ballPopsSpawningBichis[i] = ballPopsSpawningBichis[i] + 1;
        }
        int numIndices = ballPopsSpawningBichis.Count;
        var spawningString = string.Join(',', ballPopsSpawningBichis.GetRange(nextIndex, numIndices - nextIndex));
        Debug.LogFormat($"<color=#ff8000>[BICHIS]</color>INCREASE!: [{spawningString}]");
    }

    public void ClearAll()
    {
        foreach(var bichi in livingBichis)
        {
            if(bichi != null)
            {
                bichi.Destroyed -= OnBichiDestroyed;
                bichi.Dead -= OnBichiDied;
                Destroy(bichi.gameObject);
            }
        }
        livingBichis.Clear();

        ballPopsSpawningBichis.Clear();
    }

    private void OnDestroy()
    {
        ballsManager.BallPopped -= OnBallPopped;
        ClearAll();
    }
}
