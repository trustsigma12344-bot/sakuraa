using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.MenuItems;

public class TextFieldMenuItem : MenuItem
{
  private int _caret;
  private bool _selectAll;
  private KeyControl _repeatKey;
  private Action _repeatAction;
  private float _repeatTimer;
  private const float RepeatDelay = 0.4f;
  private const float RepeatRate = 0.045f;
  private static TextFieldMenuItem _focused;
  private static Keyboard _subscribedKb;

  public string Text { get; set; } = "";

  public int MaxLength { get; set; } = 0;

  public Action<string> OnTextChanged { get; set; }

  public bool ForceUpperCase { get; set; } = false;

  public bool DisallowSpaces { get; set; } = false;

  public static bool AnyFocused => TextFieldMenuItem._focused != null;

  public bool IsFocused => TextFieldMenuItem._focused == this;

  public TextFieldMenuItem() => this.Height = MenuConfig.DefaultItemHeight * 2.5f;

  private static void EnsureSubscribed()
  {
    Keyboard current = Keyboard.current;
    if (current == TextFieldMenuItem._subscribedKb)
      return;
    if (TextFieldMenuItem._subscribedKb != null)
    {
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      TextFieldMenuItem._subscribedKb.onTextInput -= new Action<char>(TextFieldMenuItem.OnTextInput);
    }
    TextFieldMenuItem._subscribedKb = current;
    if (current == null)
      return;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    current.onTextInput += new Action<char>(TextFieldMenuItem.OnTextInput);
  }

  private static void OnTextInput(char c)
  {
    TextFieldMenuItem focused = TextFieldMenuItem._focused;
    if (focused == null)
      return;
    Keyboard current = Keyboard.current;
    if ((current == null ? 0 : (((ButtonControl) current.leftCtrlKey).isPressed || ((ButtonControl) current.rightCtrlKey).isPressed || ((ButtonControl) current.leftCommandKey).isPressed ? 1 : (((ButtonControl) current.rightCommandKey).isPressed ? 1 : 0))) != 0 || (c < ' ' ? 1 : (c == '\u007F' ? 1 : 0)) != 0 || (!focused.DisallowSpaces ? 0 : (c == ' ' ? 1 : 0)) != 0)
      return;
    if (focused.ForceUpperCase)
      c = char.ToUpperInvariant(c);
    focused.Insert(c);
  }

  private void Insert(char c)
  {
    string str = this.Text ?? "";
    if (this._selectAll)
    {
      str = "";
      this._caret = 0;
      this._selectAll = false;
    }
    if ((this.MaxLength <= 0 ? 0 : (str.Length >= this.MaxLength ? 1 : 0)) != 0)
      return;
    this._caret = Mathf.Clamp(this._caret, 0, str.Length);
    this.Text = str.Substring(0, this._caret) + c.ToString() + str.Substring(this._caret);
    ++this._caret;
    Action<string> onTextChanged = this.OnTextChanged;
    if (onTextChanged == null)
      return;
    onTextChanged(this.Text);
  }

  private void Focus()
  {
    TextFieldMenuItem._focused = this;
    TextFieldMenuItem.EnsureSubscribed();
  }

  private void Blur()
  {
    if (TextFieldMenuItem._focused == this)
      TextFieldMenuItem._focused = (TextFieldMenuItem) null;
    this._repeatKey = (KeyControl) null;
    this._repeatAction = (Action) null;
    this._selectAll = false;
  }

