// This file makes Program class accessible for integration tests
// In .NET 8 with top-level statements, Program is implicitly internal
// This partial class declaration makes it public for testing

namespace InsuranceAgency.Web;

public partial class Program { }

