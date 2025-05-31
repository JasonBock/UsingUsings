using NUnit.Framework;
using Rocks;
using System.IO.Abstractions;

namespace UsingUsings.Core.Tests;

internal static class UsingDirectiveAggregatorTests
{
	[Test]
	public static async Task AggregateWhenDirectoryHasNoFilesAsync()
	{
		using var context = new RockContext();
		var directoryExpectations = context.Create<IDirectoryInfoCreateExpectations>();
		directoryExpectations.Methods
			.EnumerateFiles("*.cs", SearchOption.AllDirectories)
			.ReturnValue([]);

		using var writer = new StringWriter();
		var results = await UsingDirectiveAggregator.AggregateAsync(directoryExpectations.Instance(), writer);

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

		using var writer = new StringWriter();
		var results = await UsingDirectiveAggregator.AggregateAsync(directoryExpectations.Instance(), writer);

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

		using var writer = new StringWriter();
		var results = await UsingDirectiveAggregator.AggregateAsync(directoryExpectations.Instance(), writer);

		Assert.That(results, Has.Count.EqualTo(2));
	}
}