using InsuranceAgency.Application.Common.Validation;

namespace InsuranceAgency.Tests.Unit.Common;

public class VerificationRulesTests
{
    [Fact]
    public void IsRequiredType_WithFullName_ReturnsTrue()
    {
        // Act
        var result = VerificationRules.IsRequiredType("FullName");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsRequiredType_WithPassport_ReturnsTrue()
    {
        // Act
        var result = VerificationRules.IsRequiredType("Passport");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsRequiredType_WithPhone_ReturnsFalse()
    {
        // Act
        var result = VerificationRules.IsRequiredType("Phone");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsRequiredType_WithEmail_ReturnsFalse()
    {
        // Act
        var result = VerificationRules.IsRequiredType("Email");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsRequiredType_WithNull_ReturnsFalse()
    {
        // Act
        var result = VerificationRules.IsRequiredType(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsRequiredType_WithEmptyString_ReturnsFalse()
    {
        // Act
        var result = VerificationRules.IsRequiredType("");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSameType_WithMatchingTypes_ReturnsTrue()
    {
        // Act
        var result = VerificationRules.IsSameType("FullName", "FullName");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSameType_WithCaseInsensitive_ReturnsTrue()
    {
        // Act
        var result = VerificationRules.IsSameType("fullname", "FullName");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSameType_WithDifferentTypes_ReturnsFalse()
    {
        // Act
        var result = VerificationRules.IsSameType("FullName", "Passport");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSameType_WithNull_ReturnsFalse()
    {
        // Act
        var result = VerificationRules.IsSameType(null, "FullName");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RequiredPersonalDataTypes_ContainsFullNameAndPassport()
    {
        // Assert
        VerificationRules.RequiredPersonalDataTypes.Should().Contain("FullName");
        VerificationRules.RequiredPersonalDataTypes.Should().Contain("Passport");
        VerificationRules.RequiredPersonalDataTypes.Should().HaveCount(2);
    }
}


