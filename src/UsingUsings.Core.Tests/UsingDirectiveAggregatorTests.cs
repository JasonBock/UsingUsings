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
		directoryExpectations.Methods
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
		fileInfoExpectations.Properties
			 .Getters
			 .FullName()
			 .ExpectedCallCount(2)
			 .ReturnValue(fileName);

		var fileExpectations = context.Create<IFileCreateExpectations>();
		fileExpectations.Methods
			.ReadAllTextAsync(fileName)
			.ReturnValue(Task.FromResult(
				"""
				public static class Stuff { }
				"""));

		var fileSystemExpectations = context.Create<IFileSystemCreateExpectations>();
		fileSystemExpectations.Properties
			.Getters
			.File()
			.ReturnValue(fileExpectations.Instance());

		var directoryExpectations = context.Create<IDirectoryInfoCreateExpectations>();
		directoryExpectations.Methods
			.EnumerateFiles("*.cs", SearchOption.AllDirectories)
			.ReturnValue([fileInfoExpectations.Instance()]);
		directoryExpectations.Properties
			.Getters
			.FileSystem()
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
		fileInfoExpectations.Properties
			.Getters
			.FullName()
			.ExpectedCallCount(2)
			.ReturnValue(fileName);

		var fileExpectations = context.Create<IFileCreateExpectations>();
		fileExpectations.Methods
			.ReadAllTextAsync(fileName)
			.ReturnValue(Task.FromResult(
				"""
				using System;
				using System.Reflection;
				"""));

		var fileSystemExpectations = context.Create<IFileSystemCreateExpectations>();
		fileSystemExpectations.Properties
			.Getters
			.File()
			.ReturnValue(fileExpectations.Instance());

		var directoryExpectations = context.Create<IDirectoryInfoCreateExpectations>();
		directoryExpectations.Methods
			.EnumerateFiles("*.cs", SearchOption.AllDirectories)
			.ReturnValue([fileInfoExpectations.Instance()]);
		directoryExpectations.Properties
			.Getters
			.FileSystem()
			.ReturnValue(fileSystemExpectations.Instance());

		var updates = new List<string>();
		var results = await UsingDirectiveAggregator.AggregateAsync(directoryExpectations.Instance(), 
			update => updates.Add(update));

		Assert.Multiple(() =>
		{
			Assert.That(results, Has.Count.EqualTo(2));
			Assert.That(results["System"], Is.EqualTo(1.0));
			Assert.That(results["System.Reflection"], Is.EqualTo(1.0));
			Assert.That(updates, Has.Count.EqualTo(1));
			Assert.That(updates, Does.Contain($"Analyzing {fileName}..."));
		});
	}

	[Test]
	public static async Task AggregateWhenDirectoryHasMultipleFileWithUsingsAsync()
	{
		using var context = new RockContext();

		const string fileName = "code.cs";
		const string fileName2 = "code2.cs";

		var fileInfoExpectations = context.Create<IFileInfoCreateExpectations>();
		fileInfoExpectations.Properties
			.Getters
			.FullName()
			.ExpectedCallCount(2)
			.ReturnValue(fileName);

		var fileInfo2Expectations = context.Create<IFileInfoCreateExpectations>();
		fileInfo2Expectations.Properties
			.Getters
			.FullName()
			.ExpectedCallCount(2)
			.ReturnValue(fileName2);

		var fileExpectations = context.Create<IFileCreateExpectations>();
		fileExpectations.Methods
			.ReadAllTextAsync(fileName)
			.ReturnValue(Task.FromResult(
				"""
				using System;
				using System.Reflection;
				"""));
		fileExpectations.Methods
			.ReadAllTextAsync(fileName2)
			.ReturnValue(Task.FromResult(
				"""
				using System;
				"""));

		var fileSystemExpectations = context.Create<IFileSystemCreateExpectations>();
		fileSystemExpectations.Properties
			.Getters
			.File()
			.ExpectedCallCount(2)
			.ReturnValue(fileExpectations.Instance());

		var directoryExpectations = context.Create<IDirectoryInfoCreateExpectations>();
		directoryExpectations.Methods
			.EnumerateFiles("*.cs", SearchOption.AllDirectories)
			.ReturnValue([fileInfoExpectations.Instance(), fileInfo2Expectations.Instance()]);
		directoryExpectations.Properties
			.Getters
			.FileSystem()
			.ExpectedCallCount(2)
			.ReturnValue(fileSystemExpectations.Instance());

		var updates = new List<string>();
		var results = await UsingDirectiveAggregator.AggregateAsync(directoryExpectations.Instance(),
			update => updates.Add(update));

		Assert.Multiple(() =>
		{
			Assert.That(results, Has.Count.EqualTo(2));
			Assert.That(results["System"], Is.EqualTo(1.0));
			Assert.That(results["System.Reflection"], Is.EqualTo(0.5));
			Assert.That(updates, Has.Count.EqualTo(2));
			Assert.That(updates, Does.Contain($"Analyzing {fileName}..."));
			Assert.That(updates, Does.Contain($"Analyzing {fileName2}..."));
		});
	}
}