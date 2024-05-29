using Guilds;
using UnityEngine;

namespace BetterCartographyTable.Model.Managers;

public static class GuildsManager
{
  public static bool IsEnabled => API.IsLoaded();
  private static Guild GetCurrentGuild() => IsEnabled ? API.GetOwnGuild() : null;
  public static string CurrentGuildName { get; private set; }
  private static readonly Color s_fallbackColor = new(1, 0.7176471f, 0.3602941f);
  private static Color? s_color = null;
  public static Color CurrentGuildColor { get => s_color ?? LazySetGuildColor(); }

  public static void TryRegisterGuild()
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
    s_color = null;
    MinimapManager.RemovePins(MinimapManager.GuildPins);
  }

  private static Color LazySetGuildColor()
  {
    if (GetCurrentGuild() is { } guild && ColorUtility.TryParseHtmlString(guild.General.color, out var parsedColor))
    {
      s_color = parsedColor;
      return parsedColor;
    }
    return s_fallbackColor;
  }
}
