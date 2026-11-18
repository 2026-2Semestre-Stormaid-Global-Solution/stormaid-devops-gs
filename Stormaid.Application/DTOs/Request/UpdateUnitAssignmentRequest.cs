namespace Stormaid.Application.DTOs.Request;

/// <summary>
/// Dados para trocar o socorrista de um vínculo ativo.
/// </summary>
public sealed record UpdateUnitAssignmentRequest(
    long ResponderUserId
);
