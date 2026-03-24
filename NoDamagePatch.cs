using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.ValueProps;

[HarmonyPatch(typeof(Creature), nameof(Creature.LoseHpInternal))]
public static class PoisonDamageTrackingPatch
{
    static void Postfix(Creature __instance, DamageResult __result)
    {
        if (__result.UnblockedDamage > 0
            && CombatManager.Instance != null
            && CombatManager.Instance.IsInProgress
            && CombatManager.Instance.IsEnding
            && __instance.CombatState != null)
        {
            CombatManager.Instance.History.DamageReceived(
                __instance.CombatState, __instance, null, __result, null);
        }
    }
}
