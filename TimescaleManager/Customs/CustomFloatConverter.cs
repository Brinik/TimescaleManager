using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace TimescaleManager.Customs
{
    //Кастомный конвертер чисел с плавающей запятой
    public class CustomFloatConverter : CsvHelper.TypeConversion.SingleConverter
    {
        public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrEmpty(text))
                throw new CsvHelper.MissingFieldException(row.Context);

            text = text.Replace(',', '.');
            text = text.Replace(" ", "");

            if (float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
            {
                return result;
            }
            // Если не удалось, пробуем с текущей культурой
            return base.ConvertFromString(text, row, memberMapData)!;
        }
    }
}
