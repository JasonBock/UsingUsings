using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace UsingUsings.Core;

public sealed class UsingDirectiveAnalyzer
{
	public UsingDirectiveAnalyzer(string code)
	{
		if (code is null)
		{
			throw new ArgumentNullException(nameof(code));
		}

		var directives = new HashSet<string>();
		var unit = SyntaxFactory.ParseCompilationUnit(code);

		foreach (var directive in unit.DescendantNodes(_ => true).OfType<UsingDirectiveSyntax>())
		{
			directives.Add(directive.Name.ToString());
		}

		this.Directives = directives.ToImmutableHashSet();
	}

	public ImmutableHashSet<string> Directives { get; }
}