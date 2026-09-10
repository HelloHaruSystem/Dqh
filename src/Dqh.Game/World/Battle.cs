using Dqh.Domain.Abilities;
using Dqh.Domain.Battles;
using Dqh.Domain.Combatants.Adventurers;
using Dqh.Domain.Combatants.Monsters;
using Dqh.Domain.Encounters;
using Dqh.Domain.Magic;
using Dqh.Domain.Party;

namespace Dqh.Game.World;

/// <summary>
/// One fight: command a living party member each round. 
/// </summary>
internal sealed class Battle
{
    public enum Outcome { None, Won, Lost, Fled }

    private readonly Encounter _encounter;
    private readonly Party _party;
    private readonly IBattleResolver _resolver;
    private readonly List<IMonster> _activeMonsters;
    private readonly List<BattleCommand> _roundCommands = [];
    private readonly Conversation _log = new();

    private BattleMenu? _menu;
    private Action<int>? _onMenuSelect;
    private Adventurer? _currentActor;

    public Outcome Result { get; private set; } = Outcome.None;

    public IReadOnlyList<IMonster> ActiveMonsters => _activeMonsters;
    public IReadOnlyList<Adventurer> PartyMembers => _party.Members;

    public bool IsShowingLog => _log.IsTalking;
    public string? LogLine => _log.ActiveDialogueLine;

    public bool IsShowingMenu => _menu is not null;
    public IReadOnlyList<string> MenuOptions => _menu?.Options ?? [];
    public int SelectedMenuIndex => _menu?.SelectedIndex ?? 0;

    /// <summary>The party member currently choosing a command, for as long as any menu (main, spell, or target) is up for them.</summary>
    public Adventurer? CurrentActor => _currentActor;

    /// <summary>The battle is done — its outcome is decided and the log has finished paging.</summary>
    public bool IsFinished => Result != Outcome.None && !_log.IsTalking;

    public Battle(Encounter encounter, Party party, IBattleResolver resolver)
    {
        _encounter = encounter;
        _party = party;
        _resolver = resolver;
        _activeMonsters = encounter.Monsters.ToList();

        StartRound();
    }

    public void HandleConfirm()
    {
        if (_log.IsTalking)
        {
            _log.AdvanceDialogue();
            if (!_log.IsTalking && Result == Outcome.None) StartRound();
            return;
        }

        if (_menu is { } menu) _onMenuSelect?.Invoke(menu.SelectedIndex);
    }

    public void HandleMove(int rowDelta)
    {
        if (_log.IsTalking || rowDelta == 0) return;

        _menu?.MoveCursor(rowDelta > 0 ? 1 : -1);
    }

    private void StartRound()
    {
        foreach (var member in _party.Members) member.ResetGuard();
        AdvanceToNextActorOrResolve();
    }

    private void AdvanceToNextActorOrResolve()
    {
        var next = _party.Members.FirstOrDefault(a => !a.IsDefeated && _roundCommands.All(c => c.Actor != a));
        if (next is not null)
        {
            ShowMainMenu(next);
        }
        else
        {
            ResolveRound();
        }
    }

    private void ShowMainMenu(Adventurer actor)
    {
        _currentActor = actor;

        var options = new List<string> { "Attack" };
        if (actor is ISpellcaster) options.Add("Spell");
        if (actor is IHealer) options.Add("Heal");
        if (actor is IDefender) options.Add("Defend");
        if (actor is IFleeable) options.Add("Run");

        _menu = new BattleMenu(options);
        _onMenuSelect = index => HandleMainMenuSelect(actor, options[index]);
    }

    private void HandleMainMenuSelect(Adventurer actor, string option)
    {
        switch (option)
        {
            case "Attack":
                ShowMonsterTargetMenu(target => AddCommand(new AttackCommand(actor, target)));
                break;
            case "Spell":
                ShowSpellMenu(actor);
                break;
            case "Heal":
                ShowHealSpellMenu(actor);
                break;
            case "Defend":
                AddCommand(new GuardCommand(actor));
                break;
            case "Run":
                AddCommand(new FleeCommand(actor));
                break;
        }
    }

    private void ShowSpellMenu(Adventurer actor)
    {
        var spells = ((ISpellcaster)actor).KnownSpells;
        _menu = new BattleMenu(spells.Select(s => $"{s.Name} ({s.ManaCost} MP)").ToList());
        _onMenuSelect = index => ShowMonsterTargetMenu(target => AddCommand(new CastCommand(actor, spells[index], target)));
    }

    private void ShowHealSpellMenu(Adventurer actor)
    {
        var spells = ((IHealer)actor).KnownHealingSpells;
        _menu = new BattleMenu(spells.Select(s => $"{s.Name} ({s.ManaCost} MP)").ToList());
        _onMenuSelect = index => ShowAllyTargetMenu(target => AddCommand(new HealCommand(actor, spells[index], target)));
    }

    private void ShowMonsterTargetMenu(Action<IMonster> onPicked)
    {
        var targets = _activeMonsters.Where(m => !m.IsDefeated).ToList();
        if (targets.Count == 1)
        {
            onPicked(targets[0]);
            return;
        }

        _menu = new BattleMenu(targets.Select(m => $"{m.Name} ({m.CurrentHitPoints}/{m.MaxHitPoints} HP)").ToList());
        _onMenuSelect = index => onPicked(targets[index]);
    }

    private void ShowAllyTargetMenu(Action<Adventurer> onPicked)
    {
        var allies = _party.Members.Where(a => !a.IsDefeated).ToList();
        _menu = new BattleMenu(allies.Select(a => $"{a.Name} ({a.CurrentHitPoints}/{a.MaxHitPoints} HP)").ToList());
        _onMenuSelect = index => onPicked(allies[index]);
    }

    private void AddCommand(BattleCommand command)
    {
        _roundCommands.Add(command);
        _menu = null;
        _onMenuSelect = null;
        AdvanceToNextActorOrResolve();
    }

    private void ResolveRound()
    {
        _currentActor = null;
        var result = _resolver.ResolveRound(_party, _activeMonsters, _roundCommands);
        _roundCommands.Clear();
        _activeMonsters.RemoveAll(m => result.FledMonsters.Contains(m));
        _log.QueueDialogueLines(result.Log);

        if (result.PartyFled)
        {
            Result = Outcome.Fled;
            _log.QueueDialogueLines(["The party got away safely!"]);
        }
        else if (_activeMonsters.Count == 0 || _activeMonsters.All(m => m.IsDefeated))
        {
            // _activeMonsters can be empty because every last one fled rather than
            // was defeated — All() on an empty sequence is vacuously true, so check
            // the full original roster to word the message accurately either way.
            Result = Outcome.Won;
            _party.ResolveEncounter(_encounter, _ => { });
            _log.QueueDialogueLines([_encounter.Monsters.Any(m => m.IsDefeated)
                ? "The monsters have been defeated!"
                : "The monsters flee the battlefield!"]);
        }
        else if (_party.Members.All(a => a.IsDefeated))
        {
            Result = Outcome.Lost;
            _log.QueueDialogueLines(["Your party has been defeated..."]);
        }
    }
}
