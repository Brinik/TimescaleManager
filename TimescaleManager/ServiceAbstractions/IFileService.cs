using TimescaleManager.DTO;

namespace TimescaleManager.ServiceAbstractions
{
    public interface IFileService
    {
        /// <summary>
        /// Получить все имена файлов в БД
        /// </summary>
        /// <returns></returns>
        Task<ICollection<TimescaleFileDTO>> GetFilesAsync();

        /// <summary>
        /// Загрузить файл в БД
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        Task UploadFileAsync(IFormFile file);
    }
}
