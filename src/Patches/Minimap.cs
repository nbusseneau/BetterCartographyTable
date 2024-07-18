using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BetterCartographyTable.Model;
using BetterCartographyTable.Model.Managers;
using BetterCartographyTable.Model.PinClickHandlers;
using BetterCartographyTable.UI;
using HarmonyLib;
using static Minimap;

namespace BetterCartographyTable.Patches;

[HarmonyPatch(typeof(Minimap))]
public static class MinimapPatches
{
  /// <summary>
  /// Do the thing when SetMapMode is called (i.e. when map is opened / closed)
  /// </summary>
  [HarmonyPatch(typeof(Minimap), nameof(Minimap.SetMapMode))]
  private class OnSetMapMode
  {
    private static bool s_noMapModeCache = false;

    private static void Prefix(MapMode mode)
    {
      // temporarily override noMap mode when opening the map as part of using a table
      s_noMapModeCache = Game.m_noMap;
      var isMinimapOpening = mode == MapMode.Large;
      if (isMinimapOpening && MapTableManager.IsTableValid && s_noMapModeCache) Game.m_noMap = false;

      // trigger on close callback when closing map
      var isMinimapClosing = mode == MapMode.Small;
      if (isMinimapClosing) MapTableManager.TryClose();
    }

    private static void Postfix()
    {
      // restore noMap mode immediately after opening the map, ensuring default behaviour after table interaction ends
      if (s_noMapModeCache)
      {
        Game.m_noMap = true;
        s_noMapModeCache = false;
      }
    }
  }

  /// <summary>
  /// Try to enforce that pins stored in <c>Minimap.instance.m_pins</c> are <c>SharablePinData</c> by editing calls to
  /// <c>Minimap.AddPin(...)</c> to use our constructor instead of <c>PinData</c>'s constructor.
  /// </summary>
  [HarmonyTranspiler]
  [HarmonyPatch(nameof(Minimap.AddPin))]
  private static IEnumerable<CodeInstruction> EnforceSharablePinDataInAddPin(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
        .MatchStartForward(new CodeMatch(OpCodes.Newobj, AccessTools.Constructor(typeof(PinData))))
        .ThrowIfInvalid("Could not patch PinData constructor in Minimap.AddPin(...)")
        .SetOperandAndAdvance(AccessTools.Constructor(typeof(SharablePinData)))
        .InstructionEnumeration();
  }

  /// <summary>
  /// Always set pins as if created locally (with ownerID = 0L) since we replace the vanilla sharing mechanism entirely.
  /// </summary>
  [HarmonyPostfix]
  [HarmonyPatch(nameof(Minimap.AddPin))]
  private static void AlwaysAddPinAsLocal(ref PinData __result) => __result.m_ownerID = 0L;

  /// <summary>
  /// To prevent accidentally adding new pins while sharing / unsharing pins with modifier key + left click, do nothing
  /// when using modifier key + double-clicking.
  /// </summary>
  [HarmonyPrefix]
  [HarmonyPatch(nameof(Minimap.OnMapDblClick))]
  private static void InterceptOnMapDblClick(ref bool __runOriginal) => __runOriginal = !Plugin.IsModifierKeyPressed;

  [HarmonyPrefix]
  [HarmonyPatch(nameof(Minimap.OnMapLeftClick))]
  private static void InterceptOnMapLeftClick(ref bool __runOriginal)
  {
    __runOriginal = false;
    var clickType = Plugin.IsModifierKeyPressed ? ClickType.LeftClickPlusModifier : ClickType.LeftClick;
    MinimapManager.HandleClick(clickType);
  }

  [HarmonyPrefix]
  [HarmonyPatch(nameof(Minimap.OnMapRightClick))]
  private static void InterceptOnMapRightClick(ref bool __runOriginal)
  {
    __runOriginal = false;
    var clickType = Plugin.IsModifierKeyPressed ? ClickType.RightClickPlusModifier : ClickType.RightClick;
    MinimapManager.HandleClick(clickType);
  }

