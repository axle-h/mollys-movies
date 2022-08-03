using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace MollysMovies.Api.Transmission;

public interface IMagnetUriService
{
    string BuildMagnetUri(string name, string hash);
}

public class MagnetUriService : IMagnetUriService
{
    private readonly TransmissionOptions _options;

    public MagnetUriService(IOptions<TransmissionOptions> options)
    {
        _options = options.Value;
    }

    public string BuildMagnetUri(string name, string hash)
    {
        var trackers = _options.Trackers ?? new List<string>();
        if (!trackers.Any())
        {
            throw new Exception("no yts trackers configured");
        }

        var trs = string.Join('&', trackers.Select(x => $"tr={x}"));
        return $"magnet:?xt=urn:btih:{hash}&dn={Uri.EscapeDataString(name)}&{trs}";
    }
}