using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SZRST.Infrastructure.Configurations;

public class UserLoginConfiguration : IEntityTypeConfiguration<UserLogin>
{
    public void Configure(EntityTypeBuilder<UserLogin> builder)
    {
        builder.ToTable(nameof(UserLogin));
        builder.HasOne(x => x.User).WithMany(x => x.Logins).HasForeignKey(x => x.UserId).IsRequired();

    }
}
