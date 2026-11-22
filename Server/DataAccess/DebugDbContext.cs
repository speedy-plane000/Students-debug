using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class DebugDbContext(DbContextOptions<DebugDbContext> options) : DbContext(options)
{
    //das
}
