using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using QRCoder;
using SecretSharingDotNet.Cryptography;
using SecretSharingDotNet.Math;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SecretSplitter.Cli.Commands;

internal sealed class SplitCommand : Command<SplitCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        int totalParts, requiredParts;
        do
        {
            totalParts = AnsiConsole.Prompt(
                new TextPrompt<int>("How many parts do you want to split the secret on?")
                    .PromptStyle("green")
                    .ValidationErrorMessage("[red]That's not a valid number[/]")
                    .Validate(parts =>
                    {
                        return parts switch
                        {
                            < 3 => ValidationResult.Error("[red]You must at least split it in 3 parts[/]"),
                            _ => ValidationResult.Success(),
                        };
                    }));

            requiredParts = AnsiConsole.Prompt(
                new TextPrompt<int>("How many parts are needed to restore the secret?")
                    .PromptStyle("green")
                    .ValidationErrorMessage("[red]That's not a valid number[/]")
                    .Validate(partsNeeded =>
                    {
                        if (partsNeeded > totalParts)
                        {
                            return ValidationResult.Error($"[red]You are splitting the secret in {totalParts}, you can't require more than that[/]");
                        }

                        return partsNeeded switch
                        {
                            < 2 => ValidationResult.Error("[red]You must at least need 2 parts[/]"),
                            _ => ValidationResult.Success(),
                        };
                    }));
        } while (!AnsiConsole.Confirm($"Split the secret in {totalParts} requiring {requiredParts}?"));
        
        var secret = AnsiConsole.Prompt(new TextPrompt<string>("Enter the secret to split:")
            .PromptStyle("red"));
        
        var gcd = new ExtendedEuclideanAlgorithm<BigInteger>();

        var split = new ShamirsSecretSharing<BigInteger>(gcd, 521);
        var shares = split.MakeShares(requiredParts, totalParts, secret);
        
        var qrGenerator = new QRCodeGenerator();
        int i = 1;
        foreach (var share in shares)
        {
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(share.ToString(), QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var image = qrCode.GetGraphic(20);

            var txt = $@"
<html>
<body>
  <center>
    <img src=""data:image/png;base64, {Convert.ToBase64String(image)}"" style=""max-width: 300px; max-height: 300px"" />
    <br />
    <span>{share.ToString()}</span>
</body>
</html>
";
            File.WriteAllText($"share{i++}.html", txt);
        }

        return 0;
    }
}