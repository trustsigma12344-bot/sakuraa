using GorillaLocomotion;
using SakuraaCastingMod.Core;
using SakuraaCastingMod.Features.Overlays;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.Shared.Models;
using SakuraaCastingMod.VR.UtilMenu.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Pages;

public class NotificationsPage : BasePage
{
  [SavedSetting("FriendNotificationsEnabled", true)]
  public static bool FriendNotificationsEnabled = true;
  [SavedSetting("SpecialCosmeticAlerts", true)]
  private static bool _specialCosmeticAlerts = true;
  [SavedSetting("CurrentAlertMode", NotificationsPage.AlertMode.All)]
  private static NotificationsPage.AlertMode _currentAlertMode = NotificationsPage.AlertMode.All;
  [SavedSetting("SpeedometerEnabled", false)]
  private static bool _speedometerEnabled = false;
  private static HashSet<string> _notifiedPlayers = new HashSet<string>();
  private static HashSet<string> _notifiedSpecialPlayers = new HashSet<string>();
  private List<NotificationModel> _notificationHistory = new List<NotificationModel>();
  private int _historyPageOffset = 0;
  private NotificationModel _selectedNotification;
  private UtilTab _settingsTab;
  private UtilTab _historyTab;
  private GameObject _speedometerObj;
  private TextMesh _speedometerText;
  private float _displayedSpeed = 0.0f;
  private static Coroutine _modderRoutine;
  private static Coroutine _specialRoutine;
  private static Coroutine _speedometerRoutine;
  private float _nextLiveCleanup = 0.0f;
  private const float LiveCleanupInterval = 2f;

  public override string PageName => "NOTIFICATIONS";

  public override Material PageIcon => UtilMenuMain.Instance.Icons.RingingBell;

  public override void Start()
  {
    // ISSUE: explicit non-virtual call
    base.Start();
    if (NotificationsPage._modderRoutine != null)
      UtilMenuMain.Instance.StopCoroutine(NotificationsPage._modderRoutine);
    NotificationsPage._modderRoutine = UtilMenuMain.Instance.StartCoroutine(this.CheckModdersRoutine());
    if (UtilMenuMain.Instance.specialVariant == 1)
    {
      if (NotificationsPage._specialRoutine != null)
        UtilMenuMain.Instance.StopCoroutine(NotificationsPage._specialRoutine);
      NotificationsPage._specialRoutine = UtilMenuMain.Instance.StartCoroutine(this.CheckSCRoutine());
    }
    GameObject gameObject = GameObject.Find("SakSpeedometer");
    if (((UnityEngine.Object) gameObject))
      UnityEngine.Object.Destroy((UnityEngine.Object) gameObject);
    if (NotificationsPage._speedometerRoutine != null)
      UtilMenuMain.Instance.StopCoroutine(NotificationsPage._speedometerRoutine);
    NotificationsPage._speedometerRoutine = UtilMenuMain.Instance.StartCoroutine(this.SpeedometerRoutine());
  }

  public override void LateUpdate()
  {
    // ISSUE: explicit non-virtual call
    base.LateUpdate();
    if (!((UnityEngine.Object) FriendNetworkController.Instance))
      return;
    bool flag;
    if ((!(flag = ((UnityEngine.Object) UtilMenuController.Instance != (UnityEngine.Object) null) && UtilMenuController.Instance.CurrentTabIndex == 0) ? 0 : ((double) Time.unscaledTime >= (double) this._nextLiveCleanup ? 1 : 0)) != 0)
    {
      this._nextLiveCleanup = Time.unscaledTime + 2f;
      FriendNetworkController.Instance.CleanupExpired();
    }
    if (!FriendNetworkController.Instance.UI_NewNotification)
      return;
    FriendNetworkController.Instance.UI_NewNotification = false;
    if (!flag)
      return;
    if ((this._selectedNotification == null ? 0 : (!FriendNetworkController.Instance._notifications.Contains(this._selectedNotification) ? 1 : 0)) != 0)
      this._selectedNotification = (NotificationModel) null;
    this.RefreshHistoryTab();
    UtilMenuController.Instance.RefreshUI();
  }

  public override void BuildTabs()
  {
    this._historyTab = new UtilTab()
    {
      TabIcon = UtilMenuMain.Instance.Icons.RingingBell,
      TabName = "History"
    };
    this.Tabs.Add(this._historyTab);
    this._settingsTab = new UtilTab()
    {
      TabIcon = UtilMenuMain.Instance.Icons.FileSettingsIcon,
      TabName = "Settings"
    };
    this.Tabs.Add(this._settingsTab);
    this.RefreshHistoryTab();
    this.UpdateSettingsTab();
  }

