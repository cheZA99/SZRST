using Microsoft.EntityFrameworkCore.Migrations;

namespace SZRST.Infrastructure.Migrations
{
	/// <inheritdoc />
	public partial class addedTenantSeed :Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			// Provera da li Core Tenant već postoji
			migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM [Tenant] WHERE [Name] = 'Core Tenant')
                BEGIN
                    SET IDENTITY_INSERT [Tenant] ON;

                    INSERT INTO [Tenant] ([Id], [Name], [DateCreated], [DateModified], [IsDeleted])
                    VALUES (1, 'Core Tenant', GETUTCDATE(), GETUTCDATE(), 0);

                    SET IDENTITY_INSERT [Tenant] OFF;
                END
            ");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@"
                DELETE FROM [Tenant] WHERE [Id] = 1 AND [Name] = 'Core Tenant';
            ");
		}
	}
}