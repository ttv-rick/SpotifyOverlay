using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace SpotifyOverlay.Classes
{
    public class SpotifyBackgroundService: BackgroundService
    {
        private readonly SpotifyService spotifyService;

        public SpotifyBackgroundService(SpotifyService spotifyService)
        {
            this.spotifyService = spotifyService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // poll for changes
            // tell the service
            // it'll tell it's peeps
            await spotifyService.SetCurrentTrackAsync(stoppingToken);
            
            while (true)
            {
                // TODO: configure this
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

                await spotifyService.SetCurrentTrackAsync(stoppingToken);
            }
        }
    }
}