using SecretSplitter.Cli.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(config =>
{
    config.AddCommand<SplitCommand>("split");
    config.AddCommand<RestoreCommand>("restore");
});

return app.Run(args);