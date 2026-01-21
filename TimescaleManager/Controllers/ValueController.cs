using Microsoft.AspNetCore.Mvc;
using TimescaleManager.ServiceAbstractions;

namespace TimescaleManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ValueController : Controller
    {
        private readonly IValueService _valueService;
        public ValueController(IValueService valueService)
        {
            _valueService = valueService;
        }

        /// <summary>
        /// Получение списка последних 10 значений, отсортированных по начальному времени запуска Date по имени заданного файла.
        /// </summary>
        /// <param name="fileName">Имя файла (полное с расширением)</param>
        /// <returns></returns>
        [HttpGet("LastTen")]
        public async Task<IActionResult> GetLastTen(string fileName) 
        {
            try
            {
                return Ok(await _valueService.GetLastTenAsync(fileName));
            }
            catch (BadHttpRequestException ex) 
            {
                return NotFound(ex.Message);
            }
        }
    }
}
