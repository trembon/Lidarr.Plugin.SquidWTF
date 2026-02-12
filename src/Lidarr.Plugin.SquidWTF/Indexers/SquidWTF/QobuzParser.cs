using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Http;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Plugin.SquidWTF;

namespace NzbDrone.Core.Indexers.SquidWTF
{
    public class QobuzParser : IParseIndexerResponse
    {
        public QobuzIndexerSettings Settings { get; set; }

        public IList<ReleaseInfo> ParseResponse(IndexerResponse response)
        {
            var torrentInfos = new List<ReleaseInfo>();

            var content = new HttpResponse<QobuzSearchResponse>(response.HttpResponse).Resource;

            return [.. content.Items.Select(x => ToReleaseInfo(x, content.SessionId))];
        }

        private ReleaseInfo ToReleaseInfo(QobuzSearchAlbumResponse x, Guid sessionId)
        {
            var downloadUrl = Helpers.BuildUrl(Settings.BaseUrl, "download", new Dictionary<string, string> { { "sessionId", sessionId.ToString() }, { "downloadId", x.Id } });

            var result = new ReleaseInfo
            {
                Guid = $"SquidWTF-Qobuz-{x.Id}",
                Artist = x.Artist,
                Album = x.Album,
                Size = x.TrackCount,
                DownloadUrl = downloadUrl,
                InfoUrl = x.InfoUrl,
                PublishDate = x.ReleaseDate,
                DownloadProtocol = nameof(SquidWTFQobuzDownloadProtocol),
                Codec = "FLAC",
                Container = "24bit Lossless",
                Title = $"{x.Artist} - {x.Album} ({x.ReleaseDate.Year})"
            };

            if (x.Explicit)
            {
                result.Title += " [Explicit]";
            }

            return result;
        }
    }
}
