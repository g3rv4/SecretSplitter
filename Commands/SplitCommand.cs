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
        var title = AnsiConsole.Prompt(new TextPrompt<string>("Enter a title for the secret: "));

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
        
        var groups = AnsiConsole.Prompt(
            new TextPrompt<string>("[gray]Optional[/] How do you want to group the results?")
                .ValidationErrorMessage($"[red]Please enter comma separated values, they should add up {totalParts}[/]")
                .AllowEmpty()
                .Validate(groups =>
                {
                    int[] numbers;
                    try
                    {
                        numbers = groups.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                            .Select(e => int.Parse(e))
                            .ToArray();
                    }
                    catch (Exception)
                    {
                        return ValidationResult.Error($"[red]Please enter comma separated values that are numeric[/]");
                    }

                    return numbers.Sum() == totalParts
                        ? ValidationResult.Success()
                        : ValidationResult.Error($"[red]The parts should add up to {totalParts}[/]");
                }));

        int[] sharesPerGroup;
        if (string.IsNullOrEmpty(groups))
        {
            sharesPerGroup = new int[totalParts];
            Array.Fill(sharesPerGroup, 1);
        }
        else
        {
            sharesPerGroup = groups.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(e => int.Parse(e))
                .ToArray();
        }

        var secret = AnsiConsole.Prompt(new TextPrompt<string>("Enter the secret to split:")
            .PromptStyle("red"));
        
        var gcd = new ExtendedEuclideanAlgorithm<BigInteger>();

        var split = new ShamirsSecretSharing<BigInteger>(gcd, 521);
        var shares = split.MakeShares(requiredParts, totalParts, secret);
        
        var qrGenerator = new QRCodeGenerator();
        var currentShare = 0;
        var currentGroup = 1;
        foreach (var quantity in sharesPerGroup)
        {
            var txt = $@"
<html>
<body>
  <center>
    <h1>{title}</h1>
    <h2>{DateTime.UtcNow.ToString("yyyy-MM-dd HH\\:mm\\:ss")}</h2>
<table>
";
            for (var i = 0; i < quantity; i++)
            {
                var share = shares[currentShare];
                
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(share.ToString(), QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrCodeData);
                var image = qrCode.GetGraphic(20);

                txt += $@"
<tr>
<td>
<img src=""data:image/png;base64, {Convert.ToBase64String(image)}"" style=""max-width: 300px; max-height: 300px"" />
</td>
<td style=""overflow-wrap: break-word;max-width: 500px;"">
    {share.ToString()}
</td>";

                currentShare++;
            }
            
            txt += $@"
</table>    
</body>
</html>";
            File.WriteAllText($"group{currentGroup++}.html", txt);
        }
        
        return 0;
    }
}