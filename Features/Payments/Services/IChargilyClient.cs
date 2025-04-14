using Dirassati_Backend.Features.Payments.DTOs;

namespace Dirassati_Backend.Features.Payments.Services;

public interface IChargilyClient
{
    Task<ChargilyCheckoutResponse?> CreateCheckoutSessionAsync(ChargilyCreateCheckoutRequest payload);

}
