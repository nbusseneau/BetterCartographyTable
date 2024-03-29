using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BetterCartographyTable.Managers;
using BetterCartographyTable.Model;
using BetterCartographyTable.UI;
using HarmonyLib;
using static Minimap;

namespace BetterCartographyTable.Patches;

public static class MinimapPatches
{
  [HarmonyPatch(typeof(Minimap), nameof(Minimap.Awake))]
  private class InjectSharablePinsAndCreatePinToggles
  {
    private static void Postfix()
    {
      MinimapManager.OnAwake();
      MinimapManagerUI.OnAwake();
    }
  }

  [HarmonyPatch(typeof(Minimap), nameof(Minimap.Start))]
  private class ResetTogglesVisiblityOnStart
  {
    private static void Postfix() => MinimapManagerUI.OnStart();
  }

  [HarmonyPatch(typeof(Minimap), nameof(Minimap.SetMapMode))]
  private class HandleInteractionsOnMapModeChange
  {
    private static void Postfix(MapMode mode)
    {
      var isMinimapOpen = mode == MapMode.Large;
      if (isMinimapOpen) return;
      InteractionManager.OnMapClose();
    }
  }

  /// <summary>
  /// Ensure sure calls to <c>Minimap.AddPin(...)</c> always add <c>SharablePinData</c> pins to the list and use our own
  /// <c>Add(...)</c> implementation.
  /// </summary>
  [HarmonyPatch(typeof(Minimap), nameof(Minimap.AddPin))]
  private class InjectSharablePinsAndSharablePinDataInAddPin
  {
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
      return new CodeMatcher(instructions)
          .MatchForward(false, new CodeMatch(OpCodes.Newobj, AccessTools.Constructor(typeof(PinData))))
          .ThrowIfInvalid("Could not patch PinData constructor in Minimap.AddPin(...)")
          .SetOperandAndAdvance(AccessTools.Constructor(typeof(SharablePinData)))
          .MatchForward(false, new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(List<PinData>), nameof(Minimap.m_pins.Add))))
          .ThrowIfInvalid("Could not patch List<PinData>.Add(...) method in Minimap.AddPin(...)")
          .SetOperandAndAdvance(AccessTools.Method(typeof(SharablePins), nameof(SharablePins.Add)))
          .InstructionEnumeration();
    }
  }

  [HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdatePins))]
  private class UpdateUIOnUpdatePins
  {
    private static void Postfix() => MinimapManagerUI.OnUpdatePins();
  }

  #region Click intercepts

  [HarmonyPatch(typeof(Minimap), nameof(Minimap.OnMapDblClick))]
  private class InterceptOnMapDblClick
  {
    private static bool Prefix()
    {
      var shouldRunOriginalMethod = InteractionManager.OnMapDblClick();
      return shouldRunOriginalMethod;
    }
  }

  [HarmonyPatch(typeof(Minimap), nameof(Minimap.OnMapLeftClick))]
  private class InterceptOnMapLeftClick
  {
    private static bool Prefix()
    {
      var shouldRunOriginalMethod = false;
      InteractionManager.OnMapLeftClick();
      return shouldRunOriginalMethod;
    }
  }

  [HarmonyPatch(typeof(Minimap), nameof(Minimap.OnMapRightClick))]
  private class InterceptOnMapRightClick
  {
    private static bool Prefix()
    {
      var shouldRunOriginalMethod = false;
      InteractionManager.OnMapRightClick();
      return shouldRunOriginalMethod;
    }
  }

  #endregion

  #region Map data intercepts

  /// <summary>
  /// Inject private pins as <c>Minimap.instance.m_pins</c> replacement in <c>Minimap.GetMapData()</c> so that world
  /// data stores non-shared pins in a vanilla-compatible way.
  /// </summary>
  [HarmonyPatch(typeof(Minimap), nameof(Minimap.GetMapData))]
  private class InjectPrivatePinsInGetMapData
  {
    private static List<PinData> privatePins;
    private static void Prefix() => privatePins = MinimapManager.PrivatePins.ToList<PinData>();

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
      return new CodeMatcher(instructions)
          .MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Minimap), nameof(Minimap.m_pins))))
          .ThrowIfInvalid("Could not inject pins replacement in Minimap.GetMapData()")
          .SetAndAdvance(OpCodes.Ldsfld, AccessTools.Field(typeof(InjectPrivatePinsInGetMapData), nameof(privatePins)))
          .MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Minimap), nameof(Minimap.m_pins))))
          .ThrowIfInvalid("Could not inject pins replacement in Minimap.GetMapData()")
          .SetAndAdvance(OpCodes.Ldsfld, AccessTools.Field(typeof(InjectPrivatePinsInGetMapData), nameof(privatePins)))
          .InstructionEnumeration();
    }
  }

  /// <summary>
  /// Inject public pins as <c>Minimap.instance.m_pins</c> replacement in <c>Minimap.GetSharedMapData()</c> so that
  /// non-modded clients can retrieve public pins stored in vanilla shared data.
  /// </summary>
  [HarmonyPatch(typeof(Minimap), nameof(Minimap.GetSharedMapData))]
  private class InjectPublicPinsInGetSharedMapData
  {
    private static List<PinData> publicPins;
    private static void Prefix() => publicPins = MinimapManager.PublicPins.ToList<PinData>();

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
      return new CodeMatcher(instructions)
          .MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Minimap), nameof(Minimap.m_pins))))
          .ThrowIfInvalid("Could not inject pins replacement in Minimap.GetSharedMapData()")
          .SetAndAdvance(OpCodes.Ldsfld, AccessTools.Field(typeof(InjectPublicPinsInGetSharedMapData), nameof(publicPins)))
          .MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Minimap), nameof(Minimap.m_pins))))
          .ThrowIfInvalid("Could not inject pins replacement in Minimap.GetSharedMapData()")
          .SetAndAdvance(OpCodes.Ldsfld, AccessTools.Field(typeof(InjectPublicPinsInGetSharedMapData), nameof(publicPins)))
          .InstructionEnumeration();
    }
  }

  /// <summary>
  /// Truncate <c>Minimap.AddSharedMapData()</c> after retrieving explored map data from vanilla shared data, ignoring
  /// pins coming from non-modded clients.
  /// </summary>
  [HarmonyPatch(typeof(Minimap), nameof(Minimap.AddSharedMapData))]
  private class TruncateAddSharedMapData
  {
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
      return new CodeMatcher(instructions)
          .MatchForward(false,
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
  }

  #endregion
}
