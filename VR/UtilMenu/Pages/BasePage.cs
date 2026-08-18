using SakuraaCastingMod.Shared.Models;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace SakuraaCastingMod.VR.UtilMenu.Pages;

public abstract class BasePage
{
  public List<UtilTab> Tabs = new List<UtilTab>();

  public abstract string PageName { get; }

  public abstract Material PageIcon { get; }

  public BasePage() => this.BuildTabs();

  public abstract void BuildTabs();

  public virtual string GetAccessError() => (string) null;

  public virtual void Start()
  {
  }

  public virtual void LateUpdate()
  {
  }

  public virtual void FixedUpdate()
  {
  }

  public virtual void OnTabSelected(int tabIndex)
  {
  }

  public virtual void OnPageClosed()
  {
  }

  public virtual void RefreshPageUI()
  {
  }

  public virtual bool ShouldHideStandardBars() => false;

  protected void AddTab(Material icon, string description, params MenuElement[] elements)
  {
    UtilTab utilTab = new UtilTab()
    {
      TabIcon = icon,
      Description = description
    };
    if (elements != null)
      utilTab.Elements.AddRange((IEnumerable<MenuElement>) elements);
    this.Tabs.Add(utilTab);
  }

  protected void AddTab(Material icon, params MenuElement[] elements)
  {
    this.AddTab(icon, (string) null, elements);
  }
}
