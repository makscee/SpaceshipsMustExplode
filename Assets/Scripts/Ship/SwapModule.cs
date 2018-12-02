using UnityEngine;

public class SwapModule : ShipModule
{
    public static Prefab prefab = new Prefab("SwapModule");
    public bool Swaped;

    public void SetUsed(bool v)
    {
        Swaped = v;
        GetComponent<SpriteRenderer>().color = v ? Color.gray : Color.white;
    }
}