using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;

[HarmonyPatch(typeof(NRun), "_Ready")]
public static class MultiplayerSidebarPatch
{
    private static readonly FieldInfo StateField =
        typeof(NRun).GetField("_state", BindingFlags.NonPublic | BindingFlags.Instance);

    static void Postfix(NRun __instance)
    {
        var runState = StateField?.GetValue(__instance) as RunState;
        if (runState == null) return;

        var sidebar = new MultiplayerSidebarNode();
        __instance.AddChild(sidebar);
        sidebar.Initialize(runState);
    }
}
