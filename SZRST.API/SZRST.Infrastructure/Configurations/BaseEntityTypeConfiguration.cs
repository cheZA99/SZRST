
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SZRST.Infrastructure.Configurations;

public class BaseEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class, IBaseEntity<int>
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder
            .HasKey(entity => entity.Id);

        builder
            .Property(entity => entity.DateCreated)
            .IsRequired();

        builder
            .Property(entity => entity.DateModified)
            .IsRequired(false);

        builder
            .Property(entity => entity.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
