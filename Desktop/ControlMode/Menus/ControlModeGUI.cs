using GorillaLocomotion;
using GorillaNetworking;
using SakuraaCastingMod.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

#nullable disable
namespace SakuraaCastingMod.Desktop.ControlMode.Menus;

public class ControlModeGUI : MonoBehaviour
{
  private const float PcRange = 4f;
  public static ControlModeGUI Instance;
  public bool isInUse;
  private readonly Dictionary<KeyControl, GorillaKeyboardBindings> _buttonMapping = new Dictionary<KeyControl, GorillaKeyboardBindings>();
  private readonly Dictionary<GorillaKeyboardBindings, Key> _keyMapping = new Dictionary<GorillaKeyboardBindings, Key>()
  {
    {
      (GorillaKeyboardBindings) 17,
      (Key) 15
    },
    {
      (GorillaKeyboardBindings) 18,
      (Key) 16 /*0x10*/
    },
    {
      (GorillaKeyboardBindings) 19,
      (Key) 17
    },
    {
      (GorillaKeyboardBindings) 20,
      (Key) 18
    },
    {
      (GorillaKeyboardBindings) 21,
      (Key) 19
    },
    {
      (GorillaKeyboardBindings) 22,
      (Key) 20
    },
    {
      (GorillaKeyboardBindings) 23,
      (Key) 21
    },
    {
      (GorillaKeyboardBindings) 24,
      (Key) 22
    },
    {
      (GorillaKeyboardBindings) 25,
      (Key) 23
    },
    {
      (GorillaKeyboardBindings) 26,
      (Key) 24
    },
    {
      (GorillaKeyboardBindings) 27,
      (Key) 25
    },
    {
      (GorillaKeyboardBindings) 28,
      (Key) 26
    },
    {
      (GorillaKeyboardBindings) 29,
      (Key) 27
    },
    {
      (GorillaKeyboardBindings) 30,
      (Key) 28
    },
    {
      (GorillaKeyboardBindings) 31 /*0x1F*/,
      (Key) 29
    },
    {
      (GorillaKeyboardBindings) 32 /*0x20*/,
      (Key) 30
    },
    {
      (GorillaKeyboardBindings) 33,
      (Key) 31 /*0x1F*/
    },
    {
      (GorillaKeyboardBindings) 34,
      (Key) 32 /*0x20*/
    },
    {
      (GorillaKeyboardBindings) 35,
      (Key) 33
    },
    {
      (GorillaKeyboardBindings) 36,
      (Key) 34
    },
    {
      (GorillaKeyboardBindings) 37,
      (Key) 35
    },
    {
      (GorillaKeyboardBindings) 38,
      (Key) 36
    },
    {
      (GorillaKeyboardBindings) 39,
      (Key) 37
    },
    {
      (GorillaKeyboardBindings) 40,
      (Key) 38
    },
    {
      (GorillaKeyboardBindings) 41,
      (Key) 39
    },
    {
      (GorillaKeyboardBindings) 42,
      (Key) 40
    },
    {
      (GorillaKeyboardBindings) 0,
      (Key) 50
    },
    {
      (GorillaKeyboardBindings) 1,
      (Key) 41
    },
    {
      (GorillaKeyboardBindings) 2,
      (Key) 42
    },
    {
      (GorillaKeyboardBindings) 3,
      (Key) 43
    },
    {
      (GorillaKeyboardBindings) 4,
      (Key) 44
    },
    {
      (GorillaKeyboardBindings) 5,
      (Key) 45
    },
    {
      (GorillaKeyboardBindings) 6,
      (Key) 46
    },
    {
      (GorillaKeyboardBindings) 7,
      (Key) 47
    },
    {
      (GorillaKeyboardBindings) 8,
      (Key) 48 /*0x30*/
    },
    {
      (GorillaKeyboardBindings) 9,
      (Key) 49
    },
    {
      (GorillaKeyboardBindings) 14,
      (Key) 94
    },
    {
      (GorillaKeyboardBindings) 15,
      (Key) 95
    },
    {
      (GorillaKeyboardBindings) 16 /*0x10*/,
      (Key) 96 /*0x60*/
    },
    {
      (GorillaKeyboardBindings) 13,
      (Key) 2
    },
    {
      (GorillaKeyboardBindings) 12,
      (Key) 65
    },
    {
      (GorillaKeyboardBindings) 10,
      (Key) 63 /*0x3F*/
    },
    {
      (GorillaKeyboardBindings) 11,
      (Key) 64 /*0x40*/
    }
  };
  private bool _inRange;
  private GorillaComputerTerminal[] _terminals = Array.Empty<GorillaComputerTerminal>();

  public bool InRange => this._inRange;

  public void ToggleComputer()
  {
    if (!this.isInUse)
    {
      if (!this._inRange)
        return;
      this.isInUse = true;
    }
    else
      this.isInUse = false;
  }

  private void Awake()
  {
    ControlModeGUI.Instance = this;
    this.isInUse = false;
  }

  private void Start() => this.BuildButtonMap();

  private void Update()
  {
    if ((((UnityEngine.Object) SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode.Instance == (UnityEngine.Object) null) ? 1 : (!SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode.Instance.Enabled ? 1 : 0)) == 0)
    {
      if (!this.isInUse)
        return;
      if ((!this._inRange ? 1 : (((ButtonControl) Keyboard.current.escapeKey).wasPressedThisFrame ? 1 : 0)) != 0)
      {
        this.isInUse = false;
      }
      else
      {
        foreach (KeyControl key in this._buttonMapping.Keys)
        {
          try
          {
            if (key == null)
              UnityEngine.Debug.Log((object) "Key is null");
            if ((key == null ? 0 : (((ButtonControl) key).wasPressedThisFrame ? 1 : 0)) != 0)
            {
              UnityEngine.Debug.Log((object) ("Control Mode Pressed" + ((InputControl) key).name));
              GorillaComputer.instance.PressButton(this._buttonMapping[key]);
              Sounds.ControlModePlayKbSfx(66, 0.5f);
            }
          }
          catch (Exception ex)
          {
            UnityEngine.Debug.LogException(ex);
          }
        }
      }
    }
    else
      this.isInUse = false;
  }

  private void FixedUpdate()
  {
    if ((((UnityEngine.Object) SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode.Instance == (UnityEngine.Object) null) ? 1 : (!SakuraaCastingMod.Desktop.ControlMode.Main.ControlMode.Instance.Enabled ? 1 : 0)) != 0)
      return;
    if (Time.frameCount % 60 == 0)
      this._inRange = this.IsInRange();
    if ((this._inRange ? 0 : (Time.frameCount % 600 == 0 ? 1 : 0)) == 0)
      return;
    this._terminals = UnityEngine.Object.FindObjectsByType<GorillaComputerTerminal>((FindObjectsInactive) 0, (FindObjectsSortMode) 0);
  }

  private bool IsInRange()
  {
    bool flag;
    if ((((UnityEngine.Object) GTPlayer.Instance == (UnityEngine.Object) null) || ((UnityEngine.Object) GTPlayer.Instance.bodyCollider == (UnityEngine.Object) null) ? 1 : (((UnityEngine.Object) GorillaComputer.instance == (UnityEngine.Object) null) ? 1 : 0)) == 0)
    {
      Vector3 position = ((Component) GTPlayer.Instance.bodyCollider).transform.position;
      flag = (double) position.Distance(((Component) GorillaComputer.instance).transform.position) < 4.0 || ((IEnumerable<GorillaComputerTerminal>) this._terminals).Any<GorillaComputerTerminal>((Func<GorillaComputerTerminal, bool>) (gorillaComputerTerminal => ((UnityEngine.Object) gorillaComputerTerminal != (UnityEngine.Object) null) && (double) position.Distance(((Component) gorillaComputerTerminal).transform.position) < 4.0));
    }
    else
      flag = false;
    return flag;
  }

  private void BuildButtonMap()
  {
    foreach (KeyValuePair<GorillaKeyboardBindings, Key> keyValuePair in this._keyMapping)
      this._buttonMapping.Add(Keyboard.current[keyValuePair.Value], keyValuePair.Key);
  }
}
