using System.Collections.Generic;
using UnityEngine;

public class ShipMap
{
    private struct KeyPair
    {
        public KeyPair(int k1, int k2)
        {
            _k1 = k1;
            _k2 = k2;
        }
        private int _k1, _k2;
        public override bool Equals(object obj)
        {
            if (!(obj is KeyPair)) return false;
            var k = (KeyPair) obj;
            return k._k1 == _k1 && k._k2 == _k2;

        }

        public override int GetHashCode()
        {
            return _k1 + _k2;
        }
    }
    private Dictionary<KeyPair, ShipModule> map = new Dictionary<KeyPair, ShipModule>();
    public ShipModule this[int key1, int key2]
    {
        get
        {
            var k = new KeyPair(key1, key2);
            if (!map.ContainsKey(k))
            {
                return null;
            }
            return map[k];
        }
        set { map[new KeyPair(key1, key2)] = value; }
    }
}

public class Ship : MonoBehaviour
{
    public static Ship Instance;
    public List<ShipModule> Modules = new List<ShipModule>();
    public ShipMap Map = new ShipMap();
    public MainModule MainModule;
    public bool Overload;
    
    private readonly List<ModuleShadow> _moduleShadows = new List<ModuleShadow>();
    private readonly Prefab _moduleShadowPrefab = new Prefab("ModuleShadow");
    private readonly List<EngineModule> _engines = new List<EngineModule>();

    private void Awake()
    {
        Instance = this;
        AddModule(MainModule.prefab.Instantiate(), 0, 0);
        AddModule(EmptyModule.prefab.Instantiate(), -1, 0);
        AddModule(SwapModule.prefab.Instantiate(), -2, 0);
        Level.Instance.AddModule(EngineModule.Left.Instantiate(), -1, 3, 0, -1);
        Level.Instance.AddModule(EngineModule.Left.Instantiate(), -1, -3, 0, 1);
        Level.Instance.AddModule(EngineModule.Up.Instantiate(), 0, 4, 0, -1);
        Level.Instance.AddModule(EngineModule.Down.Instantiate(), 0, -4, 0, 1);
        
        BFSLines();
        Level.Instance.AddAsteroid(6, 6, -2, -1);
//        Level.Instance.AddAsteroid(-6, 7, 2, -1);
        Level.Instance.AddAsteroid(7, 0, -1, 0);
        Level.Instance.AddModule(EmptyModule.prefab.Instantiate(), 5, 4, 0, -1); 
        
    }
    

    public void AddModule(GameObject go, int x, int y, bool local = true)
    {
        int lx = x, ly = y;
        if (!local)
        {
            lx = x - MainModule.X;
            ly = y - MainModule.Y;
        }
        var module = go.GetComponent<ShipModule>();
        Map[lx, ly] = module;
        module.Attached = true;
        var engineModule = module as EngineModule;
        if (engineModule != null)
        {
            _engines.Add(engineModule);
        }

        var mainModule = module as MainModule;
        if (mainModule != null)
        {
            MainModule = mainModule;
        }
        module.Attach(x, y);
        module.ShipX = lx;
        module.ShipY = ly;
        Modules.Add(module);
        module.transform.position = new Vector3(x * 2f, y);
        module.transform.parent = transform;
        var shadow = _moduleShadowPrefab.Instantiate().GetComponent<ModuleShadow>();
        shadow.Disabe();
        _moduleShadows.Add(shadow);
    }

    public ShipModule GetModuleLocal(int x, int y)
    {
        foreach (var module in Modules)
        {
            if (module.ShipX == x && module.ShipY == y)
            {
                return module;
            }
        }
        return null;
    }

    public ShipModule GetModuleGlobal(int x, int y)
    {
        return Map[x - MainModule.X, y - MainModule.Y];
    }

    public void DeleteModule(ShipModule module)
    {
        var engineModule = module as EngineModule;
        if (engineModule != null)
        {
            _engines.Remove(engineModule);
        }
        Modules.Remove(module);
        Map[module.ShipX, module.ShipY] = null;
        Destroy(module.gameObject);
        EngineEffects.UnitExplosion(module.transform.position);
        BFSLines();
    }

    public void DisableShadows()
    {
        foreach (var shadow in _moduleShadows)
        {
            shadow.Disabe();
        }
    }

    private int dirX, dirY;

