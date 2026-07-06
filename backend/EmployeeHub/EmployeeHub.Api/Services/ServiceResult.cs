namespace EmployeeHub.Api.Services;

public enum ResultStatus
{
    Success,
    NotFound,
    Invalid
}

/// <summary>
/// Lightweight outcome wrapper for service operations that can fail in more than
/// one way (e.g. the target record is missing, or a referenced record is invalid).
/// </summary>
public record ServiceResult<T>(ResultStatus Status, T? Value = default, string? Error = null)
{
    public static ServiceResult<T> Ok(T value) => new(ResultStatus.Success, value);

    public static ServiceResult<T> NotFound() => new(ResultStatus.NotFound);

    public static ServiceResult<T> Invalid(string error) => new(ResultStatus.Invalid, Error: error);
}

/// <summary>
/// Outcome of a delete operation: the record may be missing, in use by other
/// records, or successfully removed.
/// </summary>
public enum DeleteResult
{
    Deleted,
    NotFound,
    InUse
}
