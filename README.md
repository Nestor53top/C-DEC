# IDnSpy - Combined C# Decompiler

Cross-platform .NET decompiler combining **ILSpy's** decompiler engine with **dnSpy's** feature concepts.

## Features

- **Decompiled C# View** - Full decompilation of .NET assemblies using ICSharpCode.Decompiler engine
- **Assembly Info** - Detailed metadata inspection (assembly name, framework, machine type, etc.)
- **Hex Viewer** - Raw binary hex dump with offset and ASCII representation
- **IL Code View** - Decompiled IL code output
- **Type Browser** - Assembly explorer with type listing (methods, properties, fields, events counts)
- **Search** - Search through decompiled code
- **Cross-platform** - Windows, Linux, macOS via Avalonia UI

## Built With

- [Avalonia UI](https://avaloniaui.net/) - Cross-platform UI framework
- [ICSharpCode.Decompiler](https://github.com/icsharpcode/ILSpy) - Decompiler engine (ILSpy)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) - MVVM toolkit
- [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) - Runtime

## Build

```bash
dotnet build IDnSpy/IDnSpy.csproj -c Release
```

## Run

```bash
dotnet run --project IDnSpy/IDnSpy.csproj
```

## CI/CD

GitHub Actions builds for:
- Windows (win-x64)
- Linux (linux-x64)  
- macOS (osx-arm64)

Artifacts are available in the Actions tab after each successful build.

## Credits

- **ILSpy** (MIT) - Decompile engine and architecture inspiration
- **dnSpy/dnSpyEx** (GPLv3) - Feature concepts and design inspiration