  /// <summary>
  /// Inject private pins as <c>Minimap.instance.m_pins</c> replacement in <c>Minimap.GetMapData()</c> so that world
  /// data only stores private pins in the save file, in a vanilla-compatible way.
  /// </summary>
  [HarmonyPatch(typeof(Minimap), nameof(Minimap.GetMapData))]
  private class InjectPrivatePinsInSaveFile
  {
    private static List<PinData> s_privatePins;
    private static void Prefix() => s_privatePins = MinimapManager.PrivatePins.ToList();

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
      return new CodeMatcher(instructions)
          .MatchStartForward(new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Minimap), nameof(Minimap.m_pins))))
          .ThrowIfInvalid("Could not inject pins replacement in Minimap.GetMapData()")
          .Repeat(m => m.SetAndAdvance(OpCodes.Ldsfld, AccessTools.Field(typeof(InjectPrivatePinsInSaveFile), nameof(s_privatePins))))
          .InstructionEnumeration();
    }
  }

  /// <summary>
  /// We edit <c>Minimap.GetSharedMapData()</c> on two fronts:
  /// <list type="bullet">
  ///   <item>
  ///     <description>Inject a conditional branching over <c>Minimap.instance.m_explored</c> exploration status sharing
  ///     if the user has elected to keep their map exploration private, or to share it only with their guild but is not
  ///     interacting with a guild table.</description>
  ///   </item>
  ///   <item>
  ///     <description>Inject public pins as <c>Minimap.instance.m_pins</c> replacement so that modded clients share
  ///     public pins to non-modded clients via the vanilla shared map data.</description>
  ///   </item>
  /// </list>
  /// </summary>
  [HarmonyPatch(typeof(Minimap), nameof(Minimap.GetSharedMapData))]
  private class EditOutboundVanillaSharedMapData
  {
    private static bool s_shouldShareMapExploration;
    private static List<PinData> s_pins;
    private static void Prefix()
    {
      s_shouldShareMapExploration = Plugin.MapExplorationSharingMode == SharingMode.Public || Plugin.MapExplorationSharingMode == SharingMode.Guild && MapTableManager.CurrentTable.IsGuild;
      s_pins = MapTableManager.CurrentTable.IsPublic ? MinimapManager.PublicPins.ToList<PinData>() : [];
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
      var matcher = new CodeMatcher(instructions, generator);

      // inject conditional branching to jump over map exploration status sharing, if we are not sharing map exploration
      // part 1: insert delegate right before start of loop
      matcher.MatchEndForward(
          new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Minimap), nameof(Minimap.m_explored))),
          new CodeMatch(OpCodes.Ldlen),
          new CodeMatch(OpCodes.Conv_I4),
          new CodeMatch(OpCodes.Callvirt),
          new CodeMatch(OpCodes.Ldc_I4_0))
        .ThrowIfInvalid("Could not inject conditional branching over m_explored sharing loop in Minimap.GetSharedMapData()")
        // load ZPackage (index 1) to be consumed as argument to the delegate
        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1))
        .InsertAndAdvance(Transpilers.EmitDelegate<Func<ZPackage, bool>>(zPackage =>
        {
          if (!s_shouldShareMapExploration) for (int i = 0; i < instance.m_explored.Length; i++) zPackage.Write(false);
          return s_shouldShareMapExploration;
        }));
      var delegatePosition = matcher.Pos;

      // part 2: find end of loop
      matcher.MatchStartForward(
          new CodeMatch(OpCodes.Ldc_I4_0),
          new CodeMatch(OpCodes.Stloc_2),
          new CodeMatch(OpCodes.Ldarg_0),
          new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Minimap), nameof(Minimap.m_pins))))
        .ThrowIfInvalid("Could not inject conditional branching over m_explored sharing loop in Minimap.GetSharedMapData()");
      var branchTargetLabelPosition = matcher.Pos;

      // part 3: roll back to delegate position and insert conditional branching over the loop
      matcher.Advance(delegatePosition - branchTargetLabelPosition)
        .InsertBranchAndAdvance(OpCodes.Brfalse_S, branchTargetLabelPosition);

      // inject public pins to replace m_pins ldfld
      matcher.Start()
        .MatchStartForward(new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Minimap), nameof(Minimap.m_pins))))
        .ThrowIfInvalid("Could not inject m_pins replacement in Minimap.GetSharedMapData()")
        .Repeat(m => m.SetAndAdvance(OpCodes.Ldsfld, AccessTools.Field(typeof(EditOutboundVanillaSharedMapData), nameof(s_pins))));

      return matcher.InstructionEnumeration();
    }
  }

  /// <summary>
  /// Truncate <c>Minimap.AddSharedMapData()</c> after retrieving explored map data from vanilla shared data, so that
  /// modded clients ignore pins coming from non-modded clients via the vanilla shared map data.
  /// </summary>
  [HarmonyTranspiler]
  [HarmonyPatch(nameof(Minimap.AddSharedMapData))]
  private static IEnumerable<CodeInstruction> TruncateInboundVanillaSharedMapData(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
        .MatchStartForward(
            new CodeMatch(OpCodes.Ldc_I4_0),
            new CodeMatch(OpCodes.Stloc_S),
            new CodeMatch(OpCodes.Ldloc_1),
            new CodeMatch(OpCodes.Ldc_I4_2),
            new CodeMatch(OpCodes.Blt))
        .ThrowIfInvalid("Could not truncate Minimap.AddSharedMapData()")
        .SetAndAdvance(OpCodes.Ldloc_3, null)
        .SetAndAdvance(OpCodes.Ret, null)
        .InstructionEnumeration();
  }

  [HarmonyPostfix]
  [HarmonyPatch(nameof(Minimap.Reset))]
  private static void PrepareTogglesAndKeyHints()
  {
    MinimapUI.PrepareToggles();
    MinimapUI.PrepareKeyHints();
  }

  [HarmonyPostfix]
  [HarmonyPatch(nameof(Minimap.UpdateMap))]
  private static void UpdateTogglesAndKeyHints()
  {
    MinimapUI.UpdateToggles();
    MinimapUI.UpdateKeyHints();
  }

  [HarmonyPostfix]
  [HarmonyPatch(nameof(Minimap.UpdatePins))]
  private static void UpdateSharedPins() => MinimapUI.UpdateSharedPins();

  /// <summary>
  /// If any explored map data is present after syncing on a table or initially loading from save file, force the
  /// shared exploration toggle active (since otherwise it's dependent on non-local pins being present, and we will
  /// never have any).
  /// </summary>
  [HarmonyPostfix]
  [HarmonyPatch(nameof(Minimap.AddSharedMapData))]
  [HarmonyPatch(nameof(Minimap.ResetAndExplore))]
  private static void ShowCartographyToggle()
  {
    if (instance.m_exploredOthers.Length > 0) instance.m_sharedMapHint.SetActive(true);
  }
}
