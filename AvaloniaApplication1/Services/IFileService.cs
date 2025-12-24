using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvaloniaApplication1.Services;

public interface IFileService
{
    /// <summary>
    /// Picks a file from the file system
    /// </summary>
    /// <param name="allowedExtensions">Allowed file extensions (e.g., ".txt", ".pdf")</param>
    /// <returns>File path or null if cancelled</returns>
    Task<string?> PickFileAsync(params string[] allowedExtensions);

    /// <summary>
    /// Picks multiple files from the file system
    /// </summary>
    /// <param name="allowedExtensions">Allowed file extensions</param>
    /// <returns>List of file paths</returns>
    Task<IEnumerable<string>> PickMultipleFilesAsync(params string[] allowedExtensions);

    /// <summary>
    /// Picks a folder from the file system
    /// </summary>
    /// <returns>Folder path or null if cancelled</returns>
    Task<string?> PickFolderAsync();

    /// <summary>
    /// Saves a file to the file system
    /// </summary>
    /// <param name="fileName">Default file name</param>
    /// <returns>File path or null if cancelled</returns>
    Task<string?> SaveFileAsync(string fileName);

    /// <summary>
    /// Reads text content from a file
    /// </summary>
    /// <param name="filePath">File path</param>
    /// <returns>File content</returns>
    Task<string> ReadTextAsync(string filePath);

    /// <summary>
    /// Writes text content to a file
    /// </summary>
    /// <param name="filePath">File path</param>
    /// <param name="content">Content to write</param>
    Task WriteTextAsync(string filePath, string content);
}
