using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SZRST.Infrastructure.Configurations;

public class UserClaimConfiguration : IEntityTypeConfiguration<UserClaim>
{
    public void Configure(EntityTypeBuilder<UserClaim> builder)
    {
        builder.ToTable(nameof(UserClaim));
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.User).WithMany(x => x.Claims).HasForeignKey(x => x.UserId).IsRequired();
    }
}
