using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using SecretSharingDotNet.Cryptography;
using SecretSharingDotNet.Math;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SecretSplitter.Cli.Commands;

internal sealed class RestoreCommand : Command<RestoreCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
    }
    
    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        var parts = AnsiConsole.Prompt(
            new TextPrompt<int>("How many parts do you have?")
                .PromptStyle("green")
                .ValidationErrorMessage("[red]That's not a valid number[/]")
                .Validate(parts =>
                {
                    return parts switch
                    {
                        < 2 => ValidationResult.Error("[red]You need at least 2 parts[/]"),
                        _ => ValidationResult.Success(),
                    };
                }));

        var shares = new string[parts];
        var i = 0;
        do
        {
            shares[i] = AnsiConsole.Prompt(new TextPrompt<string>($"Enter the share {i}:")
                .PromptStyle("green"));
        } while (++i < parts);

        var gcd = new ExtendedEuclideanAlgorithm<BigInteger>();
        var combine = new ShamirsSecretSharing<BigInteger>(gcd);
        try
        {
            var recoveredSecret = combine.Reconstruction(shares);
            AnsiConsole.MarkupLine($"Your secret is [green]{recoveredSecret}[/]");
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine("[red]Could not reconstruct the secret[/]. Exception: " + e.Message);
        }

        return 0;
    }
}