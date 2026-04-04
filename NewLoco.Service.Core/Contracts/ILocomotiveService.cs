using NewLoco.GCommon.Enums;

namespace NewLoco.Service.Core.Contracts;

public interface ILocomotiveService
{
    Task<LocomotiveType> GetTypeAsync(int locomotiveId);                 // keep exact signature
    Task<IEnumerable<LocoNumberDto>> GetAllAsync(string? filter);
    Task<LocoDetailsDto> GetDetailsAsync(int id);

    Task CreateAsync(LocomotiveFormDto model, string user);
    Task<LocomotiveFormDto?> GetForEditAsync(int id);
    Task EditAsync(int id, LocomotiveFormDto model, string user);

    Task<IEnumerable<LocoOptionDto>> GetOptionsAsync();                  // for dropdowns

    Task DeleteAsync(int id, string user);
    Task UndeleteAsync(int id, string user);
 
}