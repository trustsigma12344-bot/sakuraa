using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.Shared.Helpers;

public static class CosmeticProfileStore
{
  private const string ProfileFileName = "Sak_CosmeticProfiles.json";

  private static string ProfilePath
  {
    get => Path.Combine(Application.persistentDataPath, "Sak_CosmeticProfiles.json");
  }

  public static List<CosmeticProfile> LoadProfiles()
  {
    try
    {
      string profilePath = CosmeticProfileStore.ProfilePath;
      if (!File.Exists(profilePath))
        return new List<CosmeticProfile>();
      ProfileContainer profileContainer = JsonConvert.DeserializeObject<ProfileContainer>(File.ReadAllText(profilePath));
      List<CosmeticProfile> cosmeticProfileList;
      if (profileContainer == null)
      {
        cosmeticProfileList = (List<CosmeticProfile>) null;
      }
      else
      {
        cosmeticProfileList = profileContainer.Profiles;
        if (cosmeticProfileList != null)
          goto label_6;
      }
      cosmeticProfileList = new List<CosmeticProfile>();
label_6:
      return cosmeticProfileList;
    }
    catch
    {
      return new List<CosmeticProfile>();
    }
  }

  public static void SaveProfiles(List<CosmeticProfile> profiles)
  {
    File.WriteAllText(CosmeticProfileStore.ProfilePath, JsonConvert.SerializeObject((object) new ProfileContainer()
    {
      Profiles = (profiles ?? new List<CosmeticProfile>())
    }, (Formatting) 1));
  }
}
