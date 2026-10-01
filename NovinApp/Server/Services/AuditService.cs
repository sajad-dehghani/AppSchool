using NovinApp.Server.MyContext;

namespace NovinApp.Server.Services
{
    public interface IAuditService
    {
        Task LogAsync(string action, string entityName, string entityId, string details);
    }

    public class AuditService : IAuditService
    {
        private readonly MyAppContext _context;
        private readonly ICurrentUserService _currentUserService;

        public AuditService(MyAppContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task LogAsync(string action, string entityName, string entityId, string details)
        {
            try
            {
                // Simple logging - can be extended
                Console.WriteLine($"[AUDIT] {DateTime.Now:yyyy-MM-dd HH:mm:ss} | User:{_currentUserService.UserId} | {action} | {entityName}:{entityId} | {details}");
                await Task.CompletedTask;
            }
            catch { }
        }
    }
}