    public void AddMove(int x, int y)
    {
        if (_engines.Count == 0)
        {
            Level.Instance.SetBottomText("NO ENGINES\n<color=#ffff00ff>SPACE TO SKIP TURN</color>", Color.red);
            DrawShadows();
            ShadowTwitch(new Color(0.79f, 0.77f, 0.18f), x, y);
            return;
        }
        Level.Instance.SetBottomText("SPACE TO JUMP", Color.yellow);
        var collideCheckPassed = true;
        foreach (var asteroid in Level.Instance.Asteroids)
        {
            if (asteroid == null) continue;
            if (Map[asteroid.X - MainModule.X - x - dirX, asteroid.Y - MainModule.Y - y - dirY] == null) continue;
            collideCheckPassed = false;
            break;
        }

        if (collideCheckPassed)
        {
            foreach (var module in Level.Instance.Modules)
            {
                if (Map[module.X - MainModule.X - x - dirX, module.Y - MainModule.Y - y - dirY] == null) continue;
                collideCheckPassed = false;
                break;
            }
        }
        if (!collideCheckPassed)
        {
            ShadowTwitch(new Color(0.79f, 0.16f, 0.37f), x, y);
            return;
        }
        if (x != 0)
        {
            if (dirX < 0 && x > 0 || dirX > 0 && x < 0)
            {
                SetEngine(false, x > 0 ? EngineDir.Right : EngineDir.Left);
            }
            else
            {
                if (!SetEngine(true, x < 0 ? EngineDir.Right : EngineDir.Left))
                {
                    ShadowTwitch(new Color(0.79f, 0.77f, 0.18f), x, y);
                    return;
                }
            }
        }
        if (y != 0)
        {
            if (dirY < 0 && y > 0 || dirY > 0 && y < 0)
            {
                SetEngine(false, y > 0 ? EngineDir.Up : EngineDir.Down);
            }
            else
            {
                if (!SetEngine(true, y < 0 ? EngineDir.Up : EngineDir.Down))
                {
                    ShadowTwitch(new Color(0.79f, 0.77f, 0.18f), x, y);
                    return;
                }
            }
        }
        dirX += x;
        dirY += y;
        DrawShadows();
    }

    private void DrawShadows()
    {
        var ind = 0;
        foreach (var module in Modules)
        {
            if (module == null) continue;
            var c = ShipModule.ShadowColor;
            if (module == MainModule)
            {
                c = new Color(0f, 0f, 1f, 0.55f);
            } else if (module is EngineModule)
            {
                c = new Color(1f, 0.93f, 0f, 0.54f);
            }

            _moduleShadows[ind].transform.position = new Vector3((module.X + dirX) * 2, module.Y + dirY);
            _moduleShadows[ind].Activate(c);
            ind++;
        }
    }

    private void ShadowTwitch(Color color, int dirX, int dirY)
    {
        var dirVec = new Vector3(dirX * 2, dirY);
        foreach (var shadow in _moduleShadows)
        {
            shadow.transform.position += dirVec;
        }

        var t = 0.2f;
        Utils.Animate(Vector3.zero, -dirVec, t, (v) =>
        {
            foreach (var shadow in _moduleShadows)
            {
                shadow.transform.position += v;
                Utils.Animate(color, shadow.Color, t, (v1) =>
                {
                    shadow.GetComponent<SpriteRenderer>().color = v1;
                }, null, true);
            }
        });
    }

    private bool SetEngine(bool active, EngineDir dir)
    {
        var l = new List<EngineModule>();
        var lo = new List<EngineModule>();
        foreach (var engine in _engines)
        {
            if (engine.Direction == dir && (engine.Activated != active || Overload && engine.Overloaded != active))
            {
                if (!active)
                {
                    engine.SetActivated(false);
                    _engines.Remove(engine);
                    _engines.Add(engine);
                    return true;
                }

                if (!engine.Activated)
                {
                    l.Add(engine);
                }
                else
                {
                    lo.Add(engine);
                }
            }
        }

        if (l.Count == 0)
        {
            if (!Overload) return false;
            if (lo.Count == 0) return false;
            l = lo;
        }
        var e = l[Random.Range(0, l.Count)];
        e.SetActivated(true);
        _engines.Remove(e);
        _engines.Insert(0, e);
        return true;
    }

    private void DisableEngines()
    {
        foreach (var engineModule in _engines)
        {
            if (engineModule.Overloaded) engineModule.SetActivated(false);
            engineModule.SetActivated(false);
        }
    }

