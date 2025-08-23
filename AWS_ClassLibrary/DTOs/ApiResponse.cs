namespace AWS_ClassLibrary.DTOs;

public class ApiResponse<T>
{
    public required bool Success { get; set; }
    public required int StatusCode { get; set; }
    public required string Message { get; set; }
    public T? Data { get; set; }
}