  public override void OnTabSelected(int tabIndex)
  {
    switch (tabIndex)
    {
      case 0:
        this._selectedNotification = (NotificationModel) null;
        if (((UnityEngine.Object) FriendNetworkController.Instance))
          FriendNetworkController.Instance.CleanupExpired();
        this.RefreshHistoryTab();
        break;
      case 1:
        this.UpdateSettingsTab();
        break;
    }
  }

  private void RefreshHistoryTab()
  {
    if (this._historyTab == null)
      return;
    this._historyTab.Elements.Clear();
    this._historyTab.Description = (string) null;
    this._notificationHistory = ((UnityEngine.Object) FriendNetworkController.Instance != (UnityEngine.Object) null) ? FriendNetworkController.Instance.GetNotificationHistory() : new List<NotificationModel>();
    if (this._selectedNotification == null)
    {
      if ((this._notificationHistory == null ? 1 : (this._notificationHistory.Count == 0 ? 1 : 0)) != 0)
      {
        this._historyTab.Elements.Add(new MenuElement("NO NOTIFICATIONS", (Action) (() => { })));
      }
      else
      {
        this._historyTab.Elements.Add(new MenuElement("CLEAR ALL", (Action) (() =>
        {
          if (((UnityEngine.Object) FriendNetworkController.Instance))
            FriendNetworkController.Instance.ClearHistory();
          this.RefreshHistoryTab();
          UtilMenuController.Instance.RefreshUI();
        })));
        int totalPages = Mathf.CeilToInt((float) this._notificationHistory.Count / 3f);
        if (this._historyPageOffset >= totalPages)
          this._historyPageOffset = 0;
        int num1 = this._historyPageOffset * 3;
        int num2 = Mathf.Min(3, this._notificationHistory.Count - num1);
        for (int index = 0; index < num2; ++index)
        {
          NotificationModel notif = this._notificationHistory[num1 + index];
          string text = notif.Title.ToUpper();
          if (text.Length > 16 /*0x10*/)
            text = text.Substring(0, 16 /*0x10*/);
          string type = notif.Type;
          if (type == "PARTY_INVITE" || type == "FRIEND_REQ" || type == "GAME_INVITE" || type == "PARTY_APPROVAL" || type == "CONFIG_RECEIVE")
            text = $"<color=red>{text}</color>";
          this._historyTab.Elements.Add(new MenuElement(text, (Action) (() =>
          {
            this._selectedNotification = notif;
            this.RefreshHistoryTab();
            UtilMenuController.Instance.RefreshUI();
          })));
        }
        if (totalPages <= 1)
          return;
        this._historyTab.Elements.Add(new MenuElement($"PAGE {this._historyPageOffset + 1}/{totalPages}", "", (Action) (() =>
        {
          --this._historyPageOffset;
          if (this._historyPageOffset < 0)
            this._historyPageOffset = totalPages - 1;
          this.RefreshHistoryTab();
          UtilMenuController.Instance.RefreshUI();
        }), (Action) (() =>
        {
          ++this._historyPageOffset;
          if (this._historyPageOffset >= totalPages)
            this._historyPageOffset = 0;
          this.RefreshHistoryTab();
          UtilMenuController.Instance.RefreshUI();
        }))
        {
          Type = ElementType.Slider
        });
      }
    }
    else
    {
      string text = "FROM: " + this._selectedNotification.Title;
      string description = this._selectedNotification.Description;
      this._historyTab.Elements.Add(new MenuElement(text, (Action) (() => { })));
      this._historyTab.Elements.Add(new MenuElement(description, (Action) (() => { })));
      this._historyTab.Elements.Add(new MenuElement("", (Action) (() => { })));
      this._historyTab.Elements.Add(new MenuElement("", (Action) (() => { })));
      string type = this._selectedNotification.Type;
      bool isActionable = type == "PARTY_INVITE" || type == "FRIEND_REQ" || type == "GAME_INVITE" || type == "PARTY_APPROVAL" || type == "CONFIG_RECEIVE";
      string initialValue = "DELETE (X) | OK (V)";
      if (isActionable)
        initialValue = !(this._selectedNotification.Type == "CONFIG_RECEIVE") ? "DECLINE (X) | ACCEPT (V)" : "DENY (X) | IMPORT (V)";
      this._historyTab.Elements.Add(new MenuElement("ACTION", initialValue, (Action) (() =>
      {
        if (((UnityEngine.Object) FriendNetworkController.Instance))
          FriendNetworkController.Instance.RemoveNotification(this._selectedNotification);
        this._selectedNotification = (NotificationModel) null;
        this.RefreshHistoryTab();
        UtilMenuController.Instance.RefreshUI();
      }), (Action) (() =>
      {
        if ((!isActionable ? 0 : (((UnityEngine.Object) FriendNetworkController.Instance) ? 1 : 0)) != 0)
          FriendNetworkController.Instance.HandleNotificationAction(this._selectedNotification);
        else if (((UnityEngine.Object) FriendNetworkController.Instance))
          FriendNetworkController.Instance.RemoveNotification(this._selectedNotification);
        this._selectedNotification = (NotificationModel) null;
        this.RefreshHistoryTab();
        UtilMenuController.Instance.RefreshUI();
      }))
      {
        Type = ElementType.Slider
      });
    }
  }

