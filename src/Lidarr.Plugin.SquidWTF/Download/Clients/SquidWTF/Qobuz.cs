using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RemotePathMappings;
using NzbDrone.Plugin.SquidWTF;

namespace NzbDrone.Core.Download.Clients.SquidWTF
{
    public class Qobuz : DownloadClientBase<QobuzSettings>
    {
        public Qobuz(IConfigService configService,
                      IDiskProvider diskProvider,
                      IRemotePathMappingService remotePathMappingService,
                      ILocalizationService localizationService,
                      Logger logger)
            : base(configService, diskProvider, remotePathMappingService, localizationService, logger)
        {
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    try
                    {
                        foreach (var item in queue.Values)
                        {
                            logger.Info("Checking status of download {0} - {1}", item.ClientItem.DownloadId, item.ClientItem.Title);

                            var statusUrl = Helpers.BuildUrl(Settings.BaseUrl, "download/status", new Dictionary<string, string> { { "downloadId", item.ClientItem.DownloadId } });

                            var response = await HttpClient.GetAsync(statusUrl);
                            response.EnsureSuccessStatusCode();

                            var data = await response.Content.ReadFromJsonAsync<QobuzDownloadStatusResponse>();

                            item.ClientItem.RemainingSize = data.ItemsToDownload - data.DownloadedItems;

                            if (data.IsDownloading)
                                item.ClientItem.Status = DownloadItemStatus.Downloading;

                            if (data.Complete)
                                item.ClientItem.Status = DownloadItemStatus.Completed;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error updating download statuses");
                    }
                }
            });
        }

        public override string Protocol => nameof(SquidWTFQobuzDownloadProtocol);

        public override string Name => "SquidWTF Qobuz";

        private static readonly ConcurrentDictionary<string, DownloadItem> queue = new();

        private HttpClient _httpClient;
        private HttpClient HttpClient => _httpClient ??= new HttpClient() { BaseAddress = new Uri(Settings.BaseUrl) };

        public override IEnumerable<DownloadClientItem> GetItems()
        {
            return queue.Values.Select(x => x.ClientItem);
        }

        public override void RemoveItem(DownloadClientItem item, bool deleteData)
        {
            if (deleteData)
                DeleteItemData(item);

            queue.TryRemove(item.DownloadId, out _);
        }

        public override async Task<string> Download(RemoteAlbum remoteAlbum, IIndexer indexer)
        {
            _logger.Info("Initiating download for album {0} - {1}", remoteAlbum.Release.Guid, remoteAlbum.Release.Title);

            var response = await HttpClient.PostAsync(remoteAlbum.Release.DownloadUrl, null);
            response.EnsureSuccessStatusCode();

            var data = await response.Content.ReadFromJsonAsync<QobuzDownloadResponse>();
            var downloadItem = ToDownloadClientItem(remoteAlbum, data);

            queue.TryAdd(downloadItem.DownloadId, new DownloadItem
            {
                ClientItem = downloadItem,
                IndexerData = remoteAlbum
            });

            return downloadItem.DownloadId;
        }

        public override DownloadClientInfo GetStatus()
        {
            return new DownloadClientInfo
            {
                IsLocalhost = true,
                OutputRootFolders = new() { new OsPath(Settings.DownloadPath) }
            };
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            // given the way the code is setup, we don't really need to do anything here
        }

        private DownloadClientItem ToDownloadClientItem(RemoteAlbum x, QobuzDownloadResponse data)
        {
            var item = new DownloadClientItem
            {
                DownloadId = data.DownloadId,
                Title = x.Release.Title,
                RemainingSize = x.Release.Size,
                TotalSize = x.Release.Size,
                Status = DownloadItemStatus.Queued,
                OutputPath = new OsPath(Settings.DownloadPath),
                CanMoveFiles = true,
                CanBeRemoved = true,
            };

            return item;
        }

        private class DownloadItem
        {
            public DownloadClientItem ClientItem { get; set; }
            public RemoteAlbum IndexerData { get; set; }
        }
    }
}
