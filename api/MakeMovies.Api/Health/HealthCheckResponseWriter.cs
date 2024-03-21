using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MakeMovies.Api.Health;

public static class HealthCheckResponseWriter
{
    public static Task WriteResponse(HttpContext context, HealthReport healthReport)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var entries = healthReport.Entries.ToDictionary(x => x.Key, x =>
            new HealthReportEntryDto(x.Value.Status, x.Value.Description, x.Value.Duration, x.Value.Exception?.Message, x.Value.Data));
        var dto = new HealthReportDto(healthReport.Status, healthReport.TotalDuration, entries);

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = {new JsonStringEnumConverter()},
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        return context.Response.WriteAsync(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(json)));
    }

    private record HealthReportEntryDto(
        HealthStatus Status, string? Description, TimeSpan Duration, string? Error, IReadOnlyDictionary<string, object>? Data);

    private record HealthReportDto(HealthStatus Status, TimeSpan TotalDuration, Dictionary<string, HealthReportEntryDto> Entries);
}