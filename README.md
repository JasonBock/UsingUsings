# UsingUsings

A .NET tool that provides statistics on how much you use a using directive

## Overview

In C# 10, a new feature was added: [global using directives](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-10#global-using-directives). Essentially, this lets you declare a `using` directive that's available for all files within a solution:
```
global using System.
```