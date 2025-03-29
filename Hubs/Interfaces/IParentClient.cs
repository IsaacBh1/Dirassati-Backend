using Dirassati_Backend.Features.Teachers.Dtos;

namespace Dirassati_Backend.Hubs.Interfaces;

public interface IParentClient
{
    Task ReceiveAbsenceNotification(object notification);
    Task ReceiveBroadcastNotification(string message);
    Task ReceiveNewReport(GetStudentReportDto report);
}