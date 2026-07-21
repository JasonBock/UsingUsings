using NUnit.Framework;
using Rocks;
using System.IO.Abstractions;

namespace UsingUsings.Core.Tests;

internal static class UsingDirectiveAggregatorTests
{
	[Test]
	public static async Task AggregateWhenDirectoryIsNullAsync() =>
		await Assert.ThatAsync(async () => await UsingDirectiveAggregator.AggregateAsync(null!, update => { }),
			Throws.TypeOf<ArgumentNullException>().With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("directory"));

	[Test]
	public static async Task AggregateWhenAnalyzingUpdateIsNullAsync() =>
		await Assert.ThatAsync(async () => await UsingDirectiveAggregator.AggregateAsync(new IDirectoryInfoMakeExpectations().Instance(), null!),
			Throws.TypeOf<ArgumentNullException>().With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("analyzingUpdate"));

	[Test]
	public static async Task AggregateWhenDirectoryHasNoFilesAsync()
	{
		using var context = new RockContext();
		var directoryExpectations = context.Create<IDirectoryInfoCreateExpectations>();
		directoryExpectations.Setups
			.EnumerateFiles("*.cs", SearchOption.AllDirectories)
			.ReturnValue([]);

		var results = await UsingDirectiveAggregator.AggregateAsync(directoryExpectations.Instance(), update => { });

		Assert.That(results, Is.Empty);
	}

	[Test]
	public static async Task AggregateWhenDirectoryHasOneFileWithNoUsingsAsync()
	{
		using var context = new RockContext();

		const string fileName = "code.cs";

		var fileInfoExpectations = context.Create<IFileInfoCreateExpectations>();
		fileInfoExpectations.Setups
			.FullName
			.Gets()
			.ExpectedCallCount(2)
			.ReturnValue(fileName);

		var fileExpectations = context.Create<IFileCreateExpectations>();
		fileExpectations.Setups
			.ReadAllTextAsync(fileName)
			.ReturnValue(Task.FromResult(
				"""
				public static class Stuff { }
				"""));

		var fileSystemExpectations = context.Create<IFileSystemCreateExpectations>();
		fileSystemExpectations.Setups
			.File
			.Gets()
			.ReturnValue(fileExpectations.Instance());

		var directoryExpectations = context.Create<IDirectoryInfoCreateExpectations>();
		directoryExpectations.Setups
			.EnumerateFiles("*.cs", SearchOption.AllDirectories)
			.ReturnValue([fileInfoExpectations.Instance()]);
		directoryExpectations.Setups
			.FileSystem
			.Gets()
			.ReturnValue(fileSystemExpectations.Instance());

		var results = await UsingDirectiveAggregator.AggregateAsync(directoryExpectations.Instance(), update => { });

		Assert.That(results, Is.Empty);
	}

	[Test]
	public static async Task AggregateWhenDirectoryHasOneFileWithUsingsAsync()
	{
		using var context = new RockContext();

		const string fileName = "code.cs";

		var fileInfoExpectations = context.Create<IFileInfoCreateExpectations>();
		fileInfoExpectations.Setups
			.FullName
			.Gets()
			.ExpectedCallCount(2)
			.ReturnValue(fileName);

		var fileExpectations = context.Create<IFileCreateExpectations>();
		fileExpectations.Setups
			.ReadAllTextAsync(fileName)
			.ReturnValue(Task.FromResult(
				"""
				using System;
				using System.Reflection;
				"""));

		var fileSystemExpectations = context.Create<IFileSystemCreateExpectations>();
		fileSystemExpectations.Setups
			.File
			.Gets()
			.ReturnValue(fileExpectations.Instance());

		var directoryExpectations = context.Create<IDirectoryInfoCreateExpectations>();
		directoryExpectations.Setups
			.EnumerateFiles("*.cs", SearchOption.AllDirectories)
			.ReturnValue([fileInfoExpectations.Instance()]);
		directoryExpectations.Setups
			.FileSystem
			.Gets()
			.ReturnValue(fileSystemExpectations.Instance());

		var updates = new List<string>();
		var results = await UsingDirectiveAggregator.AggregateAsync(directoryExpectations.Instance(),
			update => updates.Add(update));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(results, Has.Count.EqualTo(2));
			Assert.That(results["System"], Is.EqualTo(1.0));
			Assert.That(results["System.Reflection"], Is.EqualTo(1.0));
			Assert.That(updates, Has.Count.EqualTo(1));
			Assert.That(updates, Does.Contain($"Analyzing {fileName}..."));
		}
	}

	[Test]
	public static async Task AggregateWhenDirectoryHasMultipleFileWithUsingsAsync()
	{
		using var context = new RockContext();

		const string fileName = "code.cs";
		const string fileName2 = "code2.cs";

		var fileInfoExpectations = context.Create<IFileInfoCreateExpectations>();
		fileInfoExpectations.Setups
			.FullName
			.Gets()
			.ExpectedCallCount(2)
			.ReturnValue(fileName);

		var fileInfo2Expectations = context.Create<IFileInfoCreateExpectations>();
		fileInfo2Expectations.Setups
			.FullName
			.Gets()
			.ExpectedCallCount(2)
			.ReturnValue(fileName2);

		var fileExpectations = context.Create<IFileCreateExpectations>();
		fileExpectations.Setups
			.ReadAllTextAsync(fileName)
			.ReturnValue(Task.FromResult(
				"""
				using System;
				using System.Reflection;
				"""));
		fileExpectations.Setups
			.ReadAllTextAsync(fileName2)
			.ReturnValue(Task.FromResult(
				"""
				using System;
				"""));

		var fileSystemExpectations = context.Create<IFileSystemCreateExpectations>();
		fileSystemExpectations.Setups
			.File
			.Gets()
			.ExpectedCallCount(2)
			.ReturnValue(fileExpectations.Instance());

		var directoryExpectations = context.Create<IDirectoryInfoCreateExpectations>();
		directoryExpectations.Setups
			.EnumerateFiles("*.cs", SearchOption.AllDirectories)
			.ReturnValue([fileInfoExpectations.Instance(), fileInfo2Expectations.Instance()]);
		directoryExpectations.Setups
			.FileSystem
			.Gets()
			.ExpectedCallCount(2)
			.ReturnValue(fileSystemExpectations.Instance());

		var updates = new List<string>();
		var results = await UsingDirectiveAggregator.AggregateAsync(directoryExpectations.Instance(),
			update => updates.Add(update));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(results, Has.Count.EqualTo(2));
			Assert.That(results["System"], Is.EqualTo(1.0));
			Assert.That(results["System.Reflection"], Is.EqualTo(0.5));
			Assert.That(updates, Has.Count.EqualTo(2));
			Assert.That(updates, Does.Contain($"Analyzing {fileName}..."));
			Assert.That(updates, Does.Contain($"Analyzing {fileName2}..."));
		}
	}
}