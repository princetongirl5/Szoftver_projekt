using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using EdzestervGenerator.Api.Data;

#nullable disable

namespace EdzestervGenerator.Api.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261114170000_AddIdentityPreferencesAndWorkoutLogging")]
public partial class AddIdentityPreferencesAndWorkoutLogging : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "UserName", table: "Users", type: "TEXT", maxLength: 256, nullable: true);
        migrationBuilder.AddColumn<string>(name: "NormalizedUserName", table: "Users", type: "TEXT", maxLength: 256, nullable: true);
        migrationBuilder.AddColumn<string>(name: "NormalizedEmail", table: "Users", type: "TEXT", maxLength: 256, nullable: true);
        migrationBuilder.AddColumn<bool>(name: "EmailConfirmed", table: "Users", type: "INTEGER", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<string>(name: "PasswordHash", table: "Users", type: "TEXT", nullable: true);
        migrationBuilder.AddColumn<string>(name: "SecurityStamp", table: "Users", type: "TEXT", nullable: true);
        migrationBuilder.AddColumn<string>(name: "ConcurrencyStamp", table: "Users", type: "TEXT", nullable: true);
        migrationBuilder.AddColumn<string>(name: "PhoneNumber", table: "Users", type: "TEXT", nullable: true);
        migrationBuilder.AddColumn<bool>(name: "PhoneNumberConfirmed", table: "Users", type: "INTEGER", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<bool>(name: "TwoFactorEnabled", table: "Users", type: "INTEGER", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<DateTimeOffset>(name: "LockoutEnd", table: "Users", type: "TEXT", nullable: true);
        migrationBuilder.AddColumn<bool>(name: "LockoutEnabled", table: "Users", type: "INTEGER", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<int>(name: "AccessFailedCount", table: "Users", type: "INTEGER", nullable: false, defaultValue: 0);

        migrationBuilder.CreateIndex(name: "EmailIndex", table: "Users", column: "NormalizedEmail", unique: true);
        migrationBuilder.CreateIndex(name: "UserNameIndex", table: "Users", column: "NormalizedUserName", unique: true);

        migrationBuilder.CreateTable(name: "AspNetRoles", columns: table => new
        {
            Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
            NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
            ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
        }, constraints: table => table.PrimaryKey("PK_AspNetRoles", x => x.Id));

        migrationBuilder.CreateTable(name: "AspNetRoleClaims", columns: table => new
        {
            Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            RoleId = table.Column<int>(type: "INTEGER", nullable: false),
            ClaimType = table.Column<string>(type: "TEXT", nullable: true),
            ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
        }, constraints: table => { table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id); table.ForeignKey("FK_AspNetRoleClaims_AspNetRoles_RoleId", x => x.RoleId, "AspNetRoles", "Id", onDelete: ReferentialAction.Cascade); });

        migrationBuilder.CreateTable(name: "AspNetUserClaims", columns: table => new
        {
            Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            UserId = table.Column<int>(type: "INTEGER", nullable: false),
            ClaimType = table.Column<string>(type: "TEXT", nullable: true),
            ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
        }, constraints: table => { table.PrimaryKey("PK_AspNetUserClaims", x => x.Id); table.ForeignKey("FK_AspNetUserClaims_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade); });

        migrationBuilder.CreateTable(name: "AspNetUserLogins", columns: table => new
        {
            LoginProvider = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
            ProviderKey = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
            ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
            UserId = table.Column<int>(type: "INTEGER", nullable: false)
        }, constraints: table => { table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey }); table.ForeignKey("FK_AspNetUserLogins_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade); });

        migrationBuilder.CreateTable(name: "AspNetUserRoles", columns: table => new
        {
            UserId = table.Column<int>(type: "INTEGER", nullable: false),
            RoleId = table.Column<int>(type: "INTEGER", nullable: false)
        }, constraints: table => { table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId }); table.ForeignKey("FK_AspNetUserRoles_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade); table.ForeignKey("FK_AspNetUserRoles_AspNetRoles_RoleId", x => x.RoleId, "AspNetRoles", "Id", onDelete: ReferentialAction.Cascade); });

        migrationBuilder.CreateTable(name: "AspNetUserTokens", columns: table => new
        {
            UserId = table.Column<int>(type: "INTEGER", nullable: false),
            LoginProvider = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
            Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
            Value = table.Column<string>(type: "TEXT", nullable: true)
        }, constraints: table => { table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name }); table.ForeignKey("FK_AspNetUserTokens_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade); });

        migrationBuilder.CreateTable(name: "PlanPreferences", columns: table => new
        {
            UserId = table.Column<int>(type: "INTEGER", nullable: false),
            Goal = table.Column<int>(type: "INTEGER", nullable: false),
            Location = table.Column<int>(type: "INTEGER", nullable: false),
            EquipmentIds = table.Column<string>(type: "TEXT", nullable: false),
            Days = table.Column<string>(type: "TEXT", nullable: false),
            SessionMinutes = table.Column<int>(type: "INTEGER", nullable: false),
            Difficulty = table.Column<int>(type: "INTEGER", nullable: false),
            UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
        }, constraints: table => { table.PrimaryKey("PK_PlanPreferences", x => x.UserId); table.ForeignKey("FK_PlanPreferences_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade); });

        migrationBuilder.CreateIndex(name: "IX_AspNetRoleClaims_RoleId", table: "AspNetRoleClaims", column: "RoleId");
        migrationBuilder.CreateIndex(name: "IX_AspNetUserClaims_UserId", table: "AspNetUserClaims", column: "UserId");
        migrationBuilder.CreateIndex(name: "IX_AspNetUserLogins_UserId", table: "AspNetUserLogins", column: "UserId");
        migrationBuilder.CreateIndex(name: "IX_AspNetUserRoles_RoleId", table: "AspNetUserRoles", column: "RoleId");
        migrationBuilder.CreateIndex(name: "RoleNameIndex", table: "AspNetRoles", column: "NormalizedName", unique: true);

        migrationBuilder.AddColumn<int>(name: "ActualReps", table: "WorkoutExercises", type: "INTEGER", nullable: true);
        migrationBuilder.AddColumn<double>(name: "ActualWeightKg", table: "WorkoutExercises", type: "REAL", nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("ActualReps", "WorkoutExercises");
        migrationBuilder.DropColumn("ActualWeightKg", "WorkoutExercises");
        migrationBuilder.DropTable("PlanPreferences");
        migrationBuilder.DropTable("AspNetUserTokens");
        migrationBuilder.DropTable("AspNetUserRoles");
        migrationBuilder.DropTable("AspNetUserLogins");
        migrationBuilder.DropTable("AspNetUserClaims");
        migrationBuilder.DropTable("AspNetRoleClaims");
        migrationBuilder.DropTable("AspNetRoles");
        migrationBuilder.DropIndex("EmailIndex", "Users");
        migrationBuilder.DropIndex("UserNameIndex", "Users");
        foreach (var column in new[] { "UserName", "NormalizedUserName", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount" })
            migrationBuilder.DropColumn(column, "Users");
    }
}
