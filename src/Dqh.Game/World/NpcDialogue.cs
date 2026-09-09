namespace Dqh.Game.World;

/// <summary>Canned dialogue for each NPC, keyed by the id used in the map data.</summary>
internal static class NpcDialogue
{
    private static readonly string[] DefaultLines = ["..."];

    private static readonly Dictionary<string, string[]> Lines = new()
    {
        ["innkeeper"] = ["Welcome, traveler! Rest here whenever your party needs mending."],
        ["dock_worker"] = ["No boats today. The tide's against us."],
    };

    public static IReadOnlyList<string> LinesFor(string npcId) => Lines.GetValueOrDefault(npcId, DefaultLines);
}
