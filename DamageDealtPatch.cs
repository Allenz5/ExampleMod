using HarmonyLib;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

[HarmonyPatch(typeof(CombatHistory), nameof(CombatHistory.DamageReceived))]
public static class DamageDealtPatch
{
    // Harmony injects all matching parameter names from the original method signature.
    // cardSource is the card that triggered the damage (if any); dealer is the attacking creature.
    static void Postfix(Creature receiver, Creature? dealer, DamageResult result, CardModel? cardSource)
    {
        // Prefer the card's owner (handles abilities played by a player whose damage is
        // delivered through a summon or indirect effect), fall back to the dealer creature's player.
        var dealerPlayer = cardSource?.Owner ?? dealer?.Player;

        // If there's still no dealer, this may be status-effect damage (e.g. poison tick).
        // ValueProp.Unpowered is set on all power/debuff-sourced damage.
        if (dealerPlayer == null && (result.Props & ValueProp.Unpowered) != 0)
        {
            dealerPlayer = DebuffSourceTracker.GetApplier(receiver);
        }

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
