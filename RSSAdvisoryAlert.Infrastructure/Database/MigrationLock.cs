using Microsoft.EntityFrameworkCore;
using RSSAdvisoryAlert.Infrastructure.Data;

namespace RSSAdvisoryAlert.Infrastructure.Database
{
    public static class MigrationLock
    {
        public static async Task AcquireAsync(AppDbContext db)
        {
            await db.Database.ExecuteSqlRawAsync(@"
                DECLARE @result int;
                EXEC @result = sp_getapplock 
                    @Resource = 'ef-core-migrations',
                    @LockMode = 'Exclusive',
                    @LockOwner = 'Session',
                    @LockTimeout = 60000;

                IF (@result < 0)
                    THROW 50000, 'Failed to acquire migration lock', 1;
                ");
        }
    }
}
