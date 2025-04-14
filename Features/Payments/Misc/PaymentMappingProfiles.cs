using System;
using AutoMapper;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Payments.DTOs;

namespace Dirassati_Backend.Features.Payments.Misc;

public class PaymentMappingProfiles : Profile
{
    public PaymentMappingProfiles()
    {
        CreateMap<Bill, GetBillDto>();
    }
}
