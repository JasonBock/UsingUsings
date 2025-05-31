using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO.Abstractions;

namespace UsingUsings.Core;

public static class UsingDirectiveAggregator
{
	public static async Task<ImmutableDictionary<string, double>> AggregateAsync(
		IDirectoryInfo directory, Action<string> analyzingUpdate)
	{
		ArgumentNullException.ThrowIfNull(directory);
		ArgumentNullException.ThrowIfNull(analyzingUpdate);

		var fileCount = 0;
		var directiveCounts = new ConcurrentDictionary<string, uint>();

		foreach (var file in directory.EnumerateFiles("*.cs", SearchOption.AllDirectories))
		{
			analyzingUpdate($"Analyzing {file.FullName}...");
			fileCount++;

			var analyzer = new UsingDirectiveDetector(
				await directory.FileSystem.File.ReadAllTextAsync(file.FullName));

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