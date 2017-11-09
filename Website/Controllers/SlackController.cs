﻿using Newtonsoft.Json;
using System;
using System.Net;
using System.Web.Http;

namespace Website.Controllers
{
    public class SlackController : ApiController
    {
        private const string SLACK_APP_CLIENT_ID = "";
        private const string SLACK_APP_CLIENT_SECRET = "";

        private const string SLACK_APP_OAUTH_URL = "https://slack.com/oauth/authorize";
        private const string SLACK_APP_OAUTH_ACCESS_URL = "https://slack.com/api/oauth.access";
        private const string SLACK_APP_OAUTH_REDIRECT_URL = "http://localhost:53222/api/slack/authenticate";

        private const string SLACK_APP_OAUTH_SCOPE = "client";

        public class SlashCommand
        {
            public string token { get; set; }
            public string team_id { get; set; }
            public string team_domain { get; set; }
            public string enterprise_id { get; set; }
            public string enterprise_name { get; set; }
            public string channel_id { get; set; }
            public string channel_name { get; set; }
            public string user_id { get; set; }
            public string user_name { get; set; }
            public string command { get; set; }
            public string text { get; set; }
            public string response_url { get; set; }
            public string trigger_id { get; set; }
        }

        private class OAuthResponse
        {
            public string access_token, scope;
        }

        public IHttpActionResult ProcessMessage([FromBody] SlashCommand command)
        {
            // @TODO: verify message is actually from slack via verification token

            // @TODO: process slash command

            // @TODO: proper error code based on command execution
            return Ok();
        }

        [HttpGet]
        public IHttpActionResult Authenticate(string code, string state)
        {
            Console.WriteLine("Received Slack install: '{0}' '{1}'", code, state);

            // @TODO: save the token and associate it with something
            OAuthResponse response = RequestAccessToken(code);
            Console.WriteLine("Received Slack OAuth: '{0}', '{1}'", code, response.access_token);

            // @TODO: change this once we have an idea where we want to redirect users after installing the app
            // perhaps a tutorial page showing how to use the commands/app?
            return Redirect(Url.Content("~/"));
        }

        private OAuthResponse RequestAccessToken(string code)
        {
            WebClient client = new WebClient();
            client.Headers["Accept"] = "application/json";

            string uri = SLACK_APP_OAUTH_ACCESS_URL + String.Format("?client_id={0}&client_secret={1}&code={2}&redirect_uri={3}", SLACK_APP_CLIENT_ID, SLACK_APP_CLIENT_SECRET, code, SLACK_APP_OAUTH_REDIRECT_URL);
            string response = client.DownloadString(new Uri(uri));

            try { return JsonConvert.DeserializeObject<OAuthResponse>(response); }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Failed to process Slack OAuth Access Token response for code: '{0}', reason: {1}", code, ex.ToString()));
            }
        }

        public string GetOAuthURL()
        {
            return SLACK_APP_OAUTH_URL +
                String.Format("?client_id={0}&redirect_uri={1}&scope={2}&state={3}",
                SLACK_APP_CLIENT_ID,
                SLACK_APP_OAUTH_REDIRECT_URL,
                SLACK_APP_OAUTH_SCOPE,
                "state");
        }
    }
}