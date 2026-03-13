using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Values;

[HarmonyPatch(typeof(Creature), nameof(Creature.LoseHpInternal))]
public static class NoDamagePatch
{
    static bool Prefix(Creature __instance, ValueProp props, ref DamageResult __result)
    {
        if (__instance.Player != null)
        {
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
