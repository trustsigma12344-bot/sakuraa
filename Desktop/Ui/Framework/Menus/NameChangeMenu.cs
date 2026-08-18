using SakuraaCastingMod.Desktop.Ui.Framework.MenuItems;
using SakuraaCastingMod.Features.Tools;
using System;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class NameChangeMenu
{
  public static MenuBuilder _nameChangeMenu;

  public static void Draw()
  {
    if (NameChangeMenu._nameChangeMenu == null)
    {
      NameChangeMenu.Initialize();
      TextFieldMenuItem textFieldMenuItem = default;
      int num;
      if (NameChangeMenu._nameChangeMenu != null && NameChangeMenu._nameChangeMenu.Items.Count > 0)
      {
        textFieldMenuItem = NameChangeMenu._nameChangeMenu.Items[0] as TextFieldMenuItem;
        num = textFieldMenuItem != null ? 1 : 0;
      }
      else
        num = 0;
      if (num != 0)
        textFieldMenuItem.Text = Character.GetCurrentName() ?? "";
    }
    if (NameChangeMenu._nameChangeMenu == null)
      return;
    NameChangeMenu._nameChangeMenu.Draw();
  }

  public static void Initialize()
  {
    TextFieldMenuItem textFieldMenuItem1 = new TextFieldMenuItem();
    textFieldMenuItem1.Text = Character.GetCurrentName() ?? "";
    textFieldMenuItem1.MaxLength = 12;
    textFieldMenuItem1.ForceUpperCase = true;
    textFieldMenuItem1.Description = "Enter the desired name";
    TextFieldMenuItem textFieldMenuItem2 = textFieldMenuItem1;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    textFieldMenuItem2.OnTextChanged = new Action<string>(Character.SetNewName);
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    NameChangeMenu._nameChangeMenu = new MenuBuilder("Name Changer", 200f).AddItem((MenuItem) textFieldMenuItem2).AddButton("Update", new Action(Character.TriggerUpdateName), "Set the new name");
  }
}
