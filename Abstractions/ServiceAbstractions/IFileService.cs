using Microsoft.AspNetCore.Http;
using Domain.DTO;

namespace Domain.ServiceAbstractions
{
    public interface IFileService
    {
        Task<ICollection<TimescaleFileDTO>> GetFilesAsync();
        Task UploadFileAsync(IFormFile file);
    }
}
