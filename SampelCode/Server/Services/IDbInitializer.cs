using System.Threading.Tasks;

namespace NovinApp.Server.Services
{
    public interface IDbInitializer
    {
        Task InitializeAsync();
    }
}
