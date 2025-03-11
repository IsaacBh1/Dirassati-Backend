using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dirassati_Backend.Features.Parents.Dtos
{
    public class getStudentDto
    {
        public Guid StudentId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateOnly EnrollmentDate { get; set; }
        public string Grade { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    
}