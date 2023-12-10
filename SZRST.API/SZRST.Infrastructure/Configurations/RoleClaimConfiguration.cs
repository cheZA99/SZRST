using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SZRST.Infrastructure.Configurations;

public class RoleClaimConfiguration : IEntityTypeConfiguration<RoleClaim>
{
    public void Configure(EntityTypeBuilder<RoleClaim> builder)
    {
        builder.ToTable(nameof(RoleClaim));
        builder.HasOne(x => x.Role).WithMany(x => x.RoleClaims).HasForeignKey(x => x.RoleId).IsRequired();
    }
}
