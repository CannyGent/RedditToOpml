namespace RedditToOpml.Data;

public static class Categories
{
    public static readonly IReadOnlyList<string> All = new List<string>
    {
        "Technology",
        "News",
        "Entertainment",
        "Gaming",
        "Sports",
        "Science",
        "Education",
        "Finance",
        "Lifestyle",
        "Other"
    }.AsReadOnly();
}
