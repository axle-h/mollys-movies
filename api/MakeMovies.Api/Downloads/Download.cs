namespace MakeMovies.Api.Downloads;

public record Download(
    string Id,
    string MovieId,
    int TransmissionId,
    string Name,
    DateTime StartDate,
    bool Complete,
    DownloadStats? Stats = null);

public record DownloadStats(
    string Name,
    double PercentDone,
    bool IsStalled,
    TimeSpan Eta,
    ISet<string> Files);

public enum DownloadField
{
    DateStarted
}