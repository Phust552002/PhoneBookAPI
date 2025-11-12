#nullable enable
using System.Collections.Generic;

namespace PhoneBookDbNormalized.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int ParentId { get; set; }
        public int Level { get; set; }
        public string RootName { get; set; } = string.Empty;
        public int Status { get; set; }

        // Dùng để dựng cây
        public List<Department> Children { get; set; } = new();
    }

}
