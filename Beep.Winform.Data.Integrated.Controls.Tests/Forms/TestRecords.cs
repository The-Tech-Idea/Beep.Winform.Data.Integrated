namespace TheTechIdea.Beep.Winform.Data.Integrated.Tests.Forms;

internal sealed class EmployeeRecord
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Salary { get; set; }

    public DateTime HireDate { get; set; }

    public bool Active { get; set; }

    public int DepartmentId { get; set; }
}
