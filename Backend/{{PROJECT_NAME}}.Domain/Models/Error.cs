using {{PROJECT_NAME}}.Domain.Common.Enums;

namespace {{PROJECT_NAME}}.Domain.Models;

public class Error
{
    public int Status { get; }
    public ErrorCode Code { get; }
    public string Message { get; }
    public IDictionary<string, string[]>? ValidationErrors { get; }

    public Error(
        ErrorCode code,
        string message,
        int status = 400,
        IDictionary<string, string[]>? validationErrors = null)
    {
        Code = code;
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Status = status;
        ValidationErrors = validationErrors;
    }

    public static Error Validation(
        ErrorCode code,
        string message,
        IDictionary<string, string[]> validationErrors)
    {
        return new Error(
            code,
            message,
            400,
            validationErrors);
    }

    public static Error Failure(
        ErrorCode code,
        string message,
        int status = 400)
    {
        return new Error(
            code,
            message,
            status);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Error other)
        {
            return false;
        }

        return Code == other.Code &&
               Message == other.Message &&
               Status == other.Status;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Code, Message, Status);
    }

    public override string ToString() =>
        ValidationErrors != null
            ? $"Validation Error: {Code} - {Message}"
            : $"Error: {Code} - {Message}";
}
