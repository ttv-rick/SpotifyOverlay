using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using SpotifyAPI.Web;

namespace SpotifyOverlay.Classes
{
    public class SpotifyService
    {
        private readonly SpotifyConfig spotifyConfig;
        private readonly IServiceProvider serviceProvider;
        private SpotifyClient spotifyClient;
        public bool IsAuthed { get; set; }
        public Action<SpotifyTrack> TrackHasChanged;

        private SpotifyTrack currentTrack = null;

        public SpotifyService(SpotifyConfig spotifyConfig, IServiceProvider serviceProvider)
        {
            this.spotifyConfig = spotifyConfig;
            this.serviceProvider = serviceProvider;
        }

        public async Task SetCurrentTrackAsync(CancellationToken cancellationToken)
        {
            if (spotifyClient == null) return;

            var currentlyPlaying =
                await spotifyClient
                    .Player
                    .GetCurrentlyPlaying(
                        new PlayerCurrentlyPlayingRequest(PlayerCurrentlyPlayingRequest.AdditionalTypes.Track)
                    );
            if (currentlyPlaying.Item is FullTrack currentTrack)
            {
                this.currentTrack = new SpotifyTrack
                {
                    Artist = currentTrack.Artists?.FirstOrDefault()?.Name,
                    Album = currentTrack.Album?.Name,
                    AlbumArtUrl = currentTrack.Album?.Images.FirstOrDefault()?.Url,
                    Name = currentTrack.Name
                };

                TrackHasChanged?.Invoke(this.currentTrack);
            }
        }

        public Task PreAuth()
        {
            if (IsAuthed) return Task.CompletedTask;

            // TODO: if we have refresh token, use it for access token
            //   - that will set IsAuthed
            var refreshToken = File.Exists("refresh.txt")
                ? File.ReadAllText("refresh.txt")
                : null;
            
            return !string.IsNullOrEmpty(refreshToken) 
                ? RefreshTokenAsync(refreshToken) 
                : Task.CompletedTask;
        }

        private async Task RefreshTokenAsync(string refreshToken)
        {
            try
            {
                // await File.WriteAllTextAsync("code.txt", code);
                var response = await new OAuthClient().RequestToken(
                    new AuthorizationCodeRefreshRequest(spotifyConfig.ClientId, spotifyConfig.ClientSecret, refreshToken)
                );

                var config = SpotifyClientConfig
                    .CreateDefault()
                    .WithAuthenticator(new AuthorizationCodeAuthenticator(spotifyConfig.ClientId,
                        spotifyConfig.ClientSecret, new AuthorizationCodeTokenResponse()
                        {
                          AccessToken   = response.AccessToken,
                          ExpiresIn =  response.ExpiresIn,
                          RefreshToken =  refreshToken,
                          CreatedAt = response.CreatedAt,
                          Scope = response.Scope,
                          TokenType = response.TokenType
                        }));

                spotifyClient = new SpotifyClient(config);
                IsAuthed = true;
            }
            catch (Exception ex)
            {
                // TODO: figure out what might lead here
                return;
            }
        }

        public async Task<bool> SwapCodeForTokenAsync(string code)
        {
            try
            {
                // await File.WriteAllTextAsync("code.txt", code);
                var response = await new OAuthClient().RequestToken(
                    new AuthorizationCodeTokenRequest(spotifyConfig.ClientId, spotifyConfig.ClientSecret, code,
                        new Uri("https://localhost:5001/callback"))
                );

                await File.WriteAllTextAsync("refresh.txt", response.RefreshToken);

                var config = SpotifyClientConfig
                    .CreateDefault()
                    .WithAuthenticator(new AuthorizationCodeAuthenticator(spotifyConfig.ClientId,
                        spotifyConfig.ClientSecret, response));

                spotifyClient = new SpotifyClient(config);
                IsAuthed = true;

                return true;
            }
            catch (Exception ex)
            {
                // TODO: figure out what might lead here
                return false;
            }
        }
    }
}