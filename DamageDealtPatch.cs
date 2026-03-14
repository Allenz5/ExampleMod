using HarmonyLib;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Creatures;

[HarmonyPatch(typeof(CombatHistory), nameof(CombatHistory.DamageReceived))]
public static class DamageDealtPatch
{
    static void Postfix(Creature receiver, Creature? dealer, DamageResult result)
    {
        if (dealer?.Player != null && receiver.Player == null)
        {
            DamageTracker.RecordDealt(dealer.Player, result.UnblockedDamage);
        }
    }
}
