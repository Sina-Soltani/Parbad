// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Options
{
    /// <summary>
    /// Provides configuration for Parbad messages.
    /// </summary>
    public class MessagesOptions
    {
        public string PaymentSucceed { get; set; } = "Payment is successful.";

        public string PaymentFailed { get; set; } = "Payment failed.";

        public string DuplicateTrackingNumber { get; set; } = "The tracking number already exists in database. Use a unique one for each requests.";

        public string UnexpectedErrorText { get; set; } = "An unknown error is happened.";

        public string InvalidDataReceivedFromGateway { get; set; } = "Invalid data is received from the gateway.";

        public string PaymentIsAlreadyProcessedBefore { get; set; } = "The requested payment is already processed before.";

        public string PaymentCanceledProgrammatically { get; set; } = "Payment is canceled programmatically.";

        public string OnlyCompletedPaymentCanBeRefunded { get; set; } = "Only a completed payment can be refunded.";
    }
}
