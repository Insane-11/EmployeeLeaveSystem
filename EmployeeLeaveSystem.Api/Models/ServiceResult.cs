namespace EmployeeLeaveSystem.Api.Models;

public record ServiceResult<T>(bool IsSuccess, T? Data, string? ErrorMessage)
{
    public static ServiceResult<T> Success(T data) => new(true, data, null);
    public static ServiceResult<T> Failure(string message) => new(false, default, message);
}
