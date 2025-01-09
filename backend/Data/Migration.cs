using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public static class Migration
{
    public static void ApplyMigrations(this IApplicationBuilder app) {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        using TaskManagementDbContext dbContext = scope.ServiceProvider.GetRequiredService<TaskManagementDbContext>();

        dbContext.Database.Migrate();
    }
}
