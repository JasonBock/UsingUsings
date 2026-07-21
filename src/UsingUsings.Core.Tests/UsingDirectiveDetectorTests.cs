using NUnit.Framework;

namespace UsingUsings.Core.Tests;

internal static class UsingDirectiveDetectorTests
{
	[Test]
	public static void DetectWithAlias()
	{
		var code =
			"""
			using TupleAlias = (string[] names, Guid id);
			""";

		var detector = new UsingDirectiveDetector(code);

		using (Assert.EnterMultipleScope())
		{
			var directives = detector.Directives;
			Assert.That(directives, Is.Empty);
		}
	}

	[Test]
	public static void Detect()
	{
		var code =
			"""
			using NUnit.Framework;
			using System;

			public static class Stuff { }
			""";

		var detector = new UsingDirectiveDetector(code);

		using (Assert.EnterMultipleScope())
		{
			var directives = detector.Directives;
			Assert.That(directives, Has.Count.EqualTo(2));
			Assert.That(directives, Has.Member("NUnit.Framework"));
			Assert.That(directives, Has.Member("System"));
		}
	}

	[Test]
	public static void DetectWhenCodeContainsMultipleDirectives()
	{
		var code =
			"""
			using NUnit.Framework;
			using System;

			namespace StuffNamespace;

			using System;

			public static class Stuff 
			{ 
			public static class MoreStuff { }
			}
			""";

		var detector = new UsingDirectiveDetector(code);

		using (Assert.EnterMultipleScope())
		{
			var directives = detector.Directives;
			Assert.That(directives, Has.Count.EqualTo(2));
			Assert.That(directives, Has.Member("NUnit.Framework"));
			Assert.That(directives, Has.Member("System"));
		}
	}

	[Test]
	public static void DetectWhenCodeHasGlobalNamespace()
	{
		var code =
			"""
			using NUnit.Framework;
			using System;

			namespace StuffNamespace;

			using global::System;

			public static class Stuff 
			{ 
			public static class MoreStuff { }
			}
			""";

		var detector = new UsingDirectiveDetector(code);

		using (Assert.EnterMultipleScope())
		{
			var directives = detector.Directives;
			Assert.That(directives, Has.Count.EqualTo(2));
			Assert.That(directives, Has.Member("NUnit.Framework"));
			Assert.That(directives, Has.Member("System"));
		}
	}
}