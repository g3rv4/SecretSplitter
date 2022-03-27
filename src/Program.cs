using System.Reflection;
using SecretSplitter.Commands;
using Spectre.Console;

if (args.Length > 0 && args[0] == "version")
{
    var version = Assembly.GetExecutingAssembly().GetName().Version!;
    Console.WriteLine($"{version.Major}.{version.Minor}.{version.Revision}");
    return 0;
}

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

return 0;