namespace RedditToOpml.Data;

public static class Categories
{
    public static readonly IReadOnlyList<string> All = new List<string>
    {
        "Scotland",
        "Misc",
        "Money",
        "Tech",
        "Dev",
        "Cyber"
    }.AsReadOnly();
}
