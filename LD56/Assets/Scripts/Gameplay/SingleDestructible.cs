using UnityEngine;

public class SingleDestructible : MonoBehaviour
{
    [SerializeField] int numHits = 1;

    int hp = 0;

    private void Awake()
    {
        hp = numHits;
    }

    public void Hit(int dmg = 1)
    {
        hp = Mathf.Max(hp - dmg, 0);
        if(hp == 0)
        {
            Destroy(gameObject);
        }
    }
}
