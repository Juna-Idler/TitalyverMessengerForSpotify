using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using System.IO;
using System.Text.Json;

using System.Threading;

using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace TitalyverMessengerForSpotify
{
    public class Spotify
    {
        private const string CredentialsPath = "credentials.json";
        private static readonly string clientId = "b46a4c94f1f84529aadaf9cf65b78901";

        public SpotifyClient SpotifyClient { get; private set; }

        public static Spotify Create(bool authorization = false,int millisecondsTimeout = -1)
        {
            if (File.Exists(CredentialsPath) && authorization == false)
            {
                try
                {
                    var json = File.ReadAllText(CredentialsPath);
                    var token = JsonSerializer.Deserialize<PKCETokenResponse>(json);
                    return Create(token);
                }
                catch (Exception)
                {
                    File.Delete(CredentialsPath);
                    return null;
                }
            }
            else
            {
                using CancellationTokenSource Cancellation = new();
                using EmbedIOAuthServer _server = new(new Uri("http://localhost:5000/callback"), 5000);

                PKCETokenResponse token = null;
                var (verifier, challenge) = PKCEUtil.GenerateCodes();
                _server.AuthorizationCodeReceived += async (sender, response) =>
                {
                    try
                    {
                        await _server.Stop();
                        token = await new OAuthClient().RequestToken(
                          new PKCETokenRequest(clientId, response.Code, _server.BaseUri, verifier)
                        );
                        await File.WriteAllTextAsync(CredentialsPath, JsonSerializer.Serialize(token));
                    }
                    catch(Exception)
                    {
                    }
                    finally
                    {
                        Cancellation.Cancel();
                    }
                };
                _server.Start().Wait();

                var request = new LoginRequest(_server.BaseUri, clientId, LoginRequest.ResponseType.Code)
                {
                    CodeChallenge = challenge,
                    CodeChallengeMethod = "S256",
                    Scope = new List<string>
                    {
                        Scopes.UserReadCurrentlyPlaying , Scopes.UserReadPrivate 
                    }
                };

                Uri uri = request.ToUri();
                try
                {
                    BrowserUtil.Open(uri);
                    try
                    {
                        Task.Delay(millisecondsTimeout, Cancellation.Token).Wait();
                    }
                    catch (AggregateException e)
                    {
                        if (e.InnerException is not TaskCanceledException)
                            throw e.InnerException;
                    }
                    return (token != null) ? Create(token) : null;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

        private static Spotify Create(PKCETokenResponse token)
        {
            var authenticator = new PKCEAuthenticator(clientId, token);
            authenticator.TokenRefreshed += (sender, token) => File.WriteAllText(CredentialsPath, JsonSerializer.Serialize(token));

            var config = SpotifyClientConfig.CreateDefault()
              .WithAuthenticator(authenticator).WithHTTPClient(new CustomHttpClient());

            SpotifyClient spotify = new(config);
            return new Spotify() { SpotifyClient = spotify };
        }

    }
}
