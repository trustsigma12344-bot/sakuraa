using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework;

public class AnimState
{
  public float Hover;
  public float Press;
  public float Toggle;
  public float Focus;
  public bool HoverLatch;
  private float _toggleVel;
  private double _last = -1.0;

  public void Advance(bool hovered, bool pressed, bool? toggledOn = null, bool? focused = null)
  {
    if ((Event.current == null ? 1 : (Event.current.type != (EventType) 7 ? 1 : 0)) != 0)
      return;
    double sinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
    if (this._last < 0.0)
      this._last = sinceStartupAsDouble;
    float dt = Mathf.Min((float) (sinceStartupAsDouble - this._last), 0.05f);
    this._last = sinceStartupAsDouble;
    if ((double) dt <= 0.0)
      return;
    this.Hover = UiAnim.MoveTowardsExp(this.Hover, hovered ? 1f : 0.0f, 0.045f, dt);
    this.Press = UiAnim.MoveTowardsExp(this.Press, pressed ? 1f : 0.0f, 0.03f, dt);
    if (toggledOn.HasValue)
      UiAnim.Spring(ref this.Toggle, ref this._toggleVel, toggledOn.Value ? 1f : 0.0f, 9f, dt);
    if (!focused.HasValue)
      return;
    this.Focus = UiAnim.MoveTowardsExp(this.Focus, focused.Value ? 1f : 0.0f, 0.06f, dt);
  }
}
