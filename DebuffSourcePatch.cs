using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

// Tracks which player applied a debuff to which monster so that status-effect
// damage (e.g. poison ticking) can be attributed to the correct player.
public static class DebuffSourceTracker
{
    // ConditionalWeakTable lets the Creature be GC'd once it's no longer alive,
    // preventing memory leaks across combats.
    private static readonly ConditionalWeakTable<Creature, Player> _appliers = new();

    public static void RecordApplier(Creature monster, Player player)
    {
        // AddOrUpdate isn't available on all runtimes; remove then add.
        _appliers.Remove(monster);
        _appliers.Add(monster, player);
    }

    public static Player? GetApplier(Creature monster)
    {
        _appliers.TryGetValue(monster, out Player? player);
        return player;
    }
}

[HarmonyPatch(typeof(CombatHistory), nameof(CombatHistory.PowerReceived))]
public static class DebuffSourcePatch
{
    static void Postfix(PowerModel power, Creature? applier)
    {
        // Only care when a player applies a debuff to a monster.
        if (power.Type == PowerType.Debuff
            && applier?.Player != null
            && power.Owner?.IsMonster == true)
        {
            DebuffSourceTracker.RecordApplier(power.Owner, applier.Player);
        }
    }
}
