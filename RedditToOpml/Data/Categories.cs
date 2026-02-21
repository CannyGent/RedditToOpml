namespace RedditToOpml.Data;

public static class Categories
{
    public static readonly IReadOnlyList<string> All = new List<string>
    {
        "Glasgow",
        "Misc",
        "Money",
        "Tech"
    }.AsReadOnly();
}
