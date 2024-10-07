using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainDestructible : MonoBehaviour
{
    [SerializeField] bool loop = false;
    [SerializeField] float interval;
    [SerializeField] List<ChainNode> chainLinks = new();

    Coroutine coroutine;

    private void Awake()
    {
        //chainLinks = new List<ChainNode>(nodesRoot.GetComponentsInChildren<ChainNode>());

        if (chainLinks.Count < 2)
        {
            return;
        }
        var first = chainLinks[0];
        first.Connect(chainLinks[1], loop ? chainLinks[^1] : null);
        for (int i = 1; i < chainLinks.Count - 1; i++)
        {
            chainLinks[i].Connect(chainLinks[i + 1], chainLinks[i - 1]);
        }
        var last = chainLinks[^1];
        last.Connect(loop ? chainLinks[0] : null, chainLinks[^2]);
    }

    public void Detonate(ChainNode initial)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(DetonateRoutine(initial));
    }

    private IEnumerator DetonateRoutine(ChainNode initial)
    {
        yield return new WaitForSeconds(interval);
        initial.gameObject.SetActive(false);
        var left = initial.Prev;
        var right= initial.Next;
        while ((left != null && left.gameObject.activeSelf)
            || (right != null && right.gameObject.activeSelf))
        {
            if (left != null && left.gameObject.activeSelf)
            {
                left.gameObject.SetActive(false);
                left = left.Prev;
            }
            if (right != null && right.gameObject.activeSelf)
            {
                right.gameObject.SetActive(false);
                right = right.Next;
            }
            yield return new WaitForSeconds(interval);
        }
        Destroy(gameObject);
        coroutine = null;
    }

    private void OnDestroy()
    {
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }
}
