﻿using Newtonsoft.Json;
using System;
using System.Net;
using System.Web.Http;

namespace Website.Controllers
{
    public class GitHubController : ApiController
    {
        private const string GITHUB_APP_CLIENT_ID = "";
        private const string GITHUB_APP_CLIENT_SECRET = "";

        private const string GITHUB_APP_OAUTH_URL = "https://github.com/login/oauth/authorize";
        private const string GITHUB_APP_OAUTH_ACCESS_URL = "https://github.com/login/oauth/access_token";
        private const string GITHUB_APP_OAUTH_REDIRECT_URL = "http://localhost:53222/api/github/authenticate";

        private const string GITHUB_APP_OAUTH_SCOPE = "user notifications repo";

        private class OAuthResponse
        {
            public string access_token, scope;
        }

        [HttpGet]
        public IHttpActionResult Authenticate(string code, string state)
        {
            Console.WriteLine("Received GitHub intall: '{0}', '{1}'", code, state);

            // @TODO: save the token and associate it with something
            OAuthResponse response = RequestAccessToken(code);
            Console.WriteLine("Received GitHub OAuth: '{0}', '{1}'", code, response.access_token);


            return Redirect(Url.Content("~/"));
        }

        private OAuthResponse RequestAccessToken(string code)
        {
            WebClient client = new WebClient();
            client.Headers["Accept"] = "application/json";

            string uri = GITHUB_APP_OAUTH_ACCESS_URL + 
                String.Format("?client_id={0}&client_secret={1}&code={2}&redirect_uri={3}&state={4}",
                GITHUB_APP_CLIENT_ID,
                GITHUB_APP_CLIENT_SECRET, 
                code,
                GITHUB_APP_OAUTH_REDIRECT_URL,
                "state");

            string response = client.DownloadString(new Uri(uri));

            try { return JsonConvert.DeserializeObject<OAuthResponse>(response); }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Failed to process GitHub OAuth Access Token response for code: '{0}', reason: {1}", code, ex.ToString()));
            }
        }

        public string GetOAuthURL()
        {
            return GITHUB_APP_OAUTH_URL +
                String.Format("?client_id={0}&redirect_uri={1}&scope={2}&state={3}&allow_signup={4}",
                GITHUB_APP_CLIENT_ID,
                GITHUB_APP_OAUTH_REDIRECT_URL,
                GITHUB_APP_OAUTH_SCOPE,
                "state",
                "true");
        }
    }
}