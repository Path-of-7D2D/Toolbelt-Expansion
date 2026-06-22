using System;
using System.Reflection;
using HarmonyLib;
using InControl;

namespace ToolbeltExpansion
{
    /// <summary>
    /// Adds hotkeys for the two expanded toolbelt slots:
    ///   '-' (Key.Minus)  -> slot 11
    ///   '=' (Key.Equals) -> slot 12
    ///
    /// How it works (verified against PlayerActionsLocal / PlayerMoveController, V3.0):
    ///   Vanilla defines InventorySlot1..10 (keys 1..0) and appends them, in order, to the
    ///   public `InventoryActions` list. `InventorySlotWasPressed` returns the pressed action's
    ///   index in that list, and PlayerMoveController uses that index directly as the toolbelt
    ///   slot (gated by `index &lt; PUBLIC_SLOTS`). Appending two more actions (indices 10, 11)
    ///   bound to '-'/'=' makes them select slots 11 and 12. They also register under the
    ///   "Toolbelt" controls group so they show in the Controls menu and can be rebound.
    ///
    /// IMPORTANT timing detail: PlayerActionsLocal is created at engine startup (PlayerInput
    /// manager), BEFORE mods load, so a constructor patch never fires for the live instance.
    /// We therefore add the actions directly to the existing instance from InitMod
    /// (see ToolbeltExpansionModApi), and keep the constructor postfix only as a backup for any
    /// instance created later. <see cref="AddHotkeys"/> is idempotent so both paths are safe.
    /// </summary>
    public static class ToolbeltHotkeyPatches
    {
        private const string Slot11Action = "Inventory11";
        private const string Slot12Action = "Inventory12";

        // CreatePlayerAction is protected on InControl's PlayerActionSet; reach it via reflection.
        private static readonly MethodInfo CreatePlayerActionMethod =
            AccessTools.Method(typeof(PlayerActionSet), "CreatePlayerAction", new[] { typeof(string) });

        /// <summary>Adds the slot 11/12 actions to a PlayerActionsLocal instance. Safe to call more than once.</summary>
        public static void AddHotkeys(PlayerActionsLocal actions)
        {
            if (actions == null) return;
            if (CreatePlayerActionMethod == null)
            {
                Log.Warning("[ToolbeltExpansion] PlayerActionSet.CreatePlayerAction not found; slot 11/12 hotkeys " +
                            "not added (Shift+1 / Shift+2 still reach those slots).");
                return;
            }

            // Idempotent: bail if these actions were already added to this set.
            for (int i = 0; i < actions.InventoryActions.Count; i++)
            {
                if (actions.InventoryActions[i]?.Name == Slot11Action) return;
            }

            AddToolbeltSlot(actions, Slot11Action, "inpActInventorySlot11Name", Key.Minus);
            AddToolbeltSlot(actions, Slot12Action, "inpActInventorySlot12Name", Key.Equals);
            Log.Out($"[ToolbeltExpansion] Added toolbelt hotkeys: '-' -> slot 11, '=' -> slot 12 " +
                    $"(InventoryActions count now {actions.InventoryActions.Count}).");
        }

        private static void AddToolbeltSlot(PlayerActionsLocal actions, string actionName, string nameKey, Key key)
        {
            var action = (PlayerAction)CreatePlayerActionMethod.Invoke(actions, new object[] { actionName });
            action.UserData = new PlayerActionData.ActionUserData(nameKey, null, PlayerActionData.GroupToolbelt);
            action.AddDefaultBinding(key);
            actions.InventoryActions.Add(action);
        }

        // Backup: catch any PlayerActionsLocal constructed AFTER the mod loads (the startup
        // instance is handled directly in InitMod, since it predates this patch).
        [HarmonyPatch(typeof(PlayerActionsLocal), MethodType.Constructor, new Type[0])]
        public static class PlayerActionsLocalCtor_Patch
        {
            public static void Postfix(PlayerActionsLocal __instance) => AddHotkeys(__instance);
        }
    }
}
