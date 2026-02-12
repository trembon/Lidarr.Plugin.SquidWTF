using System;

namespace NzbDrone.Core.Indexers.SquidWTF;

public class QobuzSearchResponse
{
    public Guid SessionId { get; set; }
    public QobuzSearchAlbumResponse[] Items { get; set; }
}

public class QobuzSearchAlbumResponse
{
    public string Id { get; set; }
    public string Artist { get; set; }
    public string Album { get; set; }
    public DateTime ReleaseDate { get; set; }
    public bool Explicit { get; set; }
    public int TrackCount { get; set; }
    public int Duration { get; set; }
    public string InfoUrl { get; set; }
    public string Type { get; set; }
}
