using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HackerFlow.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCompaniesAndConnections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Add the new columns first; the old string columns stay until the data is copied
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Applications",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContactId",
                table: "Applications",
                type: "INTEGER",
                nullable: true);

            // 2. Create a Company for every distinct (case-insensitive) company string that doesn't exist yet.
            //    Blank company strings become "Unknown" because CompanyId is required.
            migrationBuilder.Sql(@"
                INSERT INTO ""Companies"" (""Name"", ""Description"", ""Website"", ""ReferralChance"")
                SELECT MIN(name), '', '', 1
                FROM (
                    SELECT CASE WHEN TRIM(""Company"") = '' THEN 'Unknown' ELSE TRIM(""Company"") END AS name
                    FROM ""Applications""
                )
                WHERE LOWER(name) NOT IN (SELECT LOWER(""Name"") FROM ""Companies"")
                GROUP BY LOWER(name);
            ");

            // 3. Create a Connection for every distinct contact string that doesn't exist yet
            //    (Closeness 1 = Weak, Level 0 = Direct, CanIntroduceMe = false)
            migrationBuilder.Sql(@"
                INSERT INTO ""Connections"" (""Name"", ""Closeness"", ""Level"", ""CanIntroduceMe"", ""IntroducedById"", ""ConnectedAt"", ""LastContactAt"")
                SELECT MIN(TRIM(""Contact"")), 1, 0, 0, NULL, NULL, NULL
                FROM ""Applications""
                WHERE TRIM(""Contact"") <> ''
                  AND LOWER(TRIM(""Contact"")) NOT IN (SELECT LOWER(""Name"") FROM ""Connections"")
                GROUP BY LOWER(TRIM(""Contact""));
            ");

            // 4. Point each application at its company / contact
            migrationBuilder.Sql(@"
                UPDATE ""Applications""
                SET ""CompanyId"" = (
                    SELECT c.""Id"" FROM ""Companies"" c
                    WHERE LOWER(c.""Name"") = LOWER(
                        CASE WHEN TRIM(""Applications"".""Company"") = '' THEN 'Unknown'
                             ELSE TRIM(""Applications"".""Company"") END)
                    LIMIT 1
                );
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Applications""
                SET ""ContactId"" = (
                    SELECT c.""Id"" FROM ""Connections"" c
                    WHERE LOWER(c.""Name"") = LOWER(TRIM(""Applications"".""Contact""))
                    LIMIT 1
                )
                WHERE TRIM(""Contact"") <> '';
            ");

            // 5. Now the old columns can go
            migrationBuilder.DropColumn(
                name: "Company",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Contact",
                table: "Applications");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_CompanyId",
                table: "Applications",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ContactId",
                table: "Applications",
                column: "ContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Companies_CompanyId",
                table: "Applications",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Connections_ContactId",
                table: "Applications",
                column: "ContactId",
                principalTable: "Connections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Bring the string columns back and refill them from the related rows.
            // The Companies / Connections rows created by Up are left in place.
            migrationBuilder.AddColumn<string>(
                name: "Company",
                table: "Applications",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Contact",
                table: "Applications",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
                UPDATE ""Applications""
                SET ""Company"" = COALESCE((SELECT c.""Name"" FROM ""Companies"" c WHERE c.""Id"" = ""Applications"".""CompanyId""), ''),
                    ""Contact"" = COALESCE((SELECT c.""Name"" FROM ""Connections"" c WHERE c.""Id"" = ""Applications"".""ContactId""), '');
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Companies_CompanyId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Connections_ContactId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_CompanyId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_ContactId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "Applications");
        }
    }
}