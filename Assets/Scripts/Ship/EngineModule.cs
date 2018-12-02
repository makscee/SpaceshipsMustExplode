using UnityEngine;

public enum EngineDir
{
    Up, Down, Left, Right
}

public class EngineModule : ShipModule
{
    public static Prefab Left = new Prefab("EngineModuleLeft");
    public static Prefab Right = new Prefab("EngineModuleRight");
    public static Prefab Up = new Prefab("EngineModuleUp");
    public static Prefab Down = new Prefab("EngineModuleDown");
    
    public EngineDir Direction;
    public bool Activated, Overloaded;

    private GameObject _fire;
    public void SetActivated(bool v)
    {
        if (!v)
        {
            if (!Overloaded)
            {
                Activated = false;
                if (_fire != null)
                {
                    Destroy(_fire);
                }
            }
            else
            {
                Overloaded = false;
                var col = _fire.GetComponent<ParticleSystem>().colorOverLifetime;
                var grad = new Gradient();
                grad.SetKeys(new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.68f, 0f), 0f), new GradientColorKey(new Color(0.49f, 0.29f, 0f), 1f)},
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) });
                col.color = grad;
            }
        }
        else
        {
            if (!Activated)
            {
                Activated = true;
                float dirX = Direction == EngineDir.Left ? -1 : (Direction == EngineDir.Right ? 1 : 0);
                float dirY = Direction == EngineDir.Down ? -1 : (Direction == EngineDir.Up ? 1 : 0);
                var pos = new Vector2(dirX + X * 2, dirY / 2 + Y);
                _fire = EngineEffects.EngineFire(pos, Direction);
                _fire.transform.SetParent(transform);
            } else if (!Overloaded)
            {
                Overloaded = true;
                var col = _fire.GetComponent<ParticleSystem>().colorOverLifetime;
                var grad = new Gradient();
                grad.SetKeys(new GradientColorKey[] { new GradientColorKey(new Color(1f, 0f, 0.02f), 0f), new GradientColorKey(new Color(0.49f, 0.29f, 0f), 1f)},
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) });
                col.color = grad;
            }
        }
        GetComponent<SpriteRenderer>().color = Activated ? (Overloaded ? new Color(0.72f, 0f, 0f) : new Color(1f, 0.89f, 0.13f)) : Color.white;
    }
    
}