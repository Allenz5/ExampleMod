using HarmonyLib;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

[HarmonyPatch(typeof(CombatHistory), nameof(CombatHistory.DamageReceived))]
public static class DamageDealtPatch
{
    // Harmony injects all matching parameter names from the original method signature.
    // cardSource is the card that triggered the damage (if any); dealer is the attacking creature.
    static void Postfix(Creature receiver, Creature? dealer, DamageResult result, CardModel? cardSource)
    {
        // Prefer the card's owner (handles abilities/relics played by a player whose
        // damage is delivered through a summon or indirect effect), fall back to the
        // dealer creature's player.
        var dealerPlayer = cardSource?.Owner ?? dealer?.Player;

        if (dealerPlayer != null && receiver.Player == null)
        {
            DamageTracker.RecordDealt(dealerPlayer, result.TotalDamage);
        }

        if (receiver.Player != null)
        {
            DamageTracker.RecordTaken(receiver.Player, result.TotalDamage);
        }
    }
}
