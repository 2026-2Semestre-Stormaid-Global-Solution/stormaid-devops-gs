using Stormaid.Domain.Enums;

namespace Stormaid.Application.DTOs.Request;

/// <summary>
/// Representa os dados necessários para atualizar
/// uma unidade institucional de resgate.
/// </summary>
public sealed record UpdateRescueUnitRequest(
    string UnitCode,
    string DisplayName,
    ResponderInstitutionType InstitutionType,
    int Capacity,
    bool SupportsAccessibleTransport
);
