namespace Dirassati_Backend.Features.Notes.Dtos
{
    public class CreateNoteDto
    {
        public Guid StudentId { get; set; }
        public int Tremester { get; set; }
        public Guid TeacherId { get; set; }
        public int AcademicYearId { get; set; }
        public int ExamTypeId { get; set; }
        public int SubjectId { get; set; }
        public double Value { get; set; }
        public Guid GroupId { get; set; }
    }
}