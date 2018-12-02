using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0);
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            var vec = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var x = (int)Math.Round(vec.x / 2);
            var y = (int)Math.Round(vec.y);
            if (Level.Instance.CurrentPhase == Phase.LevelOver && Ship.Instance.Swaps > 0)
            {
                var firstSelected = Ship.Instance.SwapFirstSelected;
                if (Ship.Instance.SwapSelect(x, y) && firstSelected)
                {
                    Ship.Instance.DoSwap();
                    UpdateSwapText();
                }
                return;
            }
            var module = Ship.Instance.GetModuleGlobal(x, y);
            if (module != null)
            {
                if (Level.Instance.CurrentPhase == Phase.ShipMove)
                {
                    var engine = module as EngineModule;
                    if (engine != null && engine.Activated)
                    {
                        Level.Instance.AddWarningText("CANT SACRIFICE ACTIVATED ENGINE", Color.yellow);
                    }
                    else
                    {
                        Ship.Instance.DeleteModule(module);
                        Ship.Instance.Overload = true;
                    }
                }
            }
        }
        if (Level.Instance.CurrentPhase == Phase.ShipMove)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                Ship.Instance.AddMove(0, 1);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                Ship.Instance.AddMove(0, -1);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                Ship.Instance.AddMove(1, 0);
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                Ship.Instance.AddMove(-1, 0);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Ship.Instance.Overload = false;
                Level.Instance.SetBottomText("WASD TO CHOSE DIRECTION", Color.green);
                Ship.Instance.DoMove();
                Utils.InvokeDelayed(() => Level.Instance.MoveElse(), 0.6f);
            }
        } else if (Level.Instance.CurrentPhase == Phase.LevelOver)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Level.Instance.SetPhase(Phase.ShipMove);
            }
        }
    }

    public static void UpdateSwapText()
    {
        var swaps = Ship.Instance.Swaps > 0 ? "CLICK MODULES TO SWAP THEM\n" + Ship.Instance.Swaps : "NO";
        Level.Instance.SetBottomText(swaps + " SWAPS LEFT\n(BASED ON SWAP MODULES COUNT)\n<color=#ffff00ff>SPACE TO CONTINUE</color>", Color.white);
    }
}