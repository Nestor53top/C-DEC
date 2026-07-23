using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IDnSpy.Services;

namespace IDnSpy.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly DecompilerService _decompilerService = new();
    private Window? _window;

    public void SetWindow(Window window) => _window = window;

    [ObservableProperty]
    private string _title = "IDnSpy - Combined C# Decompiler";

    [ObservableProperty]
    private string _currentFilePath = "";

    [ObservableProperty]
    private string _decompiledCode = "// Open a .dll or .exe file to decompile";

    [ObservableProperty]
    private string _assemblyInfoText = "";

    [ObservableProperty]
    private string _hexDump = "";

    [ObservableProperty]
    private string _ilCode = "";

    [ObservableProperty]
    private string _statusBarText = "Ready";

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private int _selectedTabIndex;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private AssemblyInfo? _currentAssemblyInfo;

    public ObservableCollection<TypeEntry> Types { get; } = new();

    [RelayCommand]
    public async Task OpenAssemblyAsync()
    {
        if (_window == null) return;

        var topLevel = TopLevel.GetTopLevel(_window);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open .NET Assembly",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("All .NET Assemblies") { Patterns = new[] { "*.dll", "*.exe", "*.winmd" } },
                new FilePickerFileType("All Files") { Patterns = new[] { "*.*" } }
            }
        });

        if (files.Count > 0)
        {
            var path = files[0].Path.LocalPath;
            await LoadAssemblyAsync(path);
        }
    }

    [RelayCommand]
    public async Task LoadAssemblyAsync(string filePath)
    {
        try
        {
            IsLoading = true;
            StatusBarText = $"Loading {Path.GetFileName(filePath)}...";
            CurrentFilePath = filePath;

            await Task.Run(() =>
            {
                CurrentAssemblyInfo = _decompilerService.GetAssemblyInfo(filePath);
                DecompiledCode = _decompilerService.DecompileAssembly(filePath);
                HexDump = _decompilerService.GenerateHexDump(filePath);
                IlCode = _decompilerService.DecompileIl(filePath);
                AssemblyInfoText = FormatAssemblyInfo(CurrentAssemblyInfo);
            });

            Types.Clear();
            if (CurrentAssemblyInfo != null)
            {
                foreach (var type in CurrentAssemblyInfo.Types)
                {
                    Types.Add(type);
                }
            }

            StatusBarText = $"Loaded: {Path.GetFileName(filePath)} | {CurrentAssemblyInfo?.Types.Count ?? 0} types";
        }
        catch (Exception ex)
        {
            StatusBarText = $"Error: {ex.Message}";
            DecompiledCode = $"// Error decompiling: {ex.Message}\n// {ex.StackTrace}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void SelectType(TypeEntry? type)
    {
        if (type == null || string.IsNullOrEmpty(CurrentFilePath)) return;

        try
        {
            StatusBarText = $"Decompiling {type.Name}...";
            DecompiledCode = _decompilerService.DecompileType(CurrentFilePath, type.Name);
            StatusBarText = $"Showing: {type.Name}";
        }
        catch (Exception ex)
        {
            StatusBarText = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    public void SearchInCode()
    {
        if (string.IsNullOrWhiteSpace(SearchText)) return;

        var lines = DecompiledCode.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            {
                StatusBarText = $"Found at line {i + 1}";
                return;
            }
        }

        StatusBarText = "Not found";
    }

    private static string FormatAssemblyInfo(AssemblyInfo info)
    {
        return $@"Assembly Information
====================
File:          {info.FileName}
Path:          {info.FullPath}
Assembly:      {info.AssemblyName}
Module Kind:   {info.ModuleKind}
Machine:       {info.ProcessorArchitecture}
PE Kind:       {info.PEKind}
Framework:     {info.TargetFramework}
Metadata:      {info.MetadataVersion}
Total Types:   {info.Types.Count}";
    }
}
