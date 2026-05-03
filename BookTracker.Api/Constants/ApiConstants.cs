namespace BookTracker.Api.Constants;

public static class ApiConstants
{
    public const int MaxCoverImageSize = 5 * 1024 * 1024; // 5MB
    public static readonly string[] AllowedCoverImageTypes = { "image/jpeg", "image/png" };
    public const string DefaultSortBy = "dateadded";
    
    public static class SortFields
    {
        public const string Title = "title";
        public const string Author = "author";
        public const string Rating = "rating";
        public const string DateAdded = "dateadded";
    }
}
