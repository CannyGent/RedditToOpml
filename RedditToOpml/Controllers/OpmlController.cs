using Microsoft.AspNetCore.Mvc;
using RedditToOpml.Data;
using System.Text;

namespace RedditToOpml.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OpmlController(Repository repository) : ControllerBase
{
    [HttpGet]
    [Produces("application/xml")]
    public IActionResult GetOpml()
    {
        var subscriptions = repository.Subscriptions;
        var opmlXml = GenerateOpmlXml(subscriptions);
        return Content(opmlXml, "application/xml", Encoding.UTF8);
    }

    private static string GenerateOpmlXml(IEnumerable<Subscription> subscriptions)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<opml version=\"2.0\">");
        sb.AppendLine("  <head>");
        sb.AppendLine("    <title>Reddit Subscriptions</title>");
        sb.AppendLine("  </head>");
        sb.AppendLine("  <body>");

        // Group subscriptions by category
        var groupedByCategory = subscriptions
            .GroupBy(s => s.Category ?? "Uncategorized")
            .OrderBy(g => g.Key);

        foreach (var categoryGroup in groupedByCategory)
        {
            var categoryEscaped = XmlEncode(categoryGroup.Key);
            sb.AppendLine($"    <outline text=\"{categoryEscaped}\" title=\"{categoryEscaped}\">");

            foreach (var subscription in categoryGroup.OrderBy(s => s.Subreddit))
            {
                var subredditEscaped = XmlEncode(subscription.Subreddit);
                var rssUrl = $"https://www.reddit.com/{subscription.Subreddit}/top.rss?t=week&limit=50";
                sb.AppendLine($"      <outline type=\"rss\" text=\"Reddit {subredditEscaped}\" title=\"Reddit {subredditEscaped}\" xmlUrl=\"{rssUrl}\"/>");
            }

            sb.AppendLine("    </outline>");
        }

        sb.AppendLine("  </body>");
        sb.AppendLine("</opml>");

        return sb.ToString();
    }

    private static string XmlEncode(string input)
    {
        return System.Security.SecurityElement.Escape(input);
    }
}
