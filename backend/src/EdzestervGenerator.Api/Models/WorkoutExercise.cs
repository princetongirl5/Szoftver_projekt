namespace EdzestervGenerator.Api.Models;

// One exercise of a generated workout.
public class WorkoutExercise
{
    public int Id { get; set; }
    public int WorkoutId { get; set; }
    public Workout Workout { get; set; } = null!;
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public int Order { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public double? WeightKg { get; set; }
    public bool IsDone { get; set; }
}
