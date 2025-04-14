using Dirassati_Backend.Features.Payments.DTOs;
using Dirassati_Backend.Features.Teachers.Dtos;

namespace Dirassati_Backend.Hubs.Interfaces;

public interface IParentClient
{
    Task ReceiveAbsenceNotification(object notification);
    Task ReceiveBroadcastNotification(string message);
    Task ReceiveNewReport(GetStudentReportDto report);
    Task ReceiveNewStudentBill(GetBillDto billDto);
    Task ReceivePendingBillNotification(List<GetBillDto> bills);
    Task ReceivePaymentBillSuccess(StudentPaymentBillDto report);
    Task ReceivePaymentBillUpdate(StudentPaymentBillDto paymentNotification);
}