using System;
using System.Numerics;
using SecretSharingDotNet.Cryptography;
using SecretSharingDotNet.Math;
using Spectre.Console;

namespace SecretSplitter.Commands;

internal sealed class RestoreCommand
{
    public static void Execute()
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
    }
}