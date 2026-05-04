using System;

/// <summary>
/// Possible website types
/// </summary>
public enum WebsiteType
{
    SOCIAL_MEDIA = 0,
    GAME = 1,
    SEARCH_ENGINE = 2,
    INFO = 3,
    STREAMING = 4,
}

/// <summary>
/// The controller for websites
/// </summary>
public struct Website
{
    /// <summary>
    /// The URL of the website
    /// </summary>
    public string Url;
    /// <summary>
    /// The security level of the website
    /// </summary>
    public int Security;
    /// <summary>
    /// The type of website
    /// </summary>
    public WebsiteType Type;

    /// <summary>
    /// Create a website instance
    /// </summary>
    /// <param name="url">The website URL</param>
    /// <param name="security">The security level</param>
    /// <param name="type">The type of the website</param>
    public Website(string url, int security, WebsiteType type)
    {
        Url = url;
        Security = security;
        Type = type;
    }

    /// <summary>
    /// Get the discount based on the current weekday passed
    /// </summary>
    /// <param name="weekday">The weekday to check</param>
    /// <returns>The discount value, for this website and if applicable.</returns>
    public double GetDayDiscount(DayOfWeek weekday)
    {
        switch (weekday)
        {
            case DayOfWeek.Sunday:
                return Type == WebsiteType.SEARCH_ENGINE ? 0.8d : 1d;
            case DayOfWeek.Monday:
                return 1.1d;
            case DayOfWeek.Tuesday:
                return Type == WebsiteType.GAME ? 0.8d : 1d;
            case DayOfWeek.Wednesday:
                return Type == WebsiteType.INFO ? 0.8d : 1d;
            case DayOfWeek.Thursday:
                return Type == WebsiteType.SOCIAL_MEDIA ? 0.8d : 1d;
            case DayOfWeek.Friday:
                return 0.9d;
            case DayOfWeek.Saturday:
                return Type == WebsiteType.STREAMING ? 0.8d : 1d;
            default:
                return 1d;
        }
    }

}