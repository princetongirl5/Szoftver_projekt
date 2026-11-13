using EdzestervGenerator.Api.Data.Seed;
using EdzestervGenerator.Api.Models;

namespace EdzestervGenerator.Tests.Services;

// Checks the hand-written seed data (no wger import).
public sealed class SeedDataTests
{
    [Fact]
    public void Equipment_ids_are_1_to_12_in_order()
    {
        Assert.Equal(Enumerable.Range(1, 12), EquipmentSeed.Items.Select(e => e.Id));
    }

    [Fact]
    public void Equipment_1_to_7_is_home_and_8_to_12_is_gym()
    {
        Assert.All(EquipmentSeed.Items.Where(e => e.Id <= 7), e => Assert.Equal(EquipmentLocation.Home, e.Location));
        Assert.All(EquipmentSeed.Items.Where(e => e.Id >= 8), e => Assert.Equal(EquipmentLocation.Gym, e.Location));
    }

    [Fact]
    public void Equipment_has_unique_hungarian_and_english_names()
    {
        Assert.All(EquipmentSeed.Items, e =>
        {
            Assert.False(string.IsNullOrWhiteSpace(e.Name));
            Assert.False(string.IsNullOrWhiteSpace(e.NameEn));
        });
        Assert.Equal(EquipmentSeed.Items.Length, EquipmentSeed.Items.Select(e => e.Name).Distinct().Count());
        Assert.Equal(EquipmentSeed.Items.Length, EquipmentSeed.Items.Select(e => e.NameEn).Distinct().Count());
    }

    [Fact]
    public void Exercise_ids_are_unique_and_continuous()
    {
        Assert.Equal(Enumerable.Range(1, ExerciseSeed.Items.Length), ExerciseSeed.Items.Select(e => e.Id));
    }

    [Fact]
    public void Exercises_have_unique_hungarian_and_english_names()
    {
        Assert.All(ExerciseSeed.Items, e =>
        {
            Assert.False(string.IsNullOrWhiteSpace(e.Name));
            Assert.False(string.IsNullOrWhiteSpace(e.NameEn));
        });
        Assert.Equal(ExerciseSeed.Items.Length, ExerciseSeed.Items.Select(e => e.Name).Distinct().Count());
        Assert.Equal(ExerciseSeed.Items.Length, ExerciseSeed.Items.Select(e => e.NameEn).Distinct().Count());
    }

    [Fact]
    public void Exercises_point_to_existing_equipment_and_have_valid_enums()
    {
        var equipmentIds = EquipmentSeed.Items.Select(e => e.Id).ToHashSet();

        Assert.All(ExerciseSeed.Items, e =>
        {
            if (e.EquipmentId is not null) Assert.Contains(e.EquipmentId.Value, equipmentIds);
            Assert.True(Enum.IsDefined(e.MuscleGroup));
            Assert.True(Enum.IsDefined(e.Difficulty));
        });
    }

    [Fact]
    public void Min_weight_is_only_set_for_exercises_with_equipment()
    {
        Assert.All(ExerciseSeed.Items.Where(e => e.EquipmentId is null), e => Assert.Null(e.MinWeightKg));
        Assert.All(ExerciseSeed.Items.Where(e => e.MinWeightKg is not null), e => Assert.True(e.MinWeightKg > 0));
    }

    [Theory]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    public void Every_gym_equipment_has_at_least_three_exercises(int equipmentId)
    {
        Assert.True(ExerciseSeed.Items.Count(e => e.EquipmentId == equipmentId) >= 3);
    }

    [Fact]
    public void Gym_equipment_exercises_cover_upper_lower_and_core()
    {
        var gym = ExerciseSeed.Items.Where(e => e.EquipmentId >= 8).ToList();

        Assert.Contains(gym, e => e.MuscleGroup == MuscleGroup.UpperBody);
        Assert.Contains(gym, e => e.MuscleGroup == MuscleGroup.LowerBody);
        Assert.Contains(gym, e => e.MuscleGroup == MuscleGroup.Core);
    }

    [Fact]
    public void Staple_pushup_is_the_first_pushup_in_the_seed()
    {
        // The generator takes the first exercise whose name contains "fekvőtámasz".
        var first = ExerciseSeed.Items.First(e => e.Name.Contains("fekvőtámasz", StringComparison.OrdinalIgnoreCase));

        Assert.Equal(1, first.Id);
        Assert.Equal("Fekvőtámasz", first.Name);
    }
}
