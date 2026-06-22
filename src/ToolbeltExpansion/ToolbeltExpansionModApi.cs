using System;
using System.Reflection;
using HarmonyLib;
using Platform;

namespace ToolbeltExpansion
{
    /// <summary>
    /// Mod entry point. 7 Days to Die's mod loader instantiates any class implementing
    /// <see cref="IModApi"/> in a mod's assemblies and calls <see cref="InitMod"/>.
    /// We use it to bootstrap Harmony.
    /// </summary>
    public class ToolbeltExpansionModApi : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            // NOTE: the patches change the inventory data model (slot count), not just
            // the UI, so they must run on dedicated servers as well as clients. Do NOT
            // early-return on GameManager.IsDedicatedServer here.
            const string id = "com.pathof7d2d.toolbeltexpansion";
            Log.Out($"[ToolbeltExpansion] InitMod running (target {ToolbeltSlotPatches.ExpandedSlots} slots)...");

            var harmony = new Harmony(id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            int patched = 0;
            foreach (var _ in harmony.GetPatchedMethods()) patched++;
            Log.Out($"[ToolbeltExpansion] Harmony applied {patched} patch target(s). " +
                    "If this line is absent in your log, the DLL is not loading.");

            // The toolbelt hotkey input set (PlayerActionsLocal) is created at engine startup,
            // BEFORE this mod loads, so the constructor patch won't fire for it. Add the
            // slot 11/12 hotkeys to the already-running instance directly.
            try
            {
                ToolbeltHotkeyPatches.AddHotkeys(PlatformManager.NativePlatform?.Input?.PrimaryPlayer);
            }
            catch (Exception e)
            {
                Log.Warning("[ToolbeltExpansion] Failed to add slot 11/12 hotkeys to the live input set: " + e);
            }
        }
    }
}