  private void UpdateSettingsTab()
  {
    if (this._settingsTab == null)
      return;
    this._settingsTab.Elements.Clear();
    this._settingsTab.Elements.Add(new MenuElement("DURATION", $"{Notification.VrDecayTime:F1}s", (Action) (() =>
    {
      Notification.VrDecayTime = Mathf.Max(1f, Notification.VrDecayTime - 0.5f);
      NotificationsPage.SaveSettings();
      this.UpdateSettingsTab();
      UtilMenuController.Instance.RefreshUI();
    }), (Action) (() =>
    {
      Notification.VrDecayTime = Mathf.Min(10f, Notification.VrDecayTime + 0.5f);
      NotificationsPage.SaveSettings();
      this.UpdateSettingsTab();
      UtilMenuController.Instance.RefreshUI();
    })));
    this._settingsTab.Elements.Add(new MenuElement("SIZE", Notification.VrFontSize.ToString(), (Action) (() =>
    {
      Notification.SetFontSize(Mathf.Max(10, Notification.VrFontSize - 5));
      NotificationsPage.SaveSettings();
      this.UpdateSettingsTab();
      UtilMenuController.Instance.RefreshUI();
    }), (Action) (() =>
    {
      Notification.SetFontSize(Mathf.Min(120, Notification.VrFontSize + 5));
      NotificationsPage.SaveSettings();
      this.UpdateSettingsTab();
      UtilMenuController.Instance.RefreshUI();
    })));
    this._settingsTab.Elements.Add(new MenuElement("MOD ALERTS: " + NotificationsPage._currentAlertMode.ToString().ToUpper(), (Action) (() =>
    {
      NotificationsPage._currentAlertMode = (NotificationsPage.AlertMode) ((int) (NotificationsPage._currentAlertMode + 1) % 4);
      if (NotificationsPage._currentAlertMode == NotificationsPage.AlertMode.Off)
        NotificationsPage._notifiedPlayers.Clear();
      NotificationsPage.SaveSettings();
      this.UpdateSettingsTab();
      UtilMenuController.Instance.RefreshUI();
    })));
    this._settingsTab.Elements.Add(new MenuElement("FRIEND ALERTS: " + (NotificationsPage.FriendNotificationsEnabled ? "ON" : "OFF"), (Action) (() =>
    {
      NotificationsPage.FriendNotificationsEnabled = !NotificationsPage.FriendNotificationsEnabled;
      NotificationsPage.SaveSettings();
      this.UpdateSettingsTab();
      UtilMenuController.Instance.RefreshUI();
    }), NotificationsPage.FriendNotificationsEnabled)
    {
      Type = ElementType.Toggle
    });
    if (UtilMenuMain.Instance.specialVariant == 1)
      this._settingsTab.Elements.Add(new MenuElement("SPECIAL ALERTS: " + (NotificationsPage._specialCosmeticAlerts ? "ON" : "OFF"), (Action) (() =>
      {
        NotificationsPage._specialCosmeticAlerts = !NotificationsPage._specialCosmeticAlerts;
        if (!NotificationsPage._specialCosmeticAlerts)
          NotificationsPage._notifiedSpecialPlayers.Clear();
        NotificationsPage.SaveSettings();
        this.UpdateSettingsTab();
        UtilMenuController.Instance.RefreshUI();
      }), NotificationsPage._specialCosmeticAlerts)
      {
        Type = ElementType.Toggle
      });
    this._settingsTab.Elements.Add(new MenuElement("SPEEDOMETER: " + (NotificationsPage._speedometerEnabled ? "ON" : "OFF"), (Action) (() =>
    {
      NotificationsPage._speedometerEnabled = !NotificationsPage._speedometerEnabled;
      if ((NotificationsPage._speedometerEnabled ? 0 : (((UnityEngine.Object) this._speedometerObj != (UnityEngine.Object) null) ? 1 : 0)) != 0)
        this._speedometerObj.SetActive(false);
      NotificationsPage.SaveSettings();
      this.UpdateSettingsTab();
      UtilMenuController.Instance.RefreshUI();
    }), NotificationsPage._speedometerEnabled)
    {
      Type = ElementType.Toggle
    });
  }

  public static void SaveSettings() => Configuration.SaveSettings();

  private IEnumerator CheckModdersRoutine()
  {
    yield return (object) new WaitForSeconds(1f);
    while (true)
    {
      this.CheckModders();
      yield return (object) new WaitForSeconds(3f);
    }
  }

  private void CheckModders()
  {
    if (NotificationsPage._currentAlertMode == NotificationsPage.AlertMode.Off)
      return;
    if ((((UnityEngine.Object) NetworkSystem.Instance == (UnityEngine.Object) null) ? 1 : (!NetworkSystem.Instance.InRoom ? 1 : 0)) != 0)
    {
      NotificationsPage._notifiedPlayers.Clear();
    }
    else
    {
      HashSet<string> currentRoomPlayers = new HashSet<string>();
      foreach (NetPlayer allNetPlayer in NetworkSystem.Instance.AllNetPlayers)
      {
        if ((allNetPlayer == null ? 1 : (string.IsNullOrEmpty(allNetPlayer.UserId) ? 1 : 0)) == 0)
        {
          currentRoomPlayers.Add(allNetPlayer.UserId);
          if ((NotificationsPage._notifiedPlayers.Contains(allNetPlayer.UserId) ? 1 : (allNetPlayer.IsLocal ? 1 : 0)) == 0)
          {
            VRRig rigByNetPlayer = PlayerTranslator.GetRigByNetPlayer(allNetPlayer);
            if ((((UnityEngine.Object) rigByNetPlayer == (UnityEngine.Object) null) || rigByNetPlayer.Creator == null ? 1 : (rigByNetPlayer.Creator.UserId != allNetPlayer.UserId ? 1 : 0)) == 0)
            {
              List<string> list = LobbyPage.GetDetectedMods(rigByNetPlayer).ToList<string>();
              int count = list.Count;
              if (count > 0)
              {
                bool flag1 = list.Any<string>((Func<string, bool>) (m => m.Contains("red")));
                bool flag2 = list.Any<string>((Func<string, bool>) (m => !m.Contains("red")));
                bool flag3 = false;
                switch (NotificationsPage._currentAlertMode)
                {
                  case NotificationsPage.AlertMode.Mods:
                    flag3 = flag2;
                    break;
                  case NotificationsPage.AlertMode.Cheats:
                    flag3 = flag1;
                    break;
                  case NotificationsPage.AlertMode.All:
                    flag3 = true;
                    break;
                }
                if (flag3)
                {
                  string str = count == 1 ? list[0] : $"{count} MODS";
                  Notification.Send($"{(flag1 ? "<color=red>[CHEATER]</color>" : "<color=cyan>[MODDER]</color>")} {allNetPlayer.NickName} ({str})", flag1 ? Color.red : Color.cyan);
                  if (((UnityEngine.Object) FriendNetworkController.Instance != (UnityEngine.Object) null))
                    FriendNetworkController.Instance.AddNotification(new NotificationModel()
                    {
                      Title = flag1 ? "CHEATER" : "MODDER",
                      Description = $"{allNetPlayer.NickName} - {str}",
                      Type = flag1 ? "CHEATER_ALERT" : "MODDER_ALERT",
                      PayloadJson = allNetPlayer.UserId
                    }, false);
                  NotificationsPage._notifiedPlayers.Add(allNetPlayer.UserId);
                }
              }
            }
          }
        }
      }
      NotificationsPage._notifiedPlayers.RemoveWhere((Predicate<string>) (id => !currentRoomPlayers.Contains(id)));
    }
  }

  private IEnumerator CheckSCRoutine()
  {
    yield return (object) new WaitForSeconds(1f);
    while (true)
    {
      if (UtilMenuMain.Instance.specialVariant == 1)
        this.CheckSC();
      yield return (object) new WaitForSeconds(3f);
    }
  }

