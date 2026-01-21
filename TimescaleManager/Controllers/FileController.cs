using TimescaleManager.ServiceAbstractions;
using Microsoft.AspNetCore.Mvc;

namespace TimescaleManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController : Controller
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() 
        {

            return Ok(await _fileService.GetFilesAsync());

        }

        /// <summary>
        /// Добавление файла csv в БД.
        /// </summary>
        /// <param name="file">Файл csv</param>
        /// <returns></returns>
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> PostFile(IFormFile file)
        {
            try
            {
                await _fileService.UploadFileAsync(file);
            }
            catch (BadHttpRequestException ex) 
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "Ошибка обработки файла",
                    Details = ex.Message
                });
            }
            return Ok();
        }
    }
}
