using System.Diagnostics;
using System.Net;
using Dropbox.Api;

namespace Engine.Dropbox;

internal class AuthManager
{
    private const string LoopbackHost = "http://127.0.0.1:8080/";
    private readonly Uri _redirectUri = new(LoopbackHost + "authorize");
    private readonly Uri _jsRedirectUri = new(LoopbackHost + "token");

    private readonly KeyManager _keyManager = new();

    private async Task HandleOAuth2Redirect(HttpListener http)
    {
        var context = await http.GetContextAsync();

        while (context.Request.Url.AbsolutePath != _redirectUri.AbsolutePath)
        {
            context = await http.GetContextAsync();
        }

        context.Response.ContentType = "text/html";

        using (var file = File.OpenRead("index.html"))
        {
            file.CopyTo(context.Response.OutputStream);
        }

        context.Response.OutputStream.Close();
    }

    private async Task<OAuth2Response> HandleJSRedirect(HttpListener http)
    {
        var context = await http.GetContextAsync();

        while (context.Request.Url.AbsolutePath != _jsRedirectUri.AbsolutePath)
        {
            context = await http.GetContextAsync();
        }

        var redirectUri = new Uri(context.Request.QueryString["url_with_fragment"]);
        var result = DropboxOAuth2Helper.ParseTokenFragment(redirectUri);

        return result;
    }

    public async Task GetAccessToken()
    {
        var state = Guid.NewGuid().ToString("N");
        var authorizeUri =
            DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, _keyManager.GetDropBoxKey(), _redirectUri,
                state: state);

        var http = new HttpListener();
        http.Prefixes.Add(LoopbackHost);
        http.Start();

        Process.Start(new ProcessStartInfo(authorizeUri.ToString()) { UseShellExecute = true });

        await HandleOAuth2Redirect(http);

        var result = await HandleJSRedirect(http);

        if (result.State != state)
        {
            return;
        }

        var tkn = result.AccessToken;
        _keyManager.SaveDropboxToken(tkn);
    }
}