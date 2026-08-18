using SakuraaCastingMod.Shared.Helpers;
using UnityEngine;
using UnityEngine.XR;

#nullable disable
namespace SakuraaCastingMod.VR.Interaction;

public class HapticEngine : MonoBehaviour
{
  public static HapticEngine Ins;
  [SavedSetting("HapticsLevel", 2)]
  public static HapticsLevel Level = HapticsLevel.Normal;
  private const float MinImpulseDuration = 0.02f;
  private const float LowPriorityMinInterval = 0.03f;
  private const float HoldRefreshTimeout = 0.1f;
  private static readonly HapticEngine.Pattern[] Patterns = HapticEngine.BuildPatterns();
  private static readonly float[] LastPlayTimes = HapticEngine.BuildLastPlayTimes();
  private readonly HapticEngine.Channel _left = new HapticEngine.Channel();
  private readonly HapticEngine.Channel _right = new HapticEngine.Channel();
  private static bool _contextActive;
  private static bool _contextHandLeft;
  private static bool _contextConsumed;

  private static HapticEngine.Pattern[] BuildPatterns()
  {
    return new HapticEngine.Pattern[22]
    {
      new HapticEngine.Pattern(HapticEngine.Priority.Normal, HapticsLevel.Normal, 0.0f, new HapticEngine.Step[1]
      {
        new HapticEngine.Step(0.18f, 0.02f)
      }),
      new HapticEngine.Pattern(HapticEngine.Priority.Normal, HapticsLevel.Normal, 0.0f, new HapticEngine.Step[1]
      {
        new HapticEngine.Step(0.45f, 0.035f)
      }),
      new HapticEngine.Pattern(HapticEngine.Priority.Normal, HapticsLevel.Normal, 0.0f, new HapticEngine.Step[3]
      {
        new HapticEngine.Step(0.35f, 0.02f),
        new HapticEngine.Step(0.0f, 0.03f),
        new HapticEngine.Step(0.6f, 0.04f)
      }),
      new HapticEngine.Pattern(HapticEngine.Priority.Normal, HapticsLevel.Normal, 0.0f, new HapticEngine.Step[3]
      {
        new HapticEngine.Step(0.6f, 0.03f),
        new HapticEngine.Step(0.0f, 0.03f),
        new HapticEngine.Step(0.22f, 0.02f)
      }),
      new HapticEngine.Pattern(HapticEngine.Priority.Low, HapticsLevel.Normal, 0.0f, new HapticEngine.Step[1]
      {
        new HapticEngine.Step(0.22f, 0.015f)
      }),
      new HapticEngine.Pattern(HapticEngine.Priority.Normal, HapticsLevel.Normal, 0.0f, new HapticEngine.Step[1]
      {
        new HapticEngine.Step(0.7f, 0.06f)
      }),
      new HapticEngine.Pattern(HapticEngine.Priority.Normal, HapticsLevel.Normal, 0.0f, new HapticEngine.Step[1]
      {
        new HapticEngine.Step(0.3f, 0.03f)
      }),
      new HapticEngine.Pattern(HapticEngine.Priority.High, HapticsLevel.Minimal, 0.0f, new HapticEngine.Step[3]
      {
        new HapticEngine.Step(0.8f, 0.05f),
        new HapticEngine.Step(0.0f, 0.04f),
        new HapticEngine.Step(0.85f, 0.07f)
      }),
      new HapticEngine.Pattern(HapticEngine.Priority.Normal, HapticsLevel.Normal, 0.0f, new HapticEngine.Step[3]
      {
        new HapticEngine.Step(0.25f, 0.04f),
        new HapticEngine.Step(0.0f, 0.02f),
        new HapticEngine.Step(0.1f, 0.03f)
      }),
      new HapticEngine.Pattern(HapticEngine.Priority.High, HapticsLevel.Minimal, 0.0f, new HapticEngine.Step[3]
      {
        new HapticEngine.Step(0.55f, 0.045f),
        new HapticEngine.Step(0.0f, 0.02f),
        new HapticEngine.Step(0.3f, 0.02f)
      }),
      new HapticEngine.Pattern(HapticEngine.Priority.Normal, HapticsLevel.Minimal, 0.0f, new HapticEngine.Step[1]
      {
        new HapticEngine.Step(0.3f, 0.03f)
      }),
      new HapticEngine.Pattern(HapticEngine.Priority.Normal, HapticsLevel.Normal, 0.0f, new HapticEngine.Step[3]
      {
        new HapticEngine.Step(0.4f, 0.025f),
        new HapticEngine.Step(0.0f, 0.05f),
        new HapticEngine.Step(0.4f, 0.025f)
      }),
      new HapticEngine.Pattern(HapticEngine.Priority.Normal, HapticsLevel.Normal, 0.0f, new HapticEngine.Step[3]
      {
        new HapticEngine.Step(0.25f, 0.02f),
        new HapticEngine.Step(0.0f, 0.03f),
        new HapticEngine.Step(0.45f, 0.04f)
      }),
      new HapticEngine.Pattern(HapticEngine.Priority.Normal, HapticsLevel.Normal, 0.0f, new HapticEngine.Step[3]
      {
        new HapticEngine.Step(0.45f, 0.04f),
        new HapticEngine.Step(0.0f, 0.03f),
        new HapticEngine.Step(0.2f, 0.02f)
      }),
      new HapticEngine.Pattern(HapticEngine.Priority.Normal, HapticsLevel.Normal, 0.0f, new HapticEngine.Step[3]
      {
        new HapticEngine.Step(0.4f, 0.03f),
        new HapticEngine.Step(0.0f, 0.04f),
        new HapticEngine.Step(0.4f, 0.03f)
      }),
      new HapticEngine.Pattern(HapticEngine.Priority.Low, HapticsLevel.ALot, 0.0f, new HapticEngine.Step[1]
      {
        new HapticEngine.Step(0.3f, 0.02f)
      }),
      new HapticEngine.Pattern(HapticEngine.Priority.High, HapticsLevel.Minimal, 0.0f, new HapticEngine.Step[3]
      {
        new HapticEngine.Step(0.5f, 0.03f),
        new HapticEngine.Step(0.0f, 0.08f),
        new HapticEngine.Step(0.5f, 0.03f)
      }),
      new HapticEngine.Pattern(HapticEngine.Priority.High, HapticsLevel.Minimal, 0.0f, new HapticEngine.Step[5]
      {
        new HapticEngine.Step(0.3f, 0.04f),
        new HapticEngine.Step(0.0f, 0.06f),
        new HapticEngine.Step(0.5f, 0.04f),
        new HapticEngine.Step(0.0f, 0.06f),
        new HapticEngine.Step(0.9f, 0.08f)
      }),
      new HapticEngine.Pattern(HapticEngine.Priority.High, HapticsLevel.Minimal, 0.0f, new HapticEngine.Step[3]
      {
        new HapticEngine.Step(0.8f, 0.12f),
        new HapticEngine.Step(0.0f, 0.05f),
        new HapticEngine.Step(0.3f, 0.03f)
      }),
      new HapticEngine.Pattern(HapticEngine.Priority.Low, HapticsLevel.ALot, 1f, new HapticEngine.Step[1]
      {
        new HapticEngine.Step(0.3f, 0.035f)
      }),
      new HapticEngine.Pattern(HapticEngine.Priority.High, HapticsLevel.Minimal, 0.0f, new HapticEngine.Step[5]
      {
        new HapticEngine.Step(0.6f, 0.05f),
        new HapticEngine.Step(0.0f, 0.05f),
        new HapticEngine.Step(0.6f, 0.05f),
        new HapticEngine.Step(0.0f, 0.05f),
        new HapticEngine.Step(0.6f, 0.05f)
      }),
      new HapticEngine.Pattern(HapticEngine.Priority.Normal, HapticsLevel.Normal, 0.0f, new HapticEngine.Step[1]
      {
        new HapticEngine.Step(0.7f, 0.05f)
      })
    };
  }

