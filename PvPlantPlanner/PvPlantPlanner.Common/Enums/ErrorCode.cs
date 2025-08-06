namespace PvPlantPlanner.Common.Enums
{
    public enum ErrorCode
    {
        // General error codes:
        Success = 0,
        NotFound = 1,

        // Application state:
        NotConfigured = 10,

        // Custom error codes:
        InvalidFeedInEnergyPriceComparison = 20,
        UnresolvedFeedInDecision = 21
    }
}
