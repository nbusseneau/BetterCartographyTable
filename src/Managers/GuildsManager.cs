using Guilds;
using UnityEngine;

namespace BetterCartographyTable.Managers;

public static class GuildsManager
{
  public static bool IsEnabled => API.IsLoaded();
  private static Guild GetCurrentGuild() => IsEnabled ? API.GetOwnGuild() : null;
  public static string CurrentGuildName { get; private set; }
  private static readonly Color fallbackColor = new(1, 0.7176471f, 0.3602941f);
  private static Color? color = null;
  public static Color CurrentGuildColor { get => color ?? LazySetGuildColor(); }

  public static void OnGameStart()
  {
    if (!IsEnabled) return;
    if (GetCurrentGuild() is { } guild) Register(guild);

    API.RegisterOnGuildJoined((guild, player) =>
    {
      var isCurrentPlayer = player == PlayerReference.forOwnPlayer();
      if (isCurrentPlayer) Register(guild);
    });

    API.RegisterOnGuildLeft((_, player) =>
    {
      var isCurrentPlayer = player == PlayerReference.forOwnPlayer();
      if (isCurrentPlayer) Unregister();
    });
  }

  private static void Register(Guild guild)
  {
    CurrentGuildName = guild.Name;
  }

  private static void Unregister()
  {
    CurrentGuildName = null;
    color = null;
    MinimapManager.RemovePins(MinimapManager.GuildPins);
  }

  private static Color LazySetGuildColor()
  {
    if (GetCurrentGuild() is { } guild && ColorUtility.TryParseHtmlString(guild.General.color, out var parsedColor))
    {
      color = parsedColor;
      return parsedColor;
    }
    return fallbackColor;
  }
}
