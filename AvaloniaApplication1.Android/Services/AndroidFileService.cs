using AvaloniaApplication1.Services;

namespace AvaloniaApplication1.Android.Services;

public class AndroidFileService : IFileService
{
    public Task<string?> PickFileAsync(params string[] allowedExtensions)
    {
        // TODO: Implement using Android file picker
        throw new NotImplementedException("Android file picker not yet implemented. Use platform-specific Android APIs.");
    }

    public Task<IEnumerable<string>> PickMultipleFilesAsync(params string[] allowedExtensions)
    {
        throw new NotImplementedException("Android file picker not yet implemented.");
    }

    public Task<string?> PickFolderAsync()
    {
        throw new NotImplementedException("Android folder picker not yet implemented.");
    }

    public Task<string?> SaveFileAsync(string fileName)
    {
        throw new NotImplementedException("Android file save not yet implemented.");
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
