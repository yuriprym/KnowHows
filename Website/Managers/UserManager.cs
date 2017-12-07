﻿using Octokit;
using System;
using System.Collections.Generic;

namespace Website.Manager
{
    public class GitHubUser
    {
        public string TeamID, ChannelID, UserID;

        private GitHubClient _client;
        private string _gitHubAccessToken;
        private List<String> _repositories;
        private List<String> _trackedRepositories;

        public IReadOnlyCollection<String> Repositories
        {
            get { return _repositories.AsReadOnly(); }
        }

        public IReadOnlyCollection<String> TrackedRepositories
        {
            get { return _trackedRepositories.AsReadOnly(); }
        }

        public string GitHubAccessToken
        {
            private get { return _gitHubAccessToken; }
            set
            {
                _gitHubAccessToken = value;
                _client.Credentials = new Credentials(_gitHubAccessToken);
                UpdateRepositoryIndex();
            }
        }

        public GitHubUser(string teamId, string channelId, string userId)
        {
            this.TeamID = teamId;
            this.ChannelID = channelId;
            this.UserID = userId;
            this._repositories = new List<String>();
            this._client = new GitHubClient(new ProductHeaderValue("")); // does this need to have a non-empty string?
        }

        private void UpdateRepositoryIndex()
        {
            _repositories.Clear();
            _trackedRepositories.Clear();
            var repos = _client.Repository.GetAllForCurrent().Result;
            foreach (var repo in repos) _repositories.Add(repo.Url);
        }

        public bool TrackRepository(string repositoryName)
        {
            if (_repositories.Contains(repositoryName) && !_trackedRepositories.Contains(repositoryName)) {
                _trackedRepositories.Add(repositoryName);
                return true;
            }
            return false;
        }

        public bool UntrackRepository(string repositoryName)
        {
            // @TODO
            return false;
        }
    }

    public class UserManager
    {
        public static UserManager Instance = new UserManager();
  
        private HashSet<String> pendingUsers = new HashSet<string>();
        private Dictionary<String, GitHubUser> githubUsers = new Dictionary<string, GitHubUser>();

        private UserManager() { }

        public bool IsGitHubAuthenticated(string uuid)
        {
            return githubUsers.ContainsKey(uuid);
        }

        public void AddPendingGitHubAuth(string uuid)
        {
            if (!githubUsers.ContainsKey(uuid)) pendingUsers.Add(uuid);
        }

        public void AddGitHubAuth(string uuid, string token)
        {
            if (pendingUsers.Contains(uuid)) {
                githubUsers[uuid] = new GitHubUser(GetTeamIDFromUUID(uuid), GetChannelIDFromUUID(uuid), GetUserIDFromUUID(uuid));
                githubUsers[uuid].GitHubAccessToken = token;
                pendingUsers.Remove(uuid);
            }
            else throw new Exception("Tried to add successful github auth for user with no pending state: " + uuid);
        }

        public GitHubUser GetGitHubUser(string uuid)
        {
            return githubUsers[uuid];
        }

        private static String GetTeamIDFromUUID(string uuid) { return uuid.Split(".")[0]; }
        private static String GetChannelIDFromUUID(string uuid) { return uuid.Split(".")[1]; }
        private static String GetUserIDFromUUID(string uuid) { return uuid.Split(".")[2]; }
        private static String GetTeamBasedIDFromUUID(string uuid) { return GetTeamIDFromUUID(uuid) + ".users." + GetUserIDFromUUID(uuid); }
    }
}
