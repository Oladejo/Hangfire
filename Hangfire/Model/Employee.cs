using System.ComponentModel.DataAnnotations;

namespace Hangfire.Model
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public string Designation { get; set; }
    }
}
