using System;
using System.Linq;

namespace InsuranceAgency.Application.Common.Validation
{
    /// <summary>
    /// Общие правила верификации персональных данных клиента.
    /// </summary>
    public static class VerificationRules
    {
        public static readonly string[] SupportedPersonalDataTypes =
        {
            "FullName",
            "Passport",
            "Phone",
            "Email",
            "Other"
        };

        // Поля, которые должны быть подтверждены до активации договора.
        public static readonly string[] RequiredPersonalDataTypes =
        {
            "FullName",
            "Passport"
        };

        public static bool IsRequiredType(string? documentType) =>
            !string.IsNullOrWhiteSpace(documentType) &&
            RequiredPersonalDataTypes.Any(t => string.Equals(t, documentType, StringComparison.OrdinalIgnoreCase));

        public static bool IsSameType(string? documentType, string expectedType) =>
            !string.IsNullOrWhiteSpace(documentType) &&
            string.Equals(documentType, expectedType, StringComparison.OrdinalIgnoreCase);
    }
}

