using CEC.Blazor.Data;

namespace CEC.Weather.Data
{
    public static class DataDictionary
    {
        // Weather Forecast Fields
        public static readonly RecordFieldInfo __WeatherForecastID = new RecordFieldInfo("WeatherForecastID");
        public static readonly RecordFieldInfo __WeatherForecastDate = new RecordFieldInfo("WeatherForecastDate");
        public static readonly RecordFieldInfo __WeatherForecastTemperatureC = new RecordFieldInfo("WeatherForecastTemperatureC");
        public static readonly RecordFieldInfo __WeatherForecastTemperatureF = new RecordFieldInfo("WeatherForecastTemperatureF");
        public static readonly RecordFieldInfo __WeatherForecastPostCode = new RecordFieldInfo("WeatherForecastPostCode");
        public static readonly RecordFieldInfo __WeatherForecastFrost = new RecordFieldInfo("WeatherForecastFrost");
        public static readonly RecordFieldInfo __WeatherForecastSummary = new RecordFieldInfo("WeatherForecastSummary");
        public static readonly RecordFieldInfo __WeatherForecastSummaryValue = new RecordFieldInfo("WeatherForecastSummaryValue");
        public static readonly RecordFieldInfo __WeatherForecastOutlook = new RecordFieldInfo("WeatherForecastOutlook");
        public static readonly RecordFieldInfo __WeatherForecastOutlookValue = new RecordFieldInfo("WeatherForecastOutlookValue");
        public static readonly RecordFieldInfo __WeatherForecastDescription = new RecordFieldInfo("WeatherForecastDescription");
        public static readonly RecordFieldInfo __WeatherForecastDetail = new RecordFieldInfo("WeatherForecastDetail");
        public static readonly RecordFieldInfo __WeatherForecastDisplayName = new RecordFieldInfo("WeatherForecastDisplayName");

        // Weather Station Fields
        public static readonly RecordFieldInfo __WeatherStationID = new RecordFieldInfo("WeatherStationID");
        public static readonly RecordFieldInfo __WeatherStationName = new RecordFieldInfo("WeatherStationName");
        public static readonly RecordFieldInfo __WeatherStationLatitude = new RecordFieldInfo("WeatherStationLatitude");
        public static readonly RecordFieldInfo __WeatherStationLongitude = new RecordFieldInfo("WeatherStationLongitude");
        public static readonly RecordFieldInfo __WeatherStationElevation = new RecordFieldInfo("WeatherStationElevation");

        // Weather Report Fields
        public static readonly RecordFieldInfo __WeatherReportID = new RecordFieldInfo("WeatherReportID");
        public static readonly RecordFieldInfo __WeatherReportDate = new RecordFieldInfo("WeatherReportDate");
        public static readonly RecordFieldInfo __WeatherReportTempMax = new RecordFieldInfo("WeatherReportTempMax");
        public static readonly RecordFieldInfo __WeatherReportTempMin = new RecordFieldInfo("WeatherReportTempMin");
        public static readonly RecordFieldInfo __WeatherReportFrostDays = new RecordFieldInfo("WeatherReportFrostDays");
        public static readonly RecordFieldInfo __WeatherReportRainfall = new RecordFieldInfo("WeatherReportRainfall");
        public static readonly RecordFieldInfo __WeatherReportSunHours = new RecordFieldInfo("WeatherReportSunHours");
        public static readonly RecordFieldInfo __WeatherReportDisplayName = new RecordFieldInfo("WeatherReportDisplayName");
        public static readonly RecordFieldInfo __WeatherReportMonth = new RecordFieldInfo("WeatherReportMonth");
        public static readonly RecordFieldInfo __WeatherReportYear = new RecordFieldInfo("WeatherReportYear");
    }
}
