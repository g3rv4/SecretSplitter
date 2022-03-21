using SecretSplitter.Commands;
using Spectre.Console;

var action = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("What do you want to do?")
        .AddChoices(new[] { "Split a secret", "Restore a secret" }));

if (action == "Split a secret")
{
    await SplitCommand.ExecuteAsync();
}
else
{
    RestoreCommand.Execute();
}
