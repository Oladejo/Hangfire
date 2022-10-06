using Hangfire.AppDbContext;
using Hangfire.Model;
using System;
using System.Threading.Tasks;

namespace Hangfire.Services
{
    public class EmployeeService : IEmployeeService
    {
        #region Property  
        private readonly EmployeeDbContext _employeeDbContext;
        #endregion

        #region Constructor  
        public EmployeeService(EmployeeDbContext employeeDbContext)
        {
            _employeeDbContext = employeeDbContext;
        }
        #endregion

        #region Insert Employee  
        public async Task<bool> InsertEmployeeAsync()
        {
            try
            {
                Employee employee = new Employee()
                {
                    EmployeeName = "Jk",
                    Designation = "Full Stack Developer"
                };
                await _employeeDbContext.AddAsync(employee);
                await _employeeDbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
