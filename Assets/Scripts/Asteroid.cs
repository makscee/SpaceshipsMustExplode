using System;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public int X, Y;
    public int DirX, DirY;
    public static readonly Prefab Prefab = new Prefab("Asteroid");
    private static readonly Prefab ShadowPrefab = new Prefab("AsteroidShadow");
    private GameObject _shadow;
    private LineRenderer _lineRenderer;
    private int _moves = 15;

    public void InitPosition(int x, int y, int dirX, int dirY)
    {
        _lineRenderer = GetComponent<LineRenderer>();
        X = x;
        Y = y;
        DirX = dirX;
        DirY = dirY;
        transform.position = new Vector3(X * 2f, Y);
        _shadow = ShadowPrefab.Instantiate();
        EnableShadow();
    }


    public void DoMove()
    {
        _moves--;
        int tx = DirX, ty = DirY;
        var incrX = tx > 0 ? -1 : 1;
        var incrY = ty > 0 ? -1 : 1;
        while (tx != 0 || ty != 0)
        {
            if (Math.Abs(ty) >= Math.Abs(tx))
            {
                ty += incrY;
                Y += -incrY;
            }
            else
            {
                tx += incrX;
                X += -incrX;
            }

            var m = Check();
            if (m != null)
            {
                Utils.InvokeDelayed(() =>
                {
                    Ship.Instance.DeleteModule(m);
                    Destroy();
                }, 0.6f);
                break;
            }
        }
        ApplyMove();
        if (_moves <= 0)
        {
            Destroy();
        }
    }

    public void Destroy()
    {
        Destroy(gameObject);
        Destroy(_shadow);
        EngineEffects.UnitExplosion(transform.position);
    }

    private void EnableShadow()
    {
        _shadow.SetActive(true);
        _lineRenderer.enabled = true;
        _lineRenderer.SetPositions(new []{transform.position, transform.position + new Vector3(DirX * 2f, DirY)});
        _shadow.transform.position = new Vector3((X + DirX) * 2f, Y + DirY);
    }

    private void DisableShadow()
    {
        _shadow.SetActive(false);
        _lineRenderer.enabled = false;
    }

    private void ApplyMove()
    {
        DisableShadow();
        Utils.Animate(transform.position, new Vector3(X * 2f, Y), 0.5f, (v) => { transform.position += v; }, this, false, 0f, InterpolationType.InvSquare);
        Utils.InvokeDelayed(EnableShadow, 0.52f, this);
    }

    private ShipModule Check()
    {
//        var modules = new List<ShipModule>();
        var module = Ship.Instance.GetModuleGlobal(X, Y);
        return module;
//        if (module != null)
//        {
//            modules.Add(module);
//        }
//        module = Ship.Instance.GetModuleGlobal(X - 1, Y);
//        if (module != null)
//        {
//            modules.Add(module);
//        }
//        module = Ship.Instance.GetModuleGlobal(X, Y + 1);
//        if (module != null)
//        {
//            modules.Add(module);
//        }
//        module = Ship.Instance.GetModuleGlobal(X, Y - 1);
//        if (module != null)
//        {
//            modules.Add(module);
//        }
//
//        if (modules.Count == 0)
//        {
//            return null;
//        }
//
//        return modules[Random.Range(0, modules.Count)];
    }
}