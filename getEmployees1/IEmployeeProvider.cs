using System.Threading.Tasks;

namespace getEmployees
{
    public interface IEmployeeProvider
    {
        Task<Employee[]> GetEmployeesAsync();
        Task<Employee> GetEmployeeAsync(string id);

        Task<Employee[]> GetEmployeeByLocationAsync(string location);
    }
}