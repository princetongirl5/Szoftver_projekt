using System.ComponentModel.DataAnnotations;

namespace EdzestervGenerator.Api.DTOs;

public record EquipmentDto(int Id, string Name);

public record UserEquipmentDto(int EquipmentId, string Name, double? WeightKg);

// One item of the "what do I have at home" list.
public record SaveUserEquipmentItem(
    int EquipmentId,
    [Range(0.5, 200)] double? WeightKg);
