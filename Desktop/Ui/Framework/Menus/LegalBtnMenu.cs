using SakuraaCastingMod.Core;
using SakuraaCastingMod.Features.Tools;
using System;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Desktop.Ui.Framework.Menus;

public static class LegalBtnMenu
{
  public static MenuBuilder _legalMenu;

  public static void Draw()
  {
    if (!LegalChecker.LegalVisible)
      return;
    if (LegalBtnMenu._legalMenu == null)
      LegalBtnMenu.Initialize();
    LegalBtnMenu._legalMenu?.Draw();
  }

  public static void Initialize()
  {
    LegalBtnMenu._legalMenu = new MenuBuilder("Legal Agreement", 170f).SetPosition((float) ((double) Screen.width / 2.0 - 85.0), (float) ((double) Screen.height / 2.0 - 30.0)).AddSpace(15f).AddButton("Accept TOS", (Action) (() =>
    {
      try
      {
        GameObject.Find("Miscellaneous Scripts/LegalAgreementCheck")?.SetActive(false);
      }
      catch
      {
      }
      try
      {
        GameObject.Find("Miscellaneous Scripts/PrivateUIRoom")?.SetActive(false);
      }
      catch
      {
      }
      try
      {
        GameObject.Find("Miscellaneous Scripts/MetaReporting")?.SetActive(false);
      }
      catch
      {
      }
      if (!((UnityEngine.Object) Plugin.Ins != (UnityEngine.Object) null))
        return;
      LegalChecker.LegalVisible = false;
    }), "Hides/accepts the annoying TOS board blocking your view >:(");
    LegalBtnMenu._legalMenu.MenuRect.height = (float) ((double) MenuConfig.HeaderHeight + (double) MenuConfig.DefaultItemHeight + (double) (MenuConfig.VerticalPadding * 2) + 5.0);
  }
}