  public override bool Draw(Rect rect)
  {
    TextFieldMenuItem.EnsureSubscribed();
    Rect rect1;
    // ISSUE: explicit constructor call
    rect1 = new Rect(rect.x, rect.y + 5f, rect.width, MenuConfig.DefaultItemHeight);
    GUI.Label(rect1, this.Label, MenuConfig.GetLabelStyle());
    Rect rect2;
    // ISSUE: explicit constructor call
    rect2 = new Rect(rect.x + 10f, (float) ((double) rect.y + (double) rect1.height + 10.0), rect.width - 20f, MenuConfig.DefaultItemHeight);
    GUIStyle transparentStyle = MenuConfig.GetTextFieldTransparentStyle();
    string str = this.Text ?? "";
    this._caret = Mathf.Clamp(this._caret, 0, str.Length);
    if ((!this.Enabled || !this.IsFocused ? 0 : (Event.current.type == (EventType) 8 ? 1 : 0)) != 0)
      this.ProcessEditingKeys();
    Event current = Event.current;
    if ((!this.Enabled ? 0 : (current.type == 0 ? 1 : 0)) != 0)
    {
      if (rect2.Contains(current.mousePosition))
      {
        this.Focus();
        this._selectAll = false;
        try
        {
          this._caret = transparentStyle.GetCursorStringIndex(rect2, new GUIContent(str), current.mousePosition);
        }
        catch
        {
          this._caret = str.Length;
        }
        this._caret = Mathf.Clamp(this._caret, 0, str.Length);
      }
      else if (this.IsFocused)
        this.Blur();
    }
    if (current.type == (EventType) 7)
    {
      bool isFocused;
      Color color1 = (isFocused = this.IsFocused) ? MenuConfig.HoverColorNow : MenuConfig.OutlineColorNow;
      GUI.DrawTexture(rect2, (Texture) Texture2D.whiteTexture, (ScaleMode) 0, true, 0.0f, color1, 0.0f, 6f);
      Rect rect3;
      // ISSUE: explicit constructor call
      rect3 = new Rect(rect2.x + 1.5f, rect2.y + 1.5f, rect2.width - 3f, rect2.height - 3f);
      GUI.DrawTexture(rect3, (Texture) Texture2D.whiteTexture, (ScaleMode) 0, true, 0.0f, MenuConfig.SliderBgColor, 0.0f, 5f);
      if ((!isFocused || !this._selectAll ? 0 : (str.Length > 0 ? 1 : 0)) != 0)
      {
        Vector2 cursorPixelPosition1;
        Vector2 cursorPixelPosition2;
        try
        {
          cursorPixelPosition1 = transparentStyle.GetCursorPixelPosition(rect2, new GUIContent(str), 0);
          cursorPixelPosition2 = transparentStyle.GetCursorPixelPosition(rect2, new GUIContent(str), str.Length);
        }
        catch
        {
          // ISSUE: explicit constructor call
          cursorPixelPosition1 = new Vector2(rect2.x + 4f, rect2.y);
          // ISSUE: explicit constructor call
          cursorPixelPosition2 = new Vector2(rect2.xMax - 4f, rect2.y);
        }
        float num = MenuConfig.DefaultItemHeight - 10f;
        Rect rect4;
        // ISSUE: explicit constructor call
        rect4 = new Rect(cursorPixelPosition1.x, rect2.y + (float) (((double) rect2.height - (double) num) * 0.5), Mathf.Max(2f, cursorPixelPosition2.x - cursorPixelPosition1.x), num);
        Color color2 = GUI.color;
        GUI.color = MenuConfig.SelectionColorNow;
        GUI.DrawTexture(rect4, (Texture) Texture2D.whiteTexture);
        GUI.color = color2;
      }
      GUI.Label(rect2, str, transparentStyle);
      if (isFocused)
      {
        Vector2 cursorPixelPosition;
        try
        {
          cursorPixelPosition = transparentStyle.GetCursorPixelPosition(rect2, new GUIContent(str), this._caret);
        }
        catch
        {
          // ISSUE: explicit constructor call
          cursorPixelPosition = new Vector2(rect2.x + 4f, rect2.y);
        }
        float num = MenuConfig.DefaultItemHeight - 10f;
        Rect rect5;
        // ISSUE: explicit constructor call
        rect5 = new Rect(cursorPixelPosition.x, rect2.y + (float) (((double) rect2.height - (double) num) * 0.5), 1.5f, num);
        Color color3 = GUI.color;
        GUI.color = MenuConfig.CaretColorNow;
        GUI.DrawTexture(rect5, (Texture) Texture2D.whiteTexture);
        GUI.color = color3;
      }
    }
    this.DrawDescription(rect);
    return false;
  }

