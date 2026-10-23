namespace EdzestervGenerator.Api.Models;

// A calendar entry: an external activity (e.g. football) or a generated workout.
public class Workout
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime Date { get; set; } // UTC
    public Activity Activity { get; set; }
    public int DurationMinutes { get; set; }
    public Intensity Intensity { get; set; }
    public string? Note { get; set; }

    public List<WorkoutExercise> Exercises { get; set; } = [];
}
