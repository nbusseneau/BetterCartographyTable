using Guilds;
using UnityEngine;

namespace BetterCartographyTable.Model.Managers;

public static class GuildsManager
{
  public static bool IsEnabled { get; private set; } = false;
  public static Guild CurrentGuild { get; private set; } = null;
  private static readonly Color s_fallbackColor = new(1, 0.7176471f, 0.3602941f);
  private static Color? s_currentGuildColor = null;
  public static Color CurrentGuildColor { get => s_currentGuildColor ?? TrySetGuildColor(); }

  public static void Initialize()
  {
    IsEnabled = API.IsLoaded();
    if (!IsEnabled) return;

    if (API.GetOwnGuild() is { } guild) Register(guild);
    API.RegisterOnGuildJoined((guild, player) => { if (player == PlayerReference.forOwnPlayer()) Register(guild); });
    API.RegisterOnGuildLeft((_, player) => { if (player == PlayerReference.forOwnPlayer()) Unregister(); });
  }

  private static void Register(Guild guild) => CurrentGuild = guild;

  private static void Unregister()
  {
    CurrentGuild = null;
    s_currentGuildColor = null;
    MinimapManager.RemovePins(MinimapManager.GuildPins);
  }

  private static Color TrySetGuildColor()
  {
    if (CurrentGuild is not null && ColorUtility.TryParseHtmlString(CurrentGuild.General.color, out var parsedColor))
    {
      s_currentGuildColor = parsedColor;
      return parsedColor;
    }
    return s_fallbackColor;
  }
}
