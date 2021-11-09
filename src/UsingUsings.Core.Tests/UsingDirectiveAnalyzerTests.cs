using NUnit.Framework;

namespace UsingUsings.Core.Tests;

public static class UsingDirectiveAnalyzerTests
{
	[Test]
	public static void Analyze()
	{
		var code =
@"using NUnit.Framework;
using System;

public static class Stuff { }";

		var analyzer = new UsingDirectiveAnalyzer(code);

		Assert.Multiple(() =>
		{
			var directives = analyzer.Directives;
			Assert.That(directives.Count, Is.EqualTo(2));
			Assert.That(directives, Has.Member("NUnit.Framework"));
			Assert.That(directives, Has.Member("System"));
		});
	}

	[Test]
	public static void AnalyzeWhenCodeContainsMultipleDirectives()
	{
		var code =
@"using NUnit.Framework;
using System;

namespace StuffNamespace;

using System;

public static class Stuff 
{ 
	public static class MoreStuff { }
}";

		var analyzer = new UsingDirectiveAnalyzer(code);

		Assert.Multiple(() =>
		{
			var directives = analyzer.Directives;
			Assert.That(directives.Count, Is.EqualTo(2));
			Assert.That(directives, Has.Member("NUnit.Framework"));
			Assert.That(directives, Has.Member("System"));
		});
	}
}