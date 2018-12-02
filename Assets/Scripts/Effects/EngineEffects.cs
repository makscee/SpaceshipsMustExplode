using UnityEngine;

public class EngineEffects
{
    private static readonly Prefab _eFire = new Prefab("EngineFire");
    private static readonly Prefab _eBlast = new Prefab("EngineBlast");
    private static readonly Prefab _uExplosion = new Prefab("UnitExplosion");
    public static GameObject EngineFire(Vector2 pos, EngineDir dir)
    {
        var go = _eFire.Instantiate();
        switch (dir)
        {
            case EngineDir.Left:
                break;
            case EngineDir.Down:
                go.transform.Rotate(90f, 0f, 0f);
                break;
            case EngineDir.Right:
                go.transform.Rotate(180f, 0f, 0f);
                break;
            case EngineDir.Up:
                go.transform.Rotate(270f, 0f, 0f);
                break;
        }
        go.transform.position = pos;
        return go;
    }

    public static void EngineBlast(Vector2 pos, EngineDir dir)
    {
        var go = _eBlast.Instantiate();
        switch (dir)
        {
            case EngineDir.Left:
                break;
            case EngineDir.Down:
                go.transform.Rotate(90f, 0f, 0f);
                break;
            case EngineDir.Right:
                go.transform.Rotate(180f, 0f, 0f);
                break;
            case EngineDir.Up:
                go.transform.Rotate(270f, 0f, 0f);
                break;
        }

        go.transform.position = pos;
        Object.Destroy(go, 3f);
    }

    public static void UnitExplosion(Vector2 pos)
    {
        var go = _uExplosion.Instantiate();
        go.transform.position = pos;
        Object.Destroy(go, 3f);
    }
}