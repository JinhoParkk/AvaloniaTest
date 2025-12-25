using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using JinoOrder.Application.Common;

namespace JinoOrder.Desktop.Services;

public class DesktopFileService : IFileService
{
    private readonly Window? _mainWindow;

    public DesktopFileService(Window? mainWindow = null)
    {
        _mainWindow = mainWindow;
    }

    private IStorageProvider? GetStorageProvider()
    {
        return _mainWindow?.StorageProvider;
    }

    public async Task<string?> PickFileAsync(params string[] allowedExtensions)
    {
        var storageProvider = GetStorageProvider();
        if (storageProvider == null) return null;

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = CreateFileTypeFilter(allowedExtensions)
        });

        return files.Count > 0 ? files[0].Path.LocalPath : null;
    }

    public async Task<IEnumerable<string>> PickMultipleFilesAsync(params string[] allowedExtensions)
    {
        var storageProvider = GetStorageProvider();
        if (storageProvider == null) return Array.Empty<string>();

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = true,
            FileTypeFilter = CreateFileTypeFilter(allowedExtensions)
        });

        return files.Select(f => f.Path.LocalPath);
    }

    public async Task<string?> PickFolderAsync()
    {
        var storageProvider = GetStorageProvider();
        if (storageProvider == null) return null;

        var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false
        });

        return folders.Count > 0 ? folders[0].Path.LocalPath : null;
    }

    public async Task<string?> SaveFileAsync(string fileName)
    {
        var storageProvider = GetStorageProvider();
        if (storageProvider == null) return null;

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            SuggestedFileName = fileName
        });

        return file?.Path.LocalPath;
    }

    public async Task<string> ReadTextAsync(string filePath)
    {
        return await File.ReadAllTextAsync(filePath);
    }

    public async Task WriteTextAsync(string filePath, string content)
    {
        await File.WriteAllTextAsync(filePath, content);
    }

    private List<FilePickerFileType>? CreateFileTypeFilter(string[] extensions)
    {
        if (extensions.Length == 0) return null;

        return new List<FilePickerFileType>
        {
            new FilePickerFileType("Files")
            {
                Patterns = extensions.Select(ext => ext.StartsWith(".") ? $"*{ext}" : $"*.{ext}").ToList()
            }
        };
    }
}
