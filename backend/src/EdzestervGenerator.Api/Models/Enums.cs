namespace EdzestervGenerator.Api.Models;

public enum MuscleGroup
{
    UpperBody,
    LowerBody,
    Core,
    Mobility,
    FullBody
}

// Where a piece of equipment is typically available.
// Home = can be owned at home (shown as a chip in the home-equipment step).
// Gym = gym-only, only used when the training location is Gym.
public enum EquipmentLocation
{
    Home,
    Gym
}

public enum Difficulty
{
    Easy,
    Medium,
    Hard
}

public enum Activity
{
    Generated,
    Football,
    Running,
    Cycling,
    Swimming,
    Other
}

public enum Intensity
{
    Low,
    Medium,
    High
}