    private void EnginesBlast()
    {
        var l = new List<ShipModule>();
        foreach (var engine in _engines)
        {
            if (!engine.Activated) continue;
            var dirX = engine.Direction == EngineDir.Left ? -1 : (engine.Direction == EngineDir.Right ? 1 : 0);
            var dirY = engine.Direction == EngineDir.Down ? -1 : (engine.Direction == EngineDir.Up ? 1 : 0);
            var pos = new Vector2(dirX + engine.X * 2, dirY / 2f + engine.Y);
            EngineEffects.EngineBlast(pos, engine.Direction);
            var m = GetModuleGlobal(dirX + engine.X, dirY + engine.Y);
            if (m != null)
            {
                l.Add(m);
            }

            foreach (var asteroid in Level.Instance.Asteroids)
            {
                if (asteroid == null) continue;
                if (asteroid.X == dirX + engine.X && asteroid.Y == dirY + engine.Y)
                {
                    asteroid.Destroy();
                }
            }
        }

        foreach (var module in l)
        {
            DeleteModule(module);
        }
    }

    public void DoMove()
    {
        EnginesBlast();
        foreach (var module in Modules)
        {
            if (module == null) continue;
            module.DoMove(dirX, dirY);
        }
        dirX = 0;
        dirY = 0;
        DisableShadows();
        DisableEngines();
    }

    public void BFSLines()
    {
        foreach (var module in Modules)
        {
            module.Used = false;
        }
        BFSStep(MainModule, null);
        var l = new List<ShipModule>();
        foreach (var module in Modules)
        {
            if (!module.Used)
            {
                l.Add(module);
            }
        }

        foreach (var module in l)
        {
            DeleteModule(module);
        }
    }

    private void BFSStep(ShipModule module, ShipModule parent)
    {
        if (module == null)
        {
            return;
        }
        if (module.Used) return;
        module.Used = true;
        if (module is BlockModule)
        {
            return;
        }
        if (parent != null)
        {
            module.Parent = parent;
        }
        int x = module.ShipX, y = module.ShipY;
        BFSStep(Map[x + 1, y], module);
        BFSStep(Map[x - 1, y], module);
        BFSStep(Map[x, y - 1], module);
        BFSStep(Map[x, y + 1], module);
    }

    public int Swaps = 0;
    public void SwapCount()
    {
        var c = 0;
        foreach (var module in Modules)
        {
            if (module is SwapModule)
                c++;
        }
        Swaps = c;
    }

    private ShipModule _swap1, _swap2;
    private int x1, x2, y1, y2;
    public bool SwapFirstSelected;

    public bool SwapSelect(int x, int y)
    {
        if (GetModuleGlobal(x, y) == MainModule)
        {
            Level.Instance.AddWarningText("CAN'T SWAP MAIN MODULE", Color.yellow);
            return false;
        } 
        if (SwapFirstSelected)
        {
            if (x == x1 && y == y1)
            {
                SwapFirstSelected = false;
                DisableShadows();
                return false;
            }
            _swap2 = GetModuleGlobal(x, y);
            x2 = x;
            y2 = y;
            var res = _swap1 != null || _swap2 != null;
            if (!res)
            {
                SwapFirstSelected = false;
                DisableShadows();
            }
            return res;
        }
        _swap1 = GetModuleGlobal(x, y);
        x1 = x;
        y1 = y;
        const int ind = 0;
        var c = new Color(0.04f, 0f, 1f, 0.75f);
        _moduleShadows[ind].transform.position = new Vector3(x * 2, y);
        _moduleShadows[ind].Activate(c);
        SwapFirstSelected = true;
        return true;
    }

    public void DoSwap()
    {
        Map[x1 - MainModule.X, y1 - MainModule.Y] = null;
        Map[x2 - MainModule.X, y2 - MainModule.Y] = null;
        if (_swap2 != null)
        {
            _swap2.ShipX = x1 - MainModule.X;
            _swap2.ShipY = y1 - MainModule.Y;
            _swap2.X = x1;
            _swap2.Y = y1;
            _swap2.transform.position = new Vector3(x1 * 2, y1);
            Map[_swap2.ShipX, _swap2.ShipY] = _swap2;
        }

        if (_swap1 != null)
        {
            _swap1.ShipX = x2 - MainModule.X;
            _swap1.ShipY = y2 - MainModule.Y;
            _swap1.X = x2;
            _swap1.Y = y2;
            _swap1.transform.position = new Vector3(x2 * 2, y2);
            Map[_swap1.ShipX, _swap1.ShipY] = _swap1;
        }

        Swaps--;
        SwapFirstSelected = false;
        DisableShadows();
        BFSLines();
    }
}