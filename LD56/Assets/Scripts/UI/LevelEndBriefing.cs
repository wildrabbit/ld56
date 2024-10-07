using UnityEngine;

public class LevelEndBriefing: MonoBehaviour
{
    [SerializeField] GameObject win;
    [SerializeField] GameObject lose;

    public void Show(GameResult result)
    {
        gameObject.SetActive(true);
        win.gameObject.SetActive(result == GameResult.Won);
        lose.gameObject.SetActive(result == GameResult.Lost);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
