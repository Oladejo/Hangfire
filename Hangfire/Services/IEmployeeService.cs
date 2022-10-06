using System.Threading.Tasks;

namespace Hangfire.Services
{
    public interface IEmployeeService
    {
        Task<bool> InsertEmployeeAsync();
    }
}
