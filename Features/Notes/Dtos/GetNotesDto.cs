namespace Dirassati_Backend.Features.Notes.Dtos
{
    public class StudentNoteDto
    {
        public int NoteId { get; set; }
        public double Value { get; set; }
        public int Tremester { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string ExamTypeName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
    
    public class StudentNotesResponseDto
    {
        public string StudentName { get; set; } = string.Empty;
        public int SchoolLevel { get; set; } 
        public string GroupName { get; set; } = string.Empty;
        public List<StudentNoteDto> Notes { get; set; } = [];
    }
}