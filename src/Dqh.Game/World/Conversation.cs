namespace Dqh.Game.World;

/// <summary>An on-screen conversation: a queue of lines/choices, paged one at a time.</summary>
internal sealed class Conversation
{
    private readonly Queue<DialogueStep> _steps = new();

    /// <summary>The line currently on screen, or null when the current step isn't a plain line.</summary>
    public string? ActiveDialogueLine => _steps.Count > 0 && _steps.Peek() is DialogueLine line ? line.Text : null;

    /// <summary>The choice currently on screen, or null when the current step isn't a choice.</summary>
    public DialogueChoice? ActiveChoice => _steps.Count > 0 && _steps.Peek() is DialogueChoice choice ? choice : null;

    public bool IsTalking => _steps.Count > 0;

    /// <summary>Which option is highlighted while <see cref="ActiveChoice"/> is showing.</summary>
    public bool ChoiceYesSelected { get; private set; } = true;

    /// <summary>Queues plain lines to show, one at a time.</summary>
    public void QueueDialogueLines(IEnumerable<string> lines)
    {
        foreach (var text in lines)
        {
            _steps.Enqueue(new DialogueLine(text));
        }
    }

    /// <summary>Queues a yes/no choice, run once the player picks an option.</summary>
    public void QueueChoice(string prompt, string yesLabel, string noLabel, Action onYes, Action? onNo = null) =>
        _steps.Enqueue(new DialogueChoice(prompt, yesLabel, noLabel, onYes, onNo));

    /// <summary>Flips which option is highlighted while a choice is showing.</summary>
    public void ToggleChoiceSelection() => ChoiceYesSelected = !ChoiceYesSelected;

    /// <summary>Dismisses the current line, revealing the next step if there is one. A no-op unless a plain line is showing.</summary>
    public void AdvanceDialogue()
    {
        if (_steps.Count > 0 && _steps.Peek() is DialogueLine) _steps.Dequeue();
    }

    /// <summary>Commits the highlighted option of the current choice, running its action. A no-op unless a choice is showing.</summary>
    public void CommitChoice()
    {
        if (_steps.Count == 0 || _steps.Peek() is not DialogueChoice choice) return;

        _steps.Dequeue();
        var pickedYes = ChoiceYesSelected;
        ChoiceYesSelected = true;
        (pickedYes ? choice.OnYes : choice.OnNo)?.Invoke();
    }
}
