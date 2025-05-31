using UsingUsings.Core;
using System.IO.Abstractions;

var fileSystem = new FileSystem();

var aggregation = await UsingDirectiveAggregator.AggregateAsync(
	fileSystem.DirectoryInfo.New(args.Length > 0 ? args[0] : Environment.CurrentDirectory),
	update => Console.WriteLine(update));

Console.WriteLine();

foreach (var result in aggregation.OrderByDescending(pair => pair.Value))
{
	Console.WriteLine($"{result.Key} - {result.Value:P}");
}