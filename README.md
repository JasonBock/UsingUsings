# UsingUsings

A .NET tool that provides statistics on how much you use a using directive. In C# 10, the global `using` directives feature was added. Essentially, this lets you declare a `using` directive that's available for all files within a solution:

```csharp
global using System;
```

But which `using` directives in your code base should you make global? That's what this tool does for you. You point it at a directory, and it'll parse all the .cs files within that directory (including all subdirectories), looking for `using` directives. It'll calculate how often you use a `using`, and provide a report when it's done so you can determine which ones you use a lot. Those are probably good targets to turn into global usings.

## Getting Started

Install the tool (the `-g` switch means it will be installed as a global tool):

```
dotnet tool install -g UsingUsings
```

### Prerequisites

This tool targets .NET 7.

## Usage

Once the tool is installed, you can run `using` analysis on code within a directory:

```
usingusings "My-Target-Directory-Goes-Here"
```

## Additional documentation

* [Global using directives](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10#global-using-directives)