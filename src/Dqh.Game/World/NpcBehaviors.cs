namespace Dqh.Game.World;

/// <summary>Per-NPC interaction behavior, dispatched by id — everything an <see cref="NpcData"/> doesn't carry as plain data.</summary>
internal static class NpcBehaviors
{
    // The inn's own layout: the floor tile beside its first bed, for the
    // player to wake up next to after choosing to rest.
    private const int InnBedsideColumn = 3;
    private const int InnBedsideRow = 1;

    public static void Interact(NpcData npc, GameWorld world, PlayerMarker player)
    {
        if (npc.Id == "innkeeper")
        {
            Innkeeper(world);
            return;
        }

        world.QueueDialogueLines(NpcDialogue.LinesFor(npc.Id));
    }

    private static void Innkeeper(GameWorld world)
    {
        world.QueueDialogueLines(["Welcome, traveler!"]);
        world.QueueChoice(
            "Would you like to stay the night and rest?",
            yesLabel: "Yes",
            noLabel: "No",
            onYes: () => world.StartRelocation(InnBedsideColumn, InnBedsideRow),
            onNo: () => world.QueueDialogueLines(["Safe travels, then."]));
    }
}
