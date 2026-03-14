using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

[HarmonyPatch(typeof(Creature), nameof(Creature.LoseHpInternal))]
public static class NoDamagePatch
{
    static bool Prefix(Creature __instance, decimal amount, ValueProp props, ref DamageResult __result)
    {
        if (__instance.Player != null)
        {
            // Allow forced kills when abandoning or game is over so the run can end properly
            if (RunManager.Instance.IsAbandoned || RunManager.Instance.IsGameOver)
                return true;

            DamageTracker.RecordTaken(__instance.Player, (int)amount);
            __result = new DamageResult(__instance, props)
            {
                UnblockedDamage = 0,
                WasTargetKilled = false,
                OverkillDamage = 0
            };
            return false;
        }
        return true;
    }
}
