using SakuraaCastingMod.Shared.Helpers;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Features.Tools;

public static class LegalChecker
{
  [SavedSetting("LegalAgreementsCheckVisible", false)]
  public static bool LegalVisible;

  public static void CheckLegalVisibility()
  {
    try
    {
      GameObject gameObject1 = GameObject.Find("Miscellaneous Scripts/LegalAgreementCheck");
      GameObject gameObject2 = GameObject.Find("Miscellaneous Scripts/PrivateUIRoom");
      LegalChecker.LegalVisible = gameObject1.activeSelf && gameObject2.activeSelf;
    }
    catch
    {
      LegalChecker.LegalVisible = false;
    }
  }
}
