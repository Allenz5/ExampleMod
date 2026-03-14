using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;

public static class DamageTracker
{
    private static readonly Dictionary<Player, int> _damageDealt = new();
    private static readonly Dictionary<Player, int> _damageTaken = new();

    public static void RecordDealt(Player player, int amount)
    {
        _damageDealt.TryGetValue(player, out int current);
        _damageDealt[player] = current + amount;
    }

    public static void RecordTaken(Player player, int amount)
    {
        _damageTaken.TryGetValue(player, out int current);
        _damageTaken[player] = current + amount;
    }

    public static int GetDealt(Player player)
    {
        _damageDealt.TryGetValue(player, out int val);
        return val;
    }

    public static int GetTaken(Player player)
    {
        _damageTaken.TryGetValue(player, out int val);
        return val;
    }
}
