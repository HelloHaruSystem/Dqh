using Dqh.Game.Input;
using Dqh.Game.Presentation;
using Dqh.Game.Timing;
using Dqh.Game.World;

namespace Dqh.Game;

/// <summary>
/// The tick loop itself, independent of Raylib. Identical for the windowed and
/// </summary>
internal static class GameLoop
{
    public static void Run(GameWorld world, PlayerMarker player, IInputSource input, IWorldPresenter presenter, IClock clock)
    {
        presenter.Present(world, player, clock.DeltaSeconds);

        while (!input.IsQuitRequested)
        {
            Update(world, player, input, clock.DeltaSeconds);
            presenter.Present(world, player, clock.DeltaSeconds);
        }
    }

    private static void Update(GameWorld world, PlayerMarker player, IInputSource input, float deltaSeconds)
    {
        if (world.IsShowingWelcome)
        {
            if (input.TryGetConfirm()) world.DismissWelcome();
            return;
        }

        player.Tick(deltaSeconds);
        world.Tick(deltaSeconds, player);

        // Movement is locked out for the duration of a map transition — the
        // fade itself is the only thing that should be happening on screen.
        if (world.Transition.IsActive) return;

        // Polled every tick regardless of state, so a confirm press can never
        // sit queued up and block later input — it's simply a no-op when
        // there's nothing to advance or interact with.
        var confirmPressed = input.TryGetConfirm();

        if (world.ActiveBattle is { } battle)
        {
            if (battle.IsFinished)
            {
                world.EndBattle(player);
                return;
            }

            if (confirmPressed)
            {
                battle.HandleConfirm();
            }
            else if (input.TryGetMove(deltaSeconds, out _, out var battleRowDelta) && battleRowDelta != 0)
            {
                battle.HandleMove(battleRowDelta);
            }

            return;
        }

        if (world.Conversation.IsTalking)
        {
            // Only one of TryGetConfirm/TryGetMove reads fresh input per tick.
            if (world.Conversation.ActiveChoice is not null)
            {
                if (confirmPressed)
                {
                    world.Conversation.CommitChoice();
                }
                else if (input.TryGetMove(deltaSeconds, out _, out var talkingRowDelta) && talkingRowDelta != 0)
                {
                    world.Conversation.ToggleChoiceSelection();
                }
            }
            else if (confirmPressed)
            {
                world.Conversation.AdvanceDialogue();
            }
            else
            {
                input.TryGetMove(deltaSeconds, out _, out _);
            }

            return;
        }

        if (confirmPressed)
        {
            world.TryInteractWithFaced(player);
            return;
        }

        if (input.TryGetMove(deltaSeconds, out var columnDelta, out var rowDelta))
        {
            player.Move(columnDelta, rowDelta);
            world.CheckPortal(player);
            world.CheckEncounter(player);
        }
    }
}
