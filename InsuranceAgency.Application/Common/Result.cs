namespace InsuranceAgency.Application.Common
{
    public class Result
    {
        public bool Success { get; }
        public string? Error { get; }

        private Result(bool success, string? error)
        {
            Success = success;
            Error = error;
        }

        public static Result Ok() => new(true, null);
        public static Result Fail(string message) => new(false, message);
    }

    public class Result<T>
    {
        public bool Success { get; }
        public string? Error { get; }
        public T? Value { get; }

        private Result(bool success, T? value, string? error)
        {
            Success = success;
            Value = value;
            Error = error;
        }

        public static Result<T> Ok(T value) => new(true, value, null);
        public static Result<T> Fail(string error) => new(false, default, error);
    }
}
