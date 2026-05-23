using Rediter.Api.Interfaces;

namespace Rediter.Api.Models;

public class Picture : Entity
{
    public virtual string FileName { get; set; } = null!;
    public virtual string StoragePath { get; set; } = null!;
    public virtual string MimeType { get; set; } = null!;
    public virtual int Size { get; set; }
    public virtual string FullPath => Path.Combine(StoragePath, FileName);

    private FileInfo? _file;
    public virtual FileInfo File
    {
        get
        {
            if (_file == null)
            {
                if (string.IsNullOrWhiteSpace(FullPath))
                    throw new Exception("File path is invalid or empty.");

                _file = new FileInfo(FullPath);
            }
            return _file;
        }
    }

    public Picture() { }
}