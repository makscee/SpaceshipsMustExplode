using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShipModule : MonoBehaviour
{
    public int X, Y, ShipX, ShipY, DirX, DirY;
    public bool Attached, Used;
    public ShipModule Parent;
    private LineRenderer _lr;
    public static readonly Color ShadowColor = new Color(0.008f, 0.369f, 0.039f, 0.45f);

    private void Awake()
    {
        _lr = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (_lr != null && Parent != null)
        {
            _lr.SetPositions(LinePositions());
        }
    }

    private Vector3[] _vec = new Vector3[2];
    private Vector3[] LinePositions()
    {
        _vec[0] = transform.position;
        _vec[1] = Parent.transform.position;
        return _vec;
    }

    public void Init(int x, int y, int dirX, int dirY)
    {
        X = x;
        Y = y;
        DirX = dirX;
        DirY = dirY;
        transform.position = new Vector3(X * 2, Y);
    }

    public void Attach(int x, int y)
    {
        X = x;
        Y = y;
        ShipX = x;
        ShipY = y;
        Attached = true;
    }

    public void DoMove(int x = 0, int y = 0)
    {
        if (!Attached)
        {
            if (Check())
            {
                Ship.Instance.AddModule(gameObject, X, Y, false);
                Ship.Instance.BFSLines();
            }
            else
            {
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

                    if (Check())
                    {
                        Ship.Instance.AddModule(gameObject, X, Y, false);
                        Ship.Instance.BFSLines();
                        break;
                    }
                }
            }
        }
        else
        {
            X += x;
            Y += y;
        }
        ApplyMove();
    }

    private void ApplyMove()
    {
        Utils.Animate(transform.position, new Vector3(X * 2f, Y), Random.Range(0.1f, 0.5f), (v) =>
        {
            transform.position += v;
        }, this, false, 0f, InterpolationType.InvSquare);
    }

    private bool Check()
    {
        if (Ship.Instance.GetModuleGlobal(X + 1, Y) != null ||
            Ship.Instance.GetModuleGlobal(X - 1, Y) != null ||
            Ship.Instance.GetModuleGlobal(X, Y + 1) != null ||
            Ship.Instance.GetModuleGlobal(X, Y - 1) != null)
        {
            return true;
        }
        return false;
    }
}