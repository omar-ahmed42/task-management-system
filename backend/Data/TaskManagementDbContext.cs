using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class TaskManagementDbContext(DbContextOptions<TaskManagementDbContext> options): DbContext(options)
{

}
