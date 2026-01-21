
namespace Domain.Specifications
{
    //Фильтры запроса результатов
    public class ResultsSpecification
    {
        public string? Name { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public float MinAvgValue { get; set; }
        public float MaxAvgValue { get; set; }
        public float MinAvgExecutionTime { get; set; }
        public float MaxAvgExecutionTime { get; set; }
    }
}
