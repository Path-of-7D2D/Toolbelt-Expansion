using HarmonyLib;
using UnityEngine;

namespace ToolbeltExpansion
{
    /// <summary>
    /// Expands the in-game player toolbelt from the vanilla 10 slots to 12.
    ///
    /// Verified against the decompiled Inventory class (V3.0 experimental):
    ///   PUBLIC_SLOTS_PLAYMODE => 10                       (trivial constant getter)
    ///   PUBLIC_SLOTS          => playmode or prefab editor (20)
    ///   INVENTORY_SLOTS       => PUBLIC_SLOTS + 1          (+1 = held/"dummy" slot)
    ///   ctor: slots = new ItemInventoryData[INVENTORY_SLOTS]   // array sized here
    ///   AddItemAtSlot/SetItem gate on `_slot &lt; PUBLIC_SLOTS`, so a too-small array
    ///   OR an un-patched PUBLIC_SLOTS silently drops items in slots 11/12.
    ///
    /// Two layers of defense (the constant getter can be JIT-inlined, which would defeat a
    /// getter-only patch):
    ///   1. Postfix the PUBLIC_SLOTS getters so every runtime read returns 12.
    ///   2. Postfix the Inventory constructor to physically grow the arrays, so capacity is
    ///      correct even if the size was inlined at allocation time.
    ///
    /// Multiplayer: this changes the networked inventory data model, so it must be installed
    /// on the server AND every client (the patch runs on dedicated servers too).
    /// </summary>
    public static class ToolbeltSlotPatches
    {
        /// <summary>Vanilla play-mode toolbelt size.</summary>
        public const int VanillaPlaySlots = 10;

        /// <summary>How many toolbelt slots we want during normal play.</summary>
        public const int ExpandedSlots = 12;

        private static bool _loggedLocalPlayer;

        // --- Layer 1: the slot-count getters ------------------------------------------------

        [HarmonyPatch(typeof(Inventory), "PUBLIC_SLOTS_PLAYMODE", MethodType.Getter)]
        public static class PublicSlotsPlaymode_Patch
        {
            public static void Postfix(ref int __result) => __result = ExpandedSlots;
        }

        [HarmonyPatch(typeof(Inventory), "PUBLIC_SLOTS", MethodType.Getter)]
        public static class PublicSlots_Patch
        {
            // Only touch the normal play count (10). Leaves the prefab editor's 20-slot belt.
            public static void Postfix(ref int __result)
            {
                if (__result == VanillaPlaySlots) __result = ExpandedSlots;
            }
        }

        // --- Layer 2: guarantee the backing arrays are actually large enough ----------------

        [HarmonyPatch(typeof(Inventory), MethodType.Constructor,
            new System.Type[] { typeof(IGameManager), typeof(EntityAlive) })]
        public static class InventoryCtor_Patch
        {
            public static void Postfix(Inventory __instance) => EnsureCapacity(__instance);
        }

        /// <summary>
        /// Grows <c>slots</c> / <c>models</c> / <c>preferredItemSlots</c> to fit the expanded
        /// toolbelt, keeping the held/"dummy" slot last (the game expects it at the end). The
        /// new public slots are filled with the shared <c>emptyItem</c>, exactly like Clear().
        /// </summary>
        private static void EnsureCapacity(Inventory inv)
        {
            int wantInv = ExpandedSlots + 1; // public slots + 1 held/dummy slot
            var oldSlots = inv.slots;

            if (oldSlots != null && oldSlots.Length >= 1 && oldSlots.Length < wantInv)
            {
                int oldDummy = oldSlots.Length - 1;

                var newSlots = new ItemInventoryData[wantInv];
                for (int i = 0; i < oldDummy; i++) newSlots[i] = oldSlots[i];
                for (int i = oldDummy; i < wantInv - 1; i++) newSlots[i] = inv.emptyItem;
                newSlots[wantInv - 1] = oldSlots[oldDummy]; // keep the dummy slot at the end
                inv.slots = newSlots;

                var oldModels = inv.models;
                var newModels = new Transform[wantInv];
                if (oldModels != null && oldModels.Length >= 1)
                {
                    int oldModelDummy = oldModels.Length - 1;
                    for (int i = 0; i < oldModelDummy && i < wantInv - 1; i++) newModels[i] = oldModels[i];
                    newModels[wantInv - 1] = oldModels[oldModelDummy];
                }
                inv.models = newModels;

                if (inv.preferredItemSlots == null || inv.preferredItemSlots.Length < ExpandedSlots)
                {
                    var oldPref = inv.preferredItemSlots;
                    var newPref = new int[ExpandedSlots];
                    if (oldPref != null)
                        for (int i = 0; i < oldPref.Length && i < ExpandedSlots; i++) newPref[i] = oldPref[i];
                    inv.preferredItemSlots = newPref;
                }
            }

            // One-shot diagnostic so the log proves whether the patch ran and what size resulted.
            if (!_loggedLocalPlayer && inv.entity is EntityPlayerLocal)
            {
                _loggedLocalPlayer = true;
                Log.Out($"[ToolbeltExpansion] Local player inventory ready: slots.Length={inv.slots?.Length}, " +
                        $"PUBLIC_SLOTS getter reports {inv.PUBLIC_SLOTS} (target {ExpandedSlots}).");
            }
        }
    }
}
