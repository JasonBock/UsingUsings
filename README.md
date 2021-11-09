# UsingUsings

A .NET tool that provides statistics on how much you use a using directive

## Overview

In C# 10, a new feature was added: [global using directives](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10#global-using-directives). Essentially, this lets you declare a `using` directive that's available for all files within a solution:
```
global using System;
```
But which `using` directives in your code base should you make global? That's what this tool does for you. You point it at a directory, and it'll parse all the .cs files within that directory (including all subdirectories), looking for `using` directives. It'll calculate how often you use a `using`, and provide a report when it's done so you can determine which ones you use a lot. Those are probably good targets to turn into global usings.