  private void ProcessEditingKeys()
  {
    Keyboard current = Keyboard.current;
    if (current == null)
      return;
    this._caret = Mathf.Clamp(this._caret, 0, (this.Text ?? "").Length);
    if ((((ButtonControl) current.leftCtrlKey).isPressed || ((ButtonControl) current.rightCtrlKey).isPressed || ((ButtonControl) current.leftCommandKey).isPressed ? 1 : (((ButtonControl) current.rightCommandKey).isPressed ? 1 : 0)) != 0)
    {
      if (((ButtonControl) current.vKey).wasPressedThisFrame)
      {
        this.Paste();
        return;
      }
      if (((ButtonControl) current.cKey).wasPressedThisFrame)
      {
        GUIUtility.systemCopyBuffer = this.Text ?? "";
        return;
      }
      if (!((ButtonControl) current.xKey).wasPressedThisFrame)
      {
        if (((ButtonControl) current.aKey).wasPressedThisFrame)
        {
          this._selectAll = (this.Text ?? "").Length > 0;
          this._caret = (this.Text ?? "").Length;
          return;
        }
      }
      else
      {
        GUIUtility.systemCopyBuffer = this.Text ?? "";
        this.Text = "";
        this._caret = 0;
        this._selectAll = false;
        Action<string> onTextChanged = this.OnTextChanged;
        if (onTextChanged == null)
          return;
        onTextChanged(this.Text);
        return;
      }
    }
    this.TryRepeatable(current.backspaceKey, new Action(this.Backspace));
    this.TryRepeatable(current.deleteKey, new Action(this.DeleteForward));
    this.TryRepeatable(current.leftArrowKey, (Action) (() =>
    {
      if (this._selectAll)
      {
        this._selectAll = false;
        this._caret = 0;
      }
      else
        this._caret = Mathf.Max(0, this._caret - 1);
    }));
    this.TryRepeatable(current.rightArrowKey, (Action) (() =>
    {
      if (!this._selectAll)
      {
        this._caret = Mathf.Min((this.Text ?? "").Length, this._caret + 1);
      }
      else
      {
        this._selectAll = false;
        this._caret = (this.Text ?? "").Length;
      }
    }));
    if ((current.homeKey == null ? 0 : (((ButtonControl) current.homeKey).wasPressedThisFrame ? 1 : 0)) != 0)
    {
      this._caret = 0;
      this._selectAll = false;
    }
    if ((current.endKey == null ? 0 : (((ButtonControl) current.endKey).wasPressedThisFrame ? 1 : 0)) != 0)
    {
      this._caret = (this.Text ?? "").Length;
      this._selectAll = false;
    }
    if ((current.enterKey != null && ((ButtonControl) current.enterKey).wasPressedThisFrame || current.numpadEnterKey != null && ((ButtonControl) current.numpadEnterKey).wasPressedThisFrame ? 1 : (current.escapeKey == null ? 0 : (((ButtonControl) current.escapeKey).wasPressedThisFrame ? 1 : 0))) == 0)
    {
      if (this._repeatKey == null)
        return;
      if (!((ButtonControl) this._repeatKey).isPressed)
      {
        this._repeatKey = (KeyControl) null;
        this._repeatAction = (Action) null;
      }
      else
      {
        this._repeatTimer -= Time.unscaledDeltaTime;
        for (int index = 0; ((double) this._repeatTimer > 0.0 ? 0 : (index++ < 10 ? 1 : 0)) != 0; this._repeatTimer += 0.045f)
        {
          Action repeatAction = this._repeatAction;
          if (repeatAction != null)
            repeatAction();
        }
      }
    }
    else
      this.Blur();
  }

  private void TryRepeatable(KeyControl key, Action action)
  {
    if ((key == null ? 0 : (((ButtonControl) key).wasPressedThisFrame ? 1 : 0)) == 0)
      return;
    action();
    this._repeatKey = key;
    this._repeatAction = action;
    this._repeatTimer = 0.4f;
  }

  private void Backspace()
  {
    if (!this._selectAll)
    {
      string str = this.Text ?? "";
      this._caret = Mathf.Clamp(this._caret, 0, str.Length);
      if (this._caret <= 0)
        return;
      this.Text = str.Substring(0, this._caret - 1) + str.Substring(this._caret);
      --this._caret;
      Action<string> onTextChanged = this.OnTextChanged;
      if (onTextChanged == null)
        return;
      onTextChanged(this.Text);
    }
    else
      this.ClearSelection();
  }

  private void DeleteForward()
  {
    if (this._selectAll)
    {
      this.ClearSelection();
    }
    else
    {
      string str = this.Text ?? "";
      this._caret = Mathf.Clamp(this._caret, 0, str.Length);
      if (this._caret >= str.Length)
        return;
      this.Text = str.Substring(0, this._caret) + str.Substring(this._caret + 1);
      Action<string> onTextChanged = this.OnTextChanged;
      if (onTextChanged == null)
        return;
      onTextChanged(this.Text);
    }
  }

  private void ClearSelection()
  {
    this._selectAll = false;
    this._caret = 0;
    if (string.IsNullOrEmpty(this.Text))
      return;
    this.Text = "";
    Action<string> onTextChanged = this.OnTextChanged;
    if (onTextChanged == null)
      return;
    onTextChanged(this.Text);
  }

  private void Paste()
  {
    string systemCopyBuffer = GUIUtility.systemCopyBuffer;
    if (string.IsNullOrEmpty(systemCopyBuffer))
      return;
    foreach (char c in systemCopyBuffer)
    {
      if ((c < ' ' ? 1 : (c == '\u007F' ? 1 : 0)) == 0 && (!this.DisallowSpaces ? 0 : (c == ' ' ? 1 : 0)) == 0)
        this.Insert(this.ForceUpperCase ? char.ToUpperInvariant(c) : c);
    }
  }
}
