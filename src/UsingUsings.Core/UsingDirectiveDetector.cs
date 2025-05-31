using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace UsingUsings.Core;

public sealed class UsingDirectiveDetector
{
	public UsingDirectiveDetector(string code)
	{
		ArgumentNullException.ThrowIfNull(code);

		var directives = new HashSet<string>();
		var unit = SyntaxFactory.ParseCompilationUnit(code);

		foreach (var directive in unit.DescendantNodes(_ => true).OfType<UsingDirectiveSyntax>())
		{
			directives.Add(directive.Name!.ToString()
				.Replace("global::", string.Empty, StringComparison.InvariantCulture));
		}

		this.Directives = [.. directives];
	}

	public ImmutableHashSet<string> Directives { get; }
}