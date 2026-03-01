using System;

namespace Application.Payment.Events;

public record StripeEventResult(string Type, string Status, string IntentId, long Amount);
