using GorillaNetworking;
using Photon.Pun;
using SakMerge.Api;
using SakuraaCastingMod.Core;
using SakuraaCastingMod.Shared.Helpers;
using SakuraaCastingMod.Shared.Models;
using SakuraaCastingMod.VR.UtilMenu.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Pages;

public class MicPage : BasePage
{
  [SavedSetting("Sak_Mic_Device", "")]
  private static string _savedMicDevice = "";
  [SavedSetting("pttType", "OPEN MIC")]
  private static string _pttType = "OPEN MIC";
  private int _micListOffset = 0;
  private const int MicButtonsPerPage = 4;
  private int _pollFrameCounter = 0;
  private const int PollIntervalFrames = 60;

  public override string PageName => "MIC SETTINGS";

  public override Material PageIcon => UtilMenuMain.Instance.Icons.Mic;

  public override void Start()
  {
    // ISSUE: explicit non-virtual call
    base.Start();
    if (!string.IsNullOrEmpty(MicPage._savedMicDevice) && MicDeviceManager.AvailableDevices.Contains(MicPage._savedMicDevice))
      MicDeviceManager.SetMicrophone(MicPage._savedMicDevice);
    if (!((UnityEngine.Object) GorillaComputer.instance != (UnityEngine.Object) null))
      return;
    Mic.SetMode(MicPage._pttType);
  }

  public override void BuildTabs()
  {
    this.AddTab(UtilMenuMain.Instance.Icons.Mic, new MenuElement("OPEN MIC", (Action) null)
    {
      Type = ElementType.Toggle,
      IsToggled = this.IsPttCurrent("OPEN MIC"),
      OnToggle = (Action) (() => this.SetMicType("OPEN MIC"))
    }, new MenuElement("PUSH TO TALK", (Action) null)
    {
      Type = ElementType.Toggle,
      IsToggled = this.IsPttCurrent("PUSH TO TALK"),
      OnToggle = (Action) (() => this.SetMicType("PUSH TO TALK"))
    }, new MenuElement("PUSH TO MUTE", (Action) null)
    {
      Type = ElementType.Toggle,
      IsToggled = this.IsPttCurrent("PUSH TO MUTE"),
      OnToggle = (Action) (() => this.SetMicType("PUSH TO MUTE"))
    }, new MenuElement("REFRESH AUDIO", (Action) (() =>
    {
      OutputDeviceManager.RestartAudioSystem();
      UtilMenuController.Instance.RefreshUI();
    })));
    this.Tabs.Add(new UtilTab()
    {
      TabIcon = UtilMenuMain.Instance.Icons.FileSettingsIcon
    });
    this.RefreshMicControlTab();
  }

  public override void FixedUpdate()
  {
    if (!UtilMenuController.Instance.isMenuEnabled)
      return;
    ++this._pollFrameCounter;
    if (this._pollFrameCounter < 60)
      return;
    this._pollFrameCounter = 0;
    if (!MicDeviceManager.PollForChanges())
      return;
    this.RefreshMicControlTab();
  }

  public override void OnTabSelected(int tabIndex)
  {
    switch (tabIndex)
    {
      case 0:
        this.UpdatePttButtons();
        break;
      case 1:
        this.RefreshMicControlTab();
        break;
    }
  }

  private bool IsPttCurrent(string type) => Mic.CurrentMode() == type;

  private void SetMicType(string type)
  {
    MicPage._pttType = type;
    Configuration.SaveSettings();
    Mic.SetMode(type);
    this.UpdatePttButtons();
    UtilMenuController.Instance.RefreshUI();
  }

  private void UpdatePttButtons()
  {
    if ((this.Tabs.Count < 1 ? 1 : (this.Tabs[0].Elements.Count < 3 ? 1 : 0)) != 0)
      return;
    this.Tabs[0].Elements[0].IsToggled = this.IsPttCurrent("OPEN MIC");
    this.Tabs[0].Elements[1].IsToggled = this.IsPttCurrent("PUSH TO TALK");
    this.Tabs[0].Elements[2].IsToggled = this.IsPttCurrent("PUSH TO MUTE");
  }

  private void RefreshMicControlTab()
  {
    if (this.Tabs.Count < 2)
      return;
    UtilTab tab = this.Tabs[1];
    tab.Elements.Clear();
    tab.Description = (string) null;
    if (PhotonNetwork.InRoom)
    {
      if (!MicDeviceManager.TryInitialize())
      {
        tab.Description = "FAILED TO INITIALIZE MIC MANAGER.";
      }
      else
      {
        List<string> availableDevices = MicDeviceManager.AvailableDevices;
        int count = availableDevices.Count;
        string selectedDevice = MicDeviceManager.SelectedDevice;
        if (count == 0)
        {
          tab.Description = "NO MICROPHONES FOUND.";
        }
        else
        {
          if (this._micListOffset >= count)
            this._micListOffset = 0;
          for (int index1 = 0; index1 < 4; ++index1)
          {
            int index2 = this._micListOffset + index1;
            if (index2 < count)
            {
              string deviceName = availableDevices[index2];
              bool initialValue = deviceName == selectedDevice;
              string str = deviceName.Length > 18 ? deviceName.Substring(0, 15) + "..." : deviceName;
              tab.Elements.Add(new MenuElement(str.ToUpper(), (Action) null, initialValue)
              {
                Type = ElementType.Toggle,
                IsToggled = initialValue,
                OnToggle = (Action) (() =>
                {
                  MicDeviceManager.SetMicrophone(deviceName);
                  MicPage._savedMicDevice = deviceName;
                  Configuration.SaveSettings();
                  this.RefreshMicControlTab();
                  UtilMenuController.Instance.RefreshUI();
                })
              });
            }
          }
          bool flag1 = this._micListOffset + 4 < count;
          bool flag2 = this._micListOffset > 0;
          if (flag1)
          {
            tab.Elements.Add(new MenuElement("NEXT PAGE >>", (Action) (() =>
            {
              this._micListOffset += 4;
              this.RefreshMicControlTab();
              UtilMenuController.Instance.RefreshUI();
            })));
          }
          else
          {
            if (!flag2)
              return;
            tab.Elements.Add(new MenuElement("<< PREVIOUS", (Action) (() =>
            {
              this._micListOffset -= 4;
              this.RefreshMicControlTab();
              UtilMenuController.Instance.RefreshUI();
            })));
          }
        }
      }
    }
    else
      tab.Description = "YOU MUST BE IN A ROOM TO CHANGE MICROPHONE DEVICES.";
  }
}
