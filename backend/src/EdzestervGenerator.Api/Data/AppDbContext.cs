using Microsoft.EntityFrameworkCore;

namespace EdzestervGenerator.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
}
