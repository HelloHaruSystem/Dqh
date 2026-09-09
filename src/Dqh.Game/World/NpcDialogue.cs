namespace Dqh.Game.World;

/// <summary>
/// Canned dialogue for NPCs with nothing beyond plain lines to say, keyed by
/// the id used in the map data. An NPC with richer behavior (choices,
/// follow-up actions) is handled directly in <see cref="NpcBehaviors"/> instead.
/// </summary>
internal static class NpcDialogue
{
    private static readonly string[] DefaultLines = ["..."];

    private static readonly Dictionary<string, string[]> Lines = new()
    {
        ["dock_worker"] = ["No boats today. The tide's against us."],
    };

    public static IReadOnlyList<string> LinesFor(string npcId) => Lines.GetValueOrDefault(npcId, DefaultLines);
}
