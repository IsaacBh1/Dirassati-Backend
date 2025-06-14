namespace Dirassati_Backend.Features.Payments.DTOs;

// --- DTOs for your API Endpoints ---

public class InitiatePaymentRequestDto
{

    public Guid StudentId { get; set; }
}

public class InitiatePaymentResponseDto
{
    public string CheckoutUrl { get; set; } = null!;
    public string CheckoutId { get; set; } = null!;
    
}


// --- DTOs mirroring Chargily's API (Manual Definition) ---

public class ChargilyCreateCheckoutRequest
{
    public long Amount { get; set; } // Assuming cents

    public string Currency { get; set; } = "dzd";

    public string success_url { get; set; } = null!;

    public string failure_url { get; set; } = null!;

    public string? webhook_endpoint { get; set; }

    // CRUCIAL: Metadata to link webhook back
    public Dictionary<string, string>? Metadata { get; set; }

}

public class ChargilyCheckoutResponse
{
    public string id { get; set; } = null!; // Checkout ID

    public string checkout_url { get; set; } = null!;
    public string webhook_endpoint { get; set; } = null!;

    // Include other fields if needed...
}

// Structure matching Chargily Webhook Payload Example
public class ChargilyWebhookPayload
{
    public string Id { get; set; } = null!; // Event ID

    public string Type { get; set; } = null!; // e.g., "checkout.paid"

    public ChargilyCheckoutObjectData Data { get; set; } = null!;

    public long CreatedAtUnix { get; set; }
}

// Structure matching the "data" object within Chargily Webhook
public class ChargilyCheckoutObjectData
{
    public string Id { get; set; } = null!; // Checkout ID

    public string Status { get; set; } = null!;

    public long Amount { get; set; } // Assuming cents

    public string Currency { get; set; } = null!;

    public string? InvoiceId { get; set; }

    // CRUCIAL: Expect metadata back from Chargily
    public Dictionary<string, string>? Metadata { get; set; }

    public long? UpdatedAtUnix { get; set; }
    // Add other fields if needed
}