using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rediter.Api.Models;

namespace Rediter.Api.Data.Mappings
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            // Tabela
            builder.ToTable("Posts");

            // Chave Primária
            builder.HasKey(p => p.Id);

            // Propriedades
            builder.Property(p => p.Content)
                .HasMaxLength(2000); // Exemplo de limite de caracteres

            builder.Property(p => p.LocationName)
                .HasMaxLength(255);

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); 
            builder.Property(p => p.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // --- Relacionamentos ---

            // 1. Relacionamento com Usuário (Autor)
            builder.HasOne(p => p.User)
                .WithMany() // Se o User não tiver ICollection<Post>, deixe vazio
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. Auto-relacionamento (Post pai e Respostas/Comentários)
            builder.HasOne(p => p.ParentPost)
                .WithMany(p => p.Replies)
                .HasForeignKey(p => p.ParentPostId)
                .OnDelete(DeleteBehavior.Restrict); // Restrict evita ciclos de cascade

            // 3. Relacionamento com Imagens (Um para Muitos)
            builder.HasMany(p => p.PostImages)
                .WithOne() // Se PostImage não tiver a propriedade 'Post', deixe vazio
                .HasForeignKey("PostId") // Nome da FK no banco de dados
                .OnDelete(DeleteBehavior.Cascade);

            // 4. Relacionamento com Likes (Muitos para Muitos ou 1:N com a tabela associativa)
            builder.HasMany(p => p.Likes)
                .WithOne() // Ajuste se UserPostLike tiver a propriedade 'Post'
                .HasForeignKey("PostId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}