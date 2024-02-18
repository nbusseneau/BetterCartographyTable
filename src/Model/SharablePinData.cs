using System;
using System.Collections.Generic;
using BetterCartographyTable.Extensions;
using TMPro;
using UnityEngine;
using static Minimap;

namespace BetterCartographyTable.Model;

/// <summary>
/// Subclass of <c>PinData</c> intended for use as a transparent and complete replacement, with
/// Harmony patches ensuring that all pins on the map are actually <c>SharablePinData</c> objects
/// and not <c>PinData</c> objects.
/// </summary>
public class SharablePinData : PinData, IEquatable<PinData>
{
  private static readonly HashSet<PinType> sharablePinTypes = [
    PinType.Icon0,
    PinType.Icon1,
    PinType.Icon2,
    PinType.Icon3,
    PinType.Icon4,
    PinType.Boss,
    PinType.Hildir1,
    PinType.Hildir2,
    PinType.Hildir3,
  ];

  public SharingMode SharingMode { get; set; } = SharingMode.Private;
  public bool IsSharable => sharablePinTypes.Contains(this.m_type);
  public bool IsPrivate => this.SharingMode == SharingMode.Private;
  public bool IsPublic => this.SharingMode == SharingMode.Public;
  public bool IsGuild => this.SharingMode == SharingMode.Guild;
  public bool IsShared => !this.IsPrivate;
  public bool IsMapTablePin(MapTable mapTable)
  {
    var (isPublic, isGuild) = mapTable.GetModes();
    return isPublic && this.IsPublic || isGuild && this.IsGuild;
  }
  public void ToggleChecked() => this.m_checked = !this.m_checked;
  public override string ToString() => $"{this.SharingMode} {m_name} {m_pos} {(m_checked ? "⦻" : "⭘")} {m_author} {m_ownerID}L {Enum.GetName(typeof(PinType), m_type)}";

  public void SetVisibility(bool isVisible)
  {
    this.m_uiElement?.gameObject?.SetActive(isVisible);
    if (instance.m_mode == MapMode.Large)
    {
      this.m_NamePinData?.PinNameGameObject?.gameObject?.SetActive(isVisible);
    }
  }

  public void SetColor(Color color)
  {
    if (this.m_iconElement is { } iconElement) iconElement.color = color;
    if (this.m_NamePinData?.PinNameText is { } pinNameText) pinNameText.color = color;
  }

  public ZPackage ToZPackage()
  {
    ZPackage zPackage = new();
    zPackage.Write(this);
    return zPackage.GetCompressed();
  }

  #region IEquatable & friends

  public bool Equals(PinData other)
  {
    if (other is null) return false;
    if (ReferenceEquals(this, other)) return true;
    return this.m_pos.Equals(other.m_pos);
  }
  public override bool Equals(object obj) => this.Equals(obj as PinData);
  public static bool operator ==(SharablePinData a, PinData b) => ReferenceEquals(a, b) || (a is not null && a.Equals(b));
  public static bool operator !=(SharablePinData a, PinData b) => !(a == b);
  public override int GetHashCode() => this.m_pos.GetHashCode();

  #endregion
}
