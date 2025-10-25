using System.Diagnostics;
using LargeDictionary.Core;
using Spectre.Console;


ShowWelcome();

const string demoMenuChoice = "LargeDictionary<long>";
const string compareMenuChoice = "Compare LargeDictionary with Dictionary<long, long>";
const string exitMenuChoice = "Exit";

while(true)
{
    var choice = ShowMenu();
    switch (choice)
    {
        case demoMenuChoice:
            StartTestLargeDict();
            break;
        case compareMenuChoice:
            ComparePerformance();
            break;
        case exitMenuChoice:
            ShowGoodbye();
            return;
    }
    
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
    Console.ReadKey();
}

static void ShowWelcome()
{
    AnsiConsole.Clear();

    var panel = new Panel(
        new FigletText("LargeDictionary")
            .Centered()
            .Color(Color.Cyan1))
    {
        Border = BoxBorder.Double,
        BorderStyle = new Style(Color.Cyan1)
    };

    AnsiConsole.Write(panel);

    AnsiConsole.MarkupLine("[dim]Performance Test Demo[/]\n");
}

static string ShowMenu()
{
    var testType = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[bold cyan]Select test to run:[/]")
            .AddChoices(compareMenuChoice, demoMenuChoice, exitMenuChoice)
            .HighlightStyle(new Style(Color.Cyan1)));
    AnsiConsole.WriteLine();
    return testType;
}

static void StartTestLargeDict()
{
    var len = long.MaxValue >> 2;

    AnsiConsole.Write(new Rule("[bold yellow]Testing LargeDictionary<long>[/]").LeftJustified());
    AnsiConsole.WriteLine();

    var table = CreateResultsTable();
    var sw = new Stopwatch();
    var dict = new LargeDictionary<long>();

    AnsiConsole.Live(table)
        .Start(ctx =>
        {
            sw.Start();
            dict[0] = 0;
            for (long i = 1; i < len; i++)
            {
                dict[i] = i;
                if (i % 1_000_000 == 0)
                {
                    var elapsed = sw.Elapsed;
                    table.AddRow(
                        $"[cyan]{i:N0}[/]",
                        $"[yellow]{elapsed:mm\\:ss\\.fff}[/]",
                        $"[green]{(1_000_000 / elapsed.TotalSeconds):N0}[/] ops/sec");

                    ctx.Refresh();
                    sw.Restart();
                }
            }

            sw.Stop();
        });

    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine($"[bold green]✓ Test completed![/] Total elements: [cyan]{dict.Count:N0}[/]");
}

static Table CreateResultsTable()
{
    return new Table()
        .Border(TableBorder.Rounded)
        .BorderColor(Color.Grey)
        .AddColumn(new TableColumn("[bold]Elements[/]").Centered())
        .AddColumn(new TableColumn("[bold]Time[/]").Centered())
        .AddColumn(new TableColumn("[bold]Performance[/]").Centered());
}

static void ComparePerformance()
{
    const long elementCount = 100_000_000;

    AnsiConsole.Write(new Rule("[bold yellow]Performance Comparison[/]").LeftJustified());
    AnsiConsole.WriteLine();

    AnsiConsole.MarkupLine($"[dim]Comparing on {elementCount:N0} elements[/]");
    AnsiConsole.WriteLine();

    var comparisonTable = new Table()
        .Border(TableBorder.Rounded)
        .BorderColor(Color.Grey)
        .AddColumn(new TableColumn("[bold]Checkpoint[/]").Centered())
        .AddColumn(new TableColumn("[bold]Dictionary<long,long>[/]").Centered())
        .AddColumn(new TableColumn("[bold]LargeDictionary<long>[/]").Centered())
        .AddColumn(new TableColumn("[bold]Difference[/]").Centered());

    var dictTimes = new List<TimeSpan>();
    var largeDictTimes = new List<TimeSpan>();


    // Test Dictionary<long, long>
    AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .Start("[bold cyan]Testing Dictionary<long, long>...[/]", statusCtx =>
        {
            var sw = new Stopwatch();
            var dict = new Dictionary<long, long>();

            sw.Start();
            dict[0] = 0;
            for (long i = 1; i < elementCount; i++)
            {
                dict[i] = i;

                if (i % 10_000_000 == 0)
                {
                    dictTimes.Add(sw.Elapsed);
                    statusCtx.Status($"[bold cyan]Dictionary: {i:N0} / {elementCount:N0}[/]");
                    sw.Restart();
                }
            }

            sw.Stop();
        });

    AnsiConsole.MarkupLine("[bold green]✓[/] Dictionary<long, long> completed");
    AnsiConsole.WriteLine();

    // Test LargeDictionary<long>
    AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .Start("[bold magenta]Testing LargeDictionary<long>...[/]", statusCtx =>
        {
            var sw = new Stopwatch();
            var largeDict = new LargeDictionary<long>();

            sw.Start();
            largeDict[0] = 0;
            for (long i = 1; i < elementCount; i++)
            {
                largeDict[i] = i;

                if (i % 10_000_000 == 0)
                {
                    largeDictTimes.Add(sw.Elapsed);
                    statusCtx.Status($"[bold magenta]LargeDictionary: {i:N0} / {elementCount:N0}[/]");
                    sw.Restart();
                }
            }

            sw.Stop();
        });

    AnsiConsole.MarkupLine("[bold green]✓[/] LargeDictionary<long> completed");
    AnsiConsole.WriteLine();

    // Build comparison table
    for (int i = 0; i < dictTimes.Count; i++)
    {
        var checkpoint = (i + 1) * 10_000_000;
        var dictTime = dictTimes[i];
        var largeTime = largeDictTimes[i];
        var diff = largeTime - dictTime;
        var diffMs = diff.TotalMilliseconds;

        var diffColor = diffMs < 0 ? "green" : diffMs < 100 ? "yellow" : "red";
        var diffSymbol = diffMs < 0 ? "▼" : "▲";
        var diffSign = diffMs < 0 ? "" : "+";

        comparisonTable.AddRow(
            $"[cyan]{checkpoint:N0}[/]",
            $"[blue]{dictTime:ss\\.fff}s[/]",
            $"[magenta]{largeTime:ss\\.fff}s[/]",
            $"[{diffColor}]{diffSymbol} {diffSign}{diffMs:F0}ms[/]");
    }

    AnsiConsole.WriteLine();

    // Summary
    var totalDict = TimeSpan.FromTicks(dictTimes.Sum(t => t.Ticks));
    var totalLarge = TimeSpan.FromTicks(largeDictTimes.Sum(t => t.Ticks));
    var totalDiff = totalLarge - totalDict;
    var ratio = totalLarge.TotalMilliseconds / totalDict.TotalMilliseconds;

    var summaryColor = ratio < 1 ? "green" : ratio < 1.1 ? "yellow" : "red";

    var summaryPanel = new Panel(
        new Markup(
            $"[bold]Performance Summary[/]\n\n" +
            $"Dictionary<long, long>:  [blue]{totalDict:mm\\:ss\\.fff}[/]\n" +
            $"LargeDictionary<long>:   [magenta]{totalLarge:mm\\:ss\\.fff}[/]\n\n" +
            $"Difference: [{(totalDiff.TotalMilliseconds < 0 ? "green" : "red")}]" +
            $"{(totalDiff.TotalMilliseconds < 0 ? "" : "+")}{totalDiff.TotalMilliseconds:F0}ms[/]\n" +
            $"Ratio: [{summaryColor}]{ratio:F3}x[/]"))
    {
        Border = BoxBorder.Double,
        BorderStyle = new Style(Color.Cyan1),
        Padding = new Padding(2, 1)
    };

    AnsiConsole.Write(summaryPanel);

    // Performance chart
    AnsiConsole.WriteLine();
    var chart = new BarChart()
        .Width(60)
        .Label("[bold underline]Average Time per 10M Operations[/]")
        .CenterLabel();

    var avgDict = dictTimes.Average(t => t.TotalMilliseconds);
    var avgLarge = largeDictTimes.Average(t => t.TotalMilliseconds);

    chart.AddItem("Dictionary", avgDict, Color.Blue);
    chart.AddItem("LargeDictionary", avgLarge, Color.Magenta1);

    AnsiConsole.Write(chart);
}

static void ShowGoodbye()
{
    AnsiConsole.Clear();
    AnsiConsole.MarkupLine("\n[cyan]Thank you for using LargeDictionary Demo![/]\n");
}