using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("pictures")]
public class Picture : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }
    [Column("storage_path")]
    public string StoragePath { get; set; } = null!;
    [Column("public_url")]
    public string PublicUrl { get; set; } = null!;
    [Column("mime_type")]
    public string MimeType { get; set; } = null!;
    [Column("file_size_bytes")]
    public long FileSizeBytes { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}