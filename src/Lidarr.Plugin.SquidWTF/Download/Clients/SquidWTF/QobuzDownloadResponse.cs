namespace NzbDrone.Core.Download.Clients.SquidWTF;

internal class QobuzDownloadResponse
{
    public string DownloadId { get; set; }
    public string DownloadFolder { get; set; }
}
internal class QobuzDownloadStatusResponse
{
    public bool IsDownloading { get; set; }
    public bool Complete { get; set; }
    public int DownloadedItems { get; set; }
    public int ItemsToDownload { get; set; }
}
