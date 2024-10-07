using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public enum RescueStatus
{
    Pending,
    Active,
    Rescued,
    Dead
}

[Serializable]
public class StatusMapping
{
    public RescueStatus status;
    public Sprite sprite;
}

public class HUD : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] TMP_Text hpValue;
    
    [Header("Rescue!")]
    [SerializeField] GameObject rescuePanelRoot;
    [SerializeField] RectTransform rescueStatusContainerRoot;
    [SerializeField] Image rescueStatusPrefab;
    [SerializeField] List<StatusMapping> statusMappings = new List<StatusMapping>();

    [Header("Timer")]
    [SerializeField] TMP_Text timer;

    [Header("Level")]
    [SerializeField] TMP_Text level;

    List<Image> rescueStatusInstances = new();

    private void Awake()
    {
        SetHP(0);
        SetTimer(0f);
        SetLevel("---");
        rescuePanelRoot.SetActive(false);
    }

    public void StartGame(int initialHP, List<RescueStatus> statuses, string levelName, float timeLeft)
    {
        SetHP(initialHP);
        InitRescuePanel(statuses);
        SetLevel(levelName);
        SetTimer(timeLeft);
    }

    private void InitRescuePanel(List<RescueStatus> statuses)
    {
        foreach (var instance in rescueStatusInstances)
        {
            Destroy(instance.gameObject);
        }
        rescueStatusInstances.Clear();
        rescuePanelRoot.SetActive(statuses != null && statuses.Count > 0);
        int idx = 0;
        foreach (var st in statuses)
        {
            var instance = Instantiate(rescueStatusPrefab, rescueStatusContainerRoot);
            rescueStatusInstances.Add(instance);

            instance.sprite = SpriteFromStatus(st);
            idx++;
        }
    }

    private Sprite SpriteFromStatus(RescueStatus st)
    {
        Sprite sp = null;
        foreach(var pair in statusMappings)
        {
            if(pair.status == st)
            {
                return pair.sprite;
            }
        }
        return sp;
    }

    public void SetTimer(float timeLeft)
    {
        TimeSpan ts = TimeSpan.FromSeconds(timeLeft);
        timer.text = $"T: {ts.Seconds:000}s";
    }

    public void SetLevel(string levelName)
    {
        level.text = levelName;
    }

    public void UpdateRescuePanel(List<RescueStatus> statuses)
    {
        Assert.IsTrue(statuses.Count == rescueStatusInstances.Count, "Invalid sizes!");
        int idx = 0;
        foreach (var st in statuses)
        {
            var instance = rescueStatusInstances[idx++];
            instance.sprite = SpriteFromStatus(st);
        }
    }

    public void SetHP(int hp)
    {
        hpValue.text = $"{hp:00}";
    }

    internal void StartGame(object hP, object value, string levelDisplayName, float v)
    {
        throw new NotImplementedException();
    }
}
