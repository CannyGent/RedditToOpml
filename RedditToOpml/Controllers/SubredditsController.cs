using Microsoft.AspNetCore.Mvc;
using RedditToOpml.Data;
using Serilog;

namespace RedditToOpml.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubredditsController(Repository repository) : ControllerBase
{
    [HttpPost("sync")]
    [Consumes("application/json")]
    public IActionResult SyncSubreddits([FromBody] string[] subreddits)
    {
        if (subreddits == null)
        {
            return BadRequest("Subreddits array is required");
        }

        var existingSubreddits = repository.Subscriptions.Select(s => s.Subreddit).ToHashSet();
        var newSubreddits = subreddits.ToHashSet();

        // Remove subreddits that are no longer in the list
        var toRemove = existingSubreddits.Except(newSubreddits).ToList();
        repository.Subscriptions.RemoveAll(s => toRemove.Contains(s.Subreddit));

        // Add new subreddits with default category "Technology"
        var toAdd = newSubreddits.Except(existingSubreddits).ToList();
        foreach (var subreddit in toAdd)
        {
            repository.Subscriptions.Add(new Subscription
            {
                Subreddit = subreddit,
                Category = "Technology"
            });
        }

        repository.Save();

        Log.Information("Synced subreddits: {Added} added, {Removed} removed", toAdd.Count, toRemove.Count);

        return Ok(new
        {
            Added = toAdd,
            Removed = toRemove,
            Total = repository.Subscriptions.Count
        });
    }
}
