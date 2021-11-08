using NUnit.Framework;

namespace UsingUsings.Core.Tests
{
	public static class UsingDirectiveAnalyzerTests
	{
		[Test]
		public static async Task AnalyzeAsync() => await Task.CompletedTask.ConfigureAwait(false);
	}
}