namespace BookTracker.Api.Models;

public class ReadingGoal
{
    public int Id { get; set; }
    public int TargetYear { get; set; }
    public int GoalCount { get; set; }
    public Guid UserId { get; set; }
}
