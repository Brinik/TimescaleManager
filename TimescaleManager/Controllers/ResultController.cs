using Microsoft.AspNetCore.Mvc;
using TimescaleManager.ServiceAbstractions;
using Domain.Specifications;

namespace TimescaleManager.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResultController : Controller
    {
        private readonly IResultService _resultService;
        public ResultController(IResultService resultService)
        {
            _resultService = resultService;
        }

        /// <summary>
        ///  получение списка записей из таблицы Results, подходящих под фильтры.
        /// </summary>
        /// <param name="resultsSpecification">Фильтры поиска</param>
        /// <returns></returns>
        [HttpPost("GetFiltered")]
        public async Task<IActionResult> GetFilteredResults(ResultsSpecification resultsSpecification)
        {
            try
            {
                return Ok( await _resultService.GetResultsRangeAsync(resultsSpecification));
            }
            catch (BadHttpRequestException ex) 
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
