using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace UsingUsings.Core;

public static class UsingDirectiveAggregator
{
	public static ImmutableDictionary<string, double> Aggregate(DirectoryInfo directory, TextWriter writer)
	{
		ArgumentNullException.ThrowIfNull(directory);
		ArgumentNullException.ThrowIfNull(writer);

		var fileCount = 0;
		var directiveCounts = new ConcurrentDictionary<string, uint>();

		foreach (var file in directory.EnumerateFiles("*.cs", SearchOption.AllDirectories))
		{
			writer.WriteLine($"Analyzing {file.FullName}...");
			fileCount++;
			var analyzer = new UsingDirectiveDetector(File.ReadAllText(file.FullName));

			foreach (var directive in analyzer.Directives)
			{
				directiveCounts.AddOrUpdate(directive, 1, (key, value) => value + 1);
			}
		}

		var results = ImmutableDictionary.CreateBuilder<string, double>();

		foreach (var directiveCount in directiveCounts)
		{
			results.Add(directiveCount.Key, (double)directiveCount.Value / fileCount);
		}

		return results.ToImmutable();
	}
}