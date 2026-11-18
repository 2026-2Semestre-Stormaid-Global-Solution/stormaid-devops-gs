using Stormaid.Application.DTOs.Request;
using Stormaid.Application.DTOs.Response;
using Stormaid.Application.Exceptions;
using Stormaid.Application.Interfaces;
using Stormaid.Domain.Entities;

namespace Stormaid.Application.Services;

/// <summary>
/// Serviço responsável pelas operações de cadastro
/// das unidades institucionais de resgate.
/// </summary>
public sealed class RescueUnitCommandService
{
    private readonly IRescueUnitRepository _repository;

    public RescueUnitCommandService(IRescueUnitRepository repository)
    {
        _repository = repository;
    }

    public async Task<RescueUnitResponse> CreateAsync(
        CreateRescueUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var unit = new RescueUnit(
            request.UnitCode,
            request.DisplayName,
            request.InstitutionType,
            request.Capacity,
            request.SupportsAccessibleTransport,
            DateTime.UtcNow
        );

        var codeExists = await _repository.ExistsByUnitCodeAsync(
            unit.UnitCode,
            cancellationToken
        );

        if (codeExists)
        {
            throw new RescueUnitCodeAlreadyExistsException(
                unit.UnitCode
            );
        }

        await _repository.AddAsync(
            unit,
            cancellationToken
        );

        return RescueUnitResponse.FromEntity(unit);
    }

    public async Task<RescueUnitResponse?> UpdateAsync(
        long id,
        UpdateRescueUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var unit = await _repository.GetTrackedByIdAsync(
            id,
            cancellationToken
        );

        if (unit is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.UnitCode))
        {
            var normalizedCode = request.UnitCode.Trim();

            var codeExists = await _repository.ExistsByUnitCodeExcludingIdAsync(
                normalizedCode,
                id,
                cancellationToken
            );

            if (codeExists)
            {
                throw new RescueUnitCodeAlreadyExistsException(
                    normalizedCode
                );
            }
        }

        unit.UpdateDetails(
            request.UnitCode,
            request.DisplayName,
            request.InstitutionType,
            request.Capacity,
            request.SupportsAccessibleTransport,
            DateTime.UtcNow
        );

        await _repository.UpdateAsync(
            unit,
            cancellationToken
        );

        return RescueUnitResponse.FromEntity(unit);
    }

    public async Task<bool> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var unit = await _repository.GetTrackedByIdAsync(
            id,
            cancellationToken
        );

        if (unit is null)
        {
            return false;
        }

        await _repository.RemoveAsync(
            unit,
            cancellationToken
        );

        return true;
    }
}