using UnityEngine;
public class ChainNode: MonoBehaviour
{
    public ChainNode Next;
    public ChainNode Prev;

    public void Connect(ChainNode next, ChainNode prev)
    {
        Next = next;
        Prev = prev;
    }
}
