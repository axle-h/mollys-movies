namespace MollysMovies.Common.TransmissionRpc;

public record TorrentFile(string Name, long Length, long BytesCompleted);

public record TorrentInfo(int Id, string Name, double PercentDone, bool IsStalled, int Eta, string DownloadDir, List<TorrentFile> Files);