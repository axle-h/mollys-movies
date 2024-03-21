namespace MakeMovies.Api.Downloads.TransmissionRpc;

public record NewTorrentInfo(int Id, string Name, string HashString);

public record TorrentFile(string Name);

public record TorrentInfo(int Id, string Name, double PercentDone, bool IsStalled, int Eta, List<TorrentFile> Files);