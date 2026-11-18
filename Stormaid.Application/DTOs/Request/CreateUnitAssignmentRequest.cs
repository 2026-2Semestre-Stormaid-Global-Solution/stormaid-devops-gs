namespace Stormaid.Application.DTOs.Request;

/// <summary>
/// Dados para vincular um socorrista a uma unidade de resgate.
/// </summary>
public sealed record CreateUnitAssignmentRequest(
    long RescueUnitId,
    long ResponderUserId
);
