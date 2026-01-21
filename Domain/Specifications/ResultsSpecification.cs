
namespace Domain.Specifications
{
    //Фильтры запроса результатов
    public class ResultsSpecification
    {
        public string? Name { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public double MinAvgValue { get; set; }
        public double MaxAvgValue { get; set; }
        public double MinAvgExecutionTime { get; set; }
        public double MaxAvgExecutionTime { get; set; }
    }
}
