using System.ComponentModel.DataAnnotations;

namespace CarWash.Api.DTOs;

public record ServiceLocationDto(int Id, string Name);

public record SaveServiceLocationDto(
    [Required, MaxLength(100)] string Name);