  private void CheckSC()
  {
    if (UtilMenuMain.Instance.specialVariant != 1 || !NotificationsPage._specialCosmeticAlerts)
      return;
    if ((((UnityEngine.Object) NetworkSystem.Instance == (UnityEngine.Object) null) ? 1 : (!NetworkSystem.Instance.InRoom ? 1 : 0)) == 0)
    {
      HashSet<string> currentRoomPlayers = new HashSet<string>();
      foreach (NetPlayer allNetPlayer in NetworkSystem.Instance.AllNetPlayers)
      {
        if ((allNetPlayer == null ? 1 : (string.IsNullOrEmpty(allNetPlayer.UserId) ? 1 : 0)) == 0)
        {
          currentRoomPlayers.Add(allNetPlayer.UserId);
          if ((NotificationsPage._notifiedSpecialPlayers.Contains(allNetPlayer.UserId) ? 1 : (allNetPlayer.IsLocal ? 1 : 0)) == 0)
          {
            VRRig rigByNetPlayer = PlayerTranslator.GetRigByNetPlayer(allNetPlayer);
            if ((!((UnityEngine.Object) rigByNetPlayer != (UnityEngine.Object) null) || rigByNetPlayer.Creator == null ? 0 : (rigByNetPlayer.Creator.UserId == allNetPlayer.UserId ? 1 : 0)) != 0)
            {
              List<string> specialCosmetics = LobbyPage.GetSpecialCosmetics(rigByNetPlayer);
              if (specialCosmetics.Count > 0)
              {
                string str = string.Join(", ", (IEnumerable<string>) specialCosmetics);
                Notification.Send($"<color=cyan>[SPECIAL]</color> {allNetPlayer.NickName} ({str})", Color.cyan);
                if (((UnityEngine.Object) FriendNetworkController.Instance != (UnityEngine.Object) null))
                  FriendNetworkController.Instance.AddNotification(new NotificationModel()
                  {
                    Title = "SPECIAL",
                    Description = $"{allNetPlayer.NickName}, {str}",
                    Type = "SPECIAL_ALERT",
                    PayloadJson = allNetPlayer.UserId
                  }, false);
                NotificationsPage._notifiedSpecialPlayers.Add(allNetPlayer.UserId);
              }
            }
          }
        }
      }
      NotificationsPage._notifiedSpecialPlayers.RemoveWhere((Predicate<string>) (id => !currentRoomPlayers.Contains(id)));
    }
    else
      NotificationsPage._notifiedSpecialPlayers.Clear();
  }

  private IEnumerator SpeedometerRoutine()
  {
    while (true)
    {
      this.UpdateSpeedometer();
      yield return (object) null;
    }
  }

  private void UpdateSpeedometer()
  {
    if (!NotificationsPage._speedometerEnabled)
      return;
    if (((UnityEngine.Object) this._speedometerObj == (UnityEngine.Object) null))
    {
      this._speedometerObj = new GameObject("SakSpeedometer");
      this._speedometerText = this._speedometerObj.AddComponent<TextMesh>();
      this._speedometerText.fontSize = 24;
      this._speedometerText.color = Color.cyan;
      this._speedometerText.anchor = (TextAnchor) 2;
      this._speedometerText.alignment = (TextAlignment) 2;
      this._speedometerObj.transform.localScale = (Vector3.one * 0.01f);
    }
    if (!this._speedometerObj.activeSelf)
      this._speedometerObj.SetActive(true);
    Transform transform = ((UnityEngine.Object) Camera.main) ? ((Component) Camera.main).transform : (Transform) null;
    if (((UnityEngine.Object) transform == (UnityEngine.Object) null))
      return;
    this._speedometerObj.transform.position = (((transform.position + (transform.forward * 1f)) + (transform.right * 0.5f)) + (transform.up * 0.4f));
    this._speedometerObj.transform.LookAt(transform);
    this._speedometerObj.transform.Rotate(0.0f, 180f, 0.0f);
    float num = 0.0f;
    if ((!((UnityEngine.Object) GTPlayer.Instance != (UnityEngine.Object) null) || !((UnityEngine.Object) GTPlayer.Instance.bodyCollider != (UnityEngine.Object) null) ? 0 : (((UnityEngine.Object) ((Collider) GTPlayer.Instance.bodyCollider).attachedRigidbody != (UnityEngine.Object) null) ? 1 : 0)) != 0)
    {
      Vector3 linearVelocity = ((Collider) GTPlayer.Instance.bodyCollider).attachedRigidbody.linearVelocity;
      num = linearVelocity.magnitude;
    }
    this._displayedSpeed = (double) num <= (double) this._displayedSpeed ? Mathf.Lerp(this._displayedSpeed, num, Time.deltaTime * 2f) : Mathf.Lerp(this._displayedSpeed, num, Time.deltaTime * 10f);
    this._speedometerText.text = $"SPEED: {this._displayedSpeed:F1}";
  }

  private enum AlertMode
  {
    Off,
    Mods,
    Cheats,
    All,
  }
}
