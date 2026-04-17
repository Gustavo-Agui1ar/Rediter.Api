using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Rediter.Api.Models
{

    [Table("pictures")]
    public class Picture
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("file_name")]
        public string FileName { get; set; } = null!; 

        [Column("storage_path")]
        public string StoragePath { get; set; } = null!;

        [Column("mime_type")]
        public string MimeType { get; set; } = null!;

        [Column("file_size_bytes")]
        public int Size { get; set; }

        [NotMapped]
        public string FullPath => Path.Combine(StoragePath, FileName);
    
        [NotMapped]
        private FileInfo? file { get; set; }

        [NotMapped]
        public FileInfo File
        { 
            get
            {
                if(file == null)
                {
                    if(string.IsNullOrWhiteSpace(FullPath))
                         throw new Exception("File path is invalid or empty.");

                    file = new FileInfo(FullPath);
                }

                return file;
            }
        }

    }
}