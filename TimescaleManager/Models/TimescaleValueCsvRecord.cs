using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;
using Domain.Entities;

namespace TimescaleManager.Models
{
    public class TimescaleValueCsvRecord
    {
        [Index(0)]
        public string Date { get; set; } = string.Empty;
        [Index(1)]
        public float ExecutionTime { get; set; }
        [Index(2)]
        public float Value { get; set; }
    }
}