  private static float[] BuildLastPlayTimes()
  {
    float[] numArray = new float[22];
    for (int index = 0; index < numArray.Length; ++index)
      numArray[index] = float.NegativeInfinity;
    return numArray;
  }

  private void Awake() => HapticEngine.Ins = this;

  private void OnDestroy()
  {
    if (!((UnityEngine.Object) HapticEngine.Ins == (UnityEngine.Object) this))
      return;
    HapticEngine.Ins = (HapticEngine) null;
  }

  public static void Play(HapticPreset preset, bool isLeftHand)
  {
    HapticEngine.Ins?.PlayInternal(preset, isLeftHand);
  }

  public static void Play(HapticPreset preset)
  {
    bool isLeftHand = HapticEngine._contextActive && HapticEngine._contextHandLeft;
    if (HapticEngine._contextActive)
      HapticEngine._contextConsumed = true;
    HapticEngine.Ins?.PlayInternal(preset, isLeftHand);
  }

  public static void BeginPress(bool isLeftHand)
  {
    HapticEngine._contextActive = true;
    HapticEngine._contextHandLeft = isLeftHand;
    HapticEngine._contextConsumed = false;
  }

  public static void EndPress(HapticPreset fallback)
  {
    if ((!HapticEngine._contextActive ? 0 : (!HapticEngine._contextConsumed ? 1 : 0)) != 0)
      HapticEngine.Ins?.PlayInternal(fallback, HapticEngine._contextHandLeft);
    HapticEngine._contextActive = false;
    HapticEngine._contextConsumed = false;
  }

  public static void SuppressPressDefault()
  {
    if (!HapticEngine._contextActive)
      return;
    HapticEngine._contextConsumed = true;
  }

  public static void AbortPress()
  {
    HapticEngine._contextActive = false;
    HapticEngine._contextConsumed = false;
  }

  public static void SetHoldProgress(bool isLeftHand, float progress)
  {
    if ((((UnityEngine.Object) HapticEngine.Ins == (UnityEngine.Object) null) || HapticEngine.Level < HapticsLevel.ALot ? 1 : (!XRSettings.isDeviceActive ? 1 : 0)) != 0)
      return;
    HapticEngine.Channel channel = isLeftHand ? HapticEngine.Ins._left : HapticEngine.Ins._right;
    channel.HoldAmp = Mathf.Lerp(0.08f, 0.35f, Mathf.Clamp01(progress));
    channel.HoldRefreshTime = Time.time;
  }

  public static void CancelHold(bool isLeftHand)
  {
    if (((UnityEngine.Object) HapticEngine.Ins == (UnityEngine.Object) null))
      return;
    (isLeftHand ? HapticEngine.Ins._left : HapticEngine.Ins._right).HoldAmp = 0.0f;
  }

  private void PlayInternal(HapticPreset preset, bool isLeftHand)
  {
    HapticEngine.Pattern pattern = HapticEngine.Patterns[(int) preset];
    if (HapticEngine.Level < pattern.MinLevel || !XRSettings.isDeviceActive)
      return;
    HapticEngine.Channel ch = isLeftHand ? this._left : this._right;
    if (((double) pattern.Cooldown <= 0.0 ? 0 : ((double) Time.time - (double) HapticEngine.LastPlayTimes[(int) preset] < (double) pattern.Cooldown ? 1 : 0)) != 0 || (pattern.Priority != HapticEngine.Priority.Low ? 0 : ((double) Time.time - (double) ch.LastLowPlayTime < 0.029999999329447746 ? 1 : 0)) != 0 || (ch.Steps == null ? 0 : (pattern.Priority < ch.Priority ? 1 : 0)) != 0)
      return;
    HapticEngine.LastPlayTimes[(int) preset] = Time.time;
    if (pattern.Priority == HapticEngine.Priority.Low)
      ch.LastLowPlayTime = Time.time;
    ch.Steps = pattern.Steps;
    ch.StepIndex = 0;
    ch.StepElapsed = 0.0f;
    ch.Priority = pattern.Priority;
    this.Emit(ch, isLeftHand, ch.Steps[0].Amp);
  }

  private void Update()
  {
    this.UpdateChannel(this._left, true);
    this.UpdateChannel(this._right, false);
  }

  private void UpdateChannel(HapticEngine.Channel ch, bool isLeftHand)
  {
    if (ch.Steps != null)
    {
      for (ch.StepElapsed += Time.deltaTime; (ch.StepIndex >= ch.Steps.Length ? 0 : ((double) ch.StepElapsed >= (double) ch.Steps[ch.StepIndex].Duration ? 1 : 0)) != 0; ++ch.StepIndex)
        ch.StepElapsed -= ch.Steps[ch.StepIndex].Duration;
      if (ch.StepIndex < ch.Steps.Length)
      {
        float amp = ch.Steps[ch.StepIndex].Amp;
        if ((double) amp > 0.0)
          this.Emit(ch, isLeftHand, amp);
        else
          HapticEngine.StopEmit(ch);
      }
      else
      {
        ch.Steps = (HapticEngine.Step[]) null;
        HapticEngine.StopEmit(ch);
      }
    }
    else if (((double) ch.HoldAmp <= 0.0 ? 0 : ((double) Time.time - (double) ch.HoldRefreshTime <= 0.10000000149011612 ? 1 : 0)) == 0)
    {
      ch.HoldAmp = 0.0f;
      HapticEngine.StopEmit(ch);
    }
    else
      this.Emit(ch, isLeftHand, ch.HoldAmp);
  }

  private void Emit(HapticEngine.Channel ch, bool isLeftHand, float amp)
  {
    if (!ch.Device.isValid)
    {
      ch.Device = HapticEngine.ResolveDevice(isLeftHand);
      if (!ch.Device.isValid)
        return;
    }
    ch.Device.SendHapticImpulse(0U, Mathf.Clamp01(amp), Mathf.Max(Time.deltaTime * 2f, 0.02f));
    ch.Emitting = true;
  }

  private static void StopEmit(HapticEngine.Channel ch)
  {
    if (!ch.Emitting)
      return;
    ch.Emitting = false;
    if (!ch.Device.isValid)
      return;
    ch.Device.StopHaptics();
  }

  private static InputDevice ResolveDevice(bool isLeftHand)
  {
    InputDevice inputDevice1;
    if (((UnityEngine.Object) ControllerInputPoller.instance != (UnityEngine.Object) null))
    {
      InputDevice inputDevice2 = isLeftHand ? ControllerInputPoller.instance.leftControllerDevice : ControllerInputPoller.instance.rightControllerDevice;
      if (inputDevice2.isValid)
      {
        inputDevice1 = inputDevice2;
        goto label_4;
      }
    }
    inputDevice1 = InputDevices.GetDeviceAtXRNode(isLeftHand ? (XRNode) 4 : (XRNode) 5);
label_4:
    return inputDevice1;
  }

  private enum Priority
  {
    Low,
    Normal,
    High,
  }

  private struct Step(float amp, float duration)
  {
    public readonly float Amp = amp;
    public readonly float Duration = duration;
  }

  private class Pattern
  {
    public readonly HapticEngine.Step[] Steps;
    public readonly HapticEngine.Priority Priority;
    public readonly HapticsLevel MinLevel;
    public readonly float Cooldown;

    public Pattern(
      HapticEngine.Priority priority,
      HapticsLevel minLevel,
      float cooldown,
      params HapticEngine.Step[] steps)
    {
      this.Steps = steps;
      this.Priority = priority;
      this.MinLevel = minLevel;
      this.Cooldown = cooldown;
    }
  }

  private class Channel
  {
    public HapticEngine.Step[] Steps;
    public int StepIndex;
    public float StepElapsed;
    public HapticEngine.Priority Priority;
    public InputDevice Device;
    public float LastLowPlayTime = float.NegativeInfinity;
    public float HoldAmp;
    public float HoldRefreshTime;
    public bool Emitting;
  }
}
