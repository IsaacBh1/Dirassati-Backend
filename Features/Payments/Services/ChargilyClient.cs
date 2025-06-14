using Dirassati_Backend.Features.Payments.DTOs;
using System.Net.Http.Headers;


namespace Dirassati_Backend.Features.Payments.Services;

public class ChargilyClient : IChargilyClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ChargilyClient> _logger;

    public ChargilyClient(HttpClient httpClient, IConfiguration configuration, ILogger<ChargilyClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        var apiBaseUrl = configuration["ChargilyConfigs:ApiBaseUrl"]?.TrimEnd('/');
        var apiKey = configuration["ChargilyConfigs:ApiKey"];

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiBaseUrl))
        {
            _logger.LogCritical("Chargily API Key or Base URL is not configured!");
            throw new InvalidOperationException("Chargily client configuration is missing.");
        }

        _httpClient.BaseAddress = new Uri(apiBaseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<ChargilyCheckoutResponse?> CreateCheckoutSessionAsync(ChargilyCreateCheckoutRequest payload)
    {
        var chargilyEndpoint = "checkouts";
        _logger.LogInformation("Sending manual request to Chargily API: POST {Endpoint}", chargilyEndpoint);

        try
        {

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(chargilyEndpoint, payload);

            if (response.IsSuccessStatusCode)
            {

                var result = await response.Content.ReadFromJsonAsync<ChargilyCheckoutResponse>();
                if (result == null)
                {
                    _logger.LogError("Failed to deserialize successful Chargily response (was null).");
                    return null;
                }
                _logger.LogInformation("Manual Chargily checkout session created. Checkout ID: {CheckoutId}", result.id);
                return result;
            }
            else
            {

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to create Chargily checkout session. Status: {StatusCode}, Response: {ErrorResponse}", response.StatusCode, errorContent);

                return null;
            }
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP request error while creating Chargily checkout session. {Error}", httpEx);
            throw; // Re-throw network-related errors
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ChargilyClient CreateCheckoutSessionAsync.");
            throw; // Re-throw network-related errors

        }
    }
}