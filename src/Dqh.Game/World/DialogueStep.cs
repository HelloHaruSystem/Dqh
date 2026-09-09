namespace Dqh.Game.World;

/// <summary>One step of an on-screen conversation: a line to show, or a yes/no choice to make.</summary>
internal abstract record DialogueStep;

internal sealed record DialogueLine(string Text) : DialogueStep;

internal sealed record DialogueChoice(string Prompt, string YesLabel, string NoLabel, Action OnYes, Action? OnNo) : DialogueStep;
