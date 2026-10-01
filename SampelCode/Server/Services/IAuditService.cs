using System.Threading.Tasks;

namespace NovinApp.Server.Services
{
    public interface IAuditService
    {
        Task LogAsync(string action, string entityName, string? entityId = null, string? details = null);
    }
}
