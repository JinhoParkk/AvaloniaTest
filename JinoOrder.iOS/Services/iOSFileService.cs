using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JinoOrder.Application.Common;

namespace JinoOrder.iOS.Services;

public class iOSFileService : IFileService
{
    public Task<string?> PickFileAsync(params string[] allowedExtensions)
    {
        // TODO: Implement using iOS UIDocumentPickerViewController
        throw new NotImplementedException("iOS file picker not yet implemented. Use UIDocumentPickerViewController.");
    }

    public Task<IEnumerable<string>> PickMultipleFilesAsync(params string[] allowedExtensions)
    {
        throw new NotImplementedException("iOS file picker not yet implemented.");
    }

    public Task<string?> PickFolderAsync()
    {
        throw new NotImplementedException("iOS folder picker not yet implemented.");
    }

    public Task<string?> SaveFileAsync(string fileName)
    {
        throw new NotImplementedException("iOS file save not yet implemented.");
    }

    public async Task<string> ReadTextAsync(string filePath)
    {
        return await File.ReadAllTextAsync(filePath);
    }

    public async Task WriteTextAsync(string filePath, string content)
    {
        await File.WriteAllTextAsync(filePath, content);
    }
}
