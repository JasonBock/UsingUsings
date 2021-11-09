using UsingUsings.Core;

var aggregation = UsingDirectiveAggregator.Aggregate(
	new DirectoryInfo(args.Length > 0 ? args[0] : Environment.CurrentDirectory), Console.Out);

foreach (var result in aggregation.OrderByDescending(pair => pair.Value))
{
	Console.WriteLine($"{result.Key} - {result.Value:P}");
}