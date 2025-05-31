using NUnit.Framework;

namespace UsingUsings.Core.Tests;

internal static class UsingDirectiveDetectorTests
{
	[Test]
	public static void Detect()
	{
		var code =
@"using NUnit.Framework;
using System;

public static class Stuff { }";

		var detector = new UsingDirectiveDetector(code);

		Assert.Multiple(() =>
		{
			var directives = detector.Directives;
			Assert.That(directives.Count, Is.EqualTo(2));
			Assert.That(directives, Has.Member("NUnit.Framework"));
			Assert.That(directives, Has.Member("System"));
		});
	}

	[Test]
	public static void DetectWhenCodeContainsMultipleDirectives()
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

		var detector = new UsingDirectiveDetector(code);

		Assert.Multiple(() =>
		{
			var directives = detector.Directives;
			Assert.That(directives.Count, Is.EqualTo(2));
			Assert.That(directives, Has.Member("NUnit.Framework"));
			Assert.That(directives, Has.Member("System"));
		});
	}

	[Test]
	public static void DetectWhenCodeHasGlobalNamespace()
	{
		var code =
@"using NUnit.Framework;
using System;

namespace StuffNamespace;

using global::System;

public static class Stuff 
{ 
	public static class MoreStuff { }
}";

		var detector = new UsingDirectiveDetector(code);

		Assert.Multiple(() =>
		{
			var directives = detector.Directives;
			Assert.That(directives.Count, Is.EqualTo(2));
			Assert.That(directives, Has.Member("NUnit.Framework"));
			Assert.That(directives, Has.Member("System"));
		});
	}
}