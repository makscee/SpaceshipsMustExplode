using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum Phase
{
    ShipMove, ElseMove, LevelOver
}
public class Level : MonoBehaviour
{
    public static Level Instance;
    public List<Asteroid> Asteroids = new List<Asteroid>();
    public List<ShipModule> Modules = new List<ShipModule>();
    public Phase CurrentPhase = Phase.ShipMove;
    public Text BottomText, WarningText, MovesText, SacrHint;
    public int Moves = 8, LevelNum = 1;

    private void Awake()
    {
        Instance = this;
        MovesText.text = "TURNS TO SURVIVES: " + Moves;
    }

    public void MoveElse()
    {
        Moves--;
        MovesText.text = "TURNS TO SURVIVE: " + Moves;
        if (Moves == 0)
        {
            LevelNum++;
            SetPhase(Phase.LevelOver);
            return;
        }
        var l = new List<ShipModule>();
        foreach (var module in Modules)
        {
            if (module == null) continue;
            module.DoMove();
            if (module.Attached)
            {
                l.Add(module);
            }
        }

        foreach (var shipModule in l)
        {
            Modules.Remove(shipModule);
        }
        var al = new List<Asteroid>();
        foreach (var asteroid in Asteroids)
        {
            if (asteroid == null)
            {
                al.Add(asteroid);
                continue;
            }
            asteroid.DoMove();
        }

        foreach (var asteroid in al)
        {
            Asteroids.Remove(asteroid);
        }
        
        if (Random.Range(0f, 1f) > 0.5f)
        {
            SpawnRandom(8, true);
        }
        SpawnRandom();
        if (Ship.Instance.Modules.Count > 12)
        {
            SpawnRandom();
        }
    }

    private void KillEverything()
    {
        foreach (var asteroid in Asteroids)
        {
            if (asteroid != null) asteroid.Destroy();
        }

        foreach (var module in Modules)
        {
            EngineEffects.UnitExplosion(module.transform.position);
            Destroy(module.gameObject);
        }
        Modules.Clear();
    }

    public void SetPhase(Phase p)
    {
        switch (p)
        {
                case Phase.ShipMove:
                    SacrHint.gameObject.SetActive(true);
                    CurrentPhase = Phase.ShipMove;
                    Moves = 8 + LevelNum * 4;
                    MovesText.text = "TURNS TO SURVIVE: " + Moves;
                    SpawnRandom();
                    SpawnRandom();
                    SpawnRandom();
                    break;
                case Phase.LevelOver:
                    SacrHint.gameObject.SetActive(false);
                    Player.UpdateSwapText();
                    KillEverything();
                    CurrentPhase = Phase.LevelOver;
                    Ship.Instance.SwapCount();
                    Player.UpdateSwapText();
                    break;
        }
    }

    private List<Prefab> _spawnModules = new List<Prefab>()
    {
        EngineModule.Down,
        EngineModule.Left,
        EngineModule.Right,
        EngineModule.Up,
        EmptyModule.prefab,
        EmptyModule.prefab,
        EmptyModule.prefab,
        BlockModule.prefab,
        BlockModule.prefab,
        SwapModule.prefab,
        SwapModule.prefab,
    };

    public void SpawnRandom(int radius = 8, bool m = false)
    {
        var a = Random.Range(0f, 360f);
        a *= Mathf.Deg2Rad;
        var spawnVec = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius;
        var dirVec = -spawnVec;
        dirVec = dirVec.normalized;
        dirVec *= Random.Range(2, 4);
        var x = (int)spawnVec.x + Ship.Instance.MainModule.X;
        var y = (int)spawnVec.y + Ship.Instance.MainModule.Y;
        var dirX = (int) dirVec.x;
        var dirY = (int) dirVec.y;
        
        if (m)
        {
            var module = _spawnModules[Random.Range(0, _spawnModules.Count)];
            AddModule(module.Instantiate(), x, y, dirX, dirY);
        }
        else
        {
            AddAsteroid(x, y, dirX, dirY);
        }
    }

    public void AddAsteroid(int x, int y, int dirX, int dirY)
    {
        var asteroid = Asteroid.Prefab.Instantiate().GetComponent<Asteroid>();
        asteroid.InitPosition(x, y, dirX, dirY);
        asteroid.transform.SetParent(transform);
        Asteroids.Add(asteroid);
    }

    public void AddModule(GameObject go, int x, int y, int dirX, int dirY)
    {
        var module = go.GetComponent<ShipModule>();
        Modules.Add(module);
        module.Init(x, y, dirX, dirY);
    }

    public void SetBottomText(string text, Color c)
    {
        BottomText.text = text;
        BottomText.color = c;
    }

    private float _warningTimer = 0f;
    public void AddWarningText(string text, Color c)
    {
        WarningText.gameObject.SetActive(true);
        WarningText.text = text;
        WarningText.color = c;
        _warningTimer = 4f;
    }

    private void Update()
    {
        if (_warningTimer > 0f)
        {
            _warningTimer -= Time.deltaTime;
        }
        if (_warningTimer <= 0f)
        {
            WarningText.gameObject.SetActive(false);
        }
    }
}