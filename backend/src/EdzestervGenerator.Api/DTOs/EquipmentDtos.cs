using System.ComponentModel.DataAnnotations;
using EdzestervGenerator.Api.Models;

namespace EdzestervGenerator.Api.DTOs;

public record EquipmentDto(int Id, string Name, EquipmentLocation Location);
public record UserEquipmentDto(int EquipmentId, string Name, double? WeightKg);
public record SaveUserEquipmentItem(int EquipmentId, [Range(0.5, 200)] double? WeightKg);
