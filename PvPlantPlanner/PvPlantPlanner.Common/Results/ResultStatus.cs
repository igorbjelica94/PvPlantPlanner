using PvPlantPlanner.Common.Enums;

namespace PvPlantPlanner.Common.Results
{
    public class ResultStatus(bool success, ErrorCode errorCode = ErrorCode.Success, string? errorMessage = null)
    {
        public bool Success { get; } = success;
        public ErrorCode ErrorCode { get; } = errorCode;
        public string? ErrorMessage { get; } = errorMessage;

        public static ResultStatus Ok() => new ResultStatus(true);
        public static ResultStatus Fail(ErrorCode errorCode, string? errorMessage = null) =>
            new ResultStatus(false, errorCode, errorMessage);
    }
}
