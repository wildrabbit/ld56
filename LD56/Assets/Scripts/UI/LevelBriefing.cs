using System;
using TMPro;
using UnityEngine;

class LevelBriefing: MonoBehaviour
{
    [SerializeField] TMP_Text levelName;
    [SerializeField] TMP_Text goal;
    [SerializeField] TMP_Text timer;
    [SerializeField] GameObject root;
    [SerializeField] TMP_Text lostBichis;

    public void Show(string levelName, string goal, float time, int bichisLimit)
    {
        gameObject.SetActive(true);
        this.levelName.text = levelName;
        this.goal.text = goal;
        this.timer.text = $"{Mathf.FloorToInt(time)}s";
        root.gameObject.SetActive(bichisLimit > 0);
        if(bichisLimit > 0)
        {
            lostBichis.text = bichisLimit.ToString();
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
