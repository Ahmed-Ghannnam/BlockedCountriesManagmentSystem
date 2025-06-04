namespace BlockedCountries.BL.Dtos
{
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponseDto<T> SuccessResponse(T data, string? message = null)
        {
            return new ApiResponseDto<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponseDto<T> ErrorResponse(string errorMessage, List<string>? errors = null)
        {
            return new ApiResponseDto<T>
            {
                Success = false,
                Message = errorMessage,
                Errors = errors
            };
        }
    }

}
