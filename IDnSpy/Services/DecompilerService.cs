using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;

namespace IDnSpy.Services;

public class DecompilerService
{
    public string DecompileAssembly(string filePath, DecompilerSettings? settings = null)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Assembly not found", filePath);

        var effectiveSettings = settings ?? CreateDefaultSettings();
        var decompiler = new CSharpDecompiler(filePath, effectiveSettings);
        return decompiler.DecompileWholeModuleAsString();
    }

    public string DecompileType(string filePath, string typeName)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Assembly not found", filePath);

        var settings = CreateDefaultSettings();
        var decompiler = new CSharpDecompiler(filePath, settings);

        foreach (var type in decompiler.TypeSystem.MainModule.TypeDefinitions)
        {
            if (type.FullName == typeName || type.Name == typeName)
            {
                return decompiler.DecompileTypeAsString(type.FullTypeName);
            }
        }

        throw new InvalidOperationException($"Type '{typeName}' not found in assembly.");
    }

    public AssemblyInfo GetAssemblyInfo(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Assembly not found", filePath);

        var settings = CreateDefaultSettings();
        var decompiler = new CSharpDecompiler(filePath, settings);
        var module = decompiler.TypeSystem.MainModule;

        var info = new AssemblyInfo
        {
            FileName = Path.GetFileName(filePath),
            FullPath = filePath,
            AssemblyName = module.AssemblyName ?? "Unknown",
            ModuleKind = module.Metadata.PEHeaders.PEHeader.Magic.ToString(),
            ProcessorArchitecture = module.Metadata.PEHeaders.CoffHeader.Machine.ToString(),
            PEKind = module.Metadata.PEHeaders.PEHeader.Magic.ToString(),
            TargetFramework = module.Metadata.DetectTargetFrameworkId() ?? "Unknown",
            MetadataVersion = module.Metadata.PEHeaders.MetadataVersion ?? "Unknown",
            Types = new List<TypeEntry>()
        };

        foreach (var type in module.TypeDefinitions)
        {
            string baseTypeName = "";
            if (type.BaseType != null)
                baseTypeName = type.BaseType.FullName;

            var entry = new TypeEntry
            {
                Name = type.FullName,
                Kind = type.Kind.ToString(),
                BaseType = baseTypeName,
                Namespace = type.Namespace ?? "",
                IsPublic = type.Accessibility == Accessibility.Public || type.Accessibility == Accessibility.Internal,
                MethodCount = type.Methods.Count,
                PropertyCount = type.Properties.Count,
                FieldCount = type.Fields.Count,
                EventCount = type.Events.Count
            };
            info.Types.Add(entry);
        }

        return info;
    }

    public string DecompileIl(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Assembly not found", filePath);

        var settings = CreateDefaultSettings();
        var decompiler = new CSharpDecompiler(filePath, settings);
        return decompiler.DecompileWholeModuleAsString();
    }

    public string GetMethodBody(string filePath, string typeName, string methodName)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Assembly not found", filePath);

        var settings = CreateDefaultSettings();
        var decompiler = new CSharpDecompiler(filePath, settings);

        foreach (var type in decompiler.TypeSystem.MainModule.TypeDefinitions)
        {
            if (type.FullName == typeName || type.Name == typeName)
            {
                foreach (var method in type.Methods)
                {
                    if (method.Name == methodName)
                    {
                        var handle = method.MetadataToken;
                        return decompiler.DecompileAsString(handle);
                    }
                }
            }
        }

        throw new InvalidOperationException($"Method '{methodName}' not found in type '{typeName}'.");
    }

    public string GenerateHexDump(string filePath, int bytesPerLine = 16)
    {
        var data = File.ReadAllBytes(filePath);
        var sb = new StringBuilder();
        var offset = 0;

        while (offset < data.Length)
        {
            sb.Append($"{offset:X8}  ");

            int i;
            for (i = 0; i < bytesPerLine && offset + i < data.Length; i++)
            {
                sb.Append($"{data[offset + i]:X2} ");
                if (i == bytesPerLine / 2 - 1) sb.Append(' ');
            }

            for (; i < bytesPerLine; i++)
            {
                sb.Append("   ");
                if (i == bytesPerLine / 2 - 1) sb.Append(' ');
            }

            sb.Append(' ');
            for (i = 0; i < bytesPerLine && offset + i < data.Length; i++)
            {
                byte b = data[offset + i];
                sb.Append(b >= 0x20 && b < 0x7F ? (char)b : '.');
            }

            offset += bytesPerLine;
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static DecompilerSettings CreateDefaultSettings()
    {
        return new DecompilerSettings(LanguageVersion.CSharp11_0)
        {
            ThrowOnAssemblyResolveErrors = false
        };
    }
}

public class AssemblyInfo
{
    public string FileName { get; set; } = "";
    public string FullPath { get; set; } = "";
    public string AssemblyName { get; set; } = "";
    public string ModuleKind { get; set; } = "";
    public string ProcessorArchitecture { get; set; } = "";
    public string PEKind { get; set; } = "";
    public string TargetFramework { get; set; } = "";
    public string MetadataVersion { get; set; } = "";
    public List<TypeEntry> Types { get; set; } = new();
}

public class TypeEntry
{
    public string Name { get; set; } = "";
    public string Kind { get; set; } = "";
    public string BaseType { get; set; } = "";
    public string Namespace { get; set; } = "";
    public bool IsPublic { get; set; }
    public int MethodCount { get; set; }
    public int PropertyCount { get; set; }
    public int FieldCount { get; set; }
    public int EventCount { get; set; }
}
