using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AreYouSleeping.Updater
{
    /*
     * 
     * [
  {
    "url": "https://api.github.com/repos/svetoslav-maksimov/AreYouSleeping/releases/102864826",
    "assets_url": "https://api.github.com/repos/svetoslav-maksimov/AreYouSleeping/releases/102864826/assets",
    "upload_url": "https://uploads.github.com/repos/svetoslav-maksimov/AreYouSleeping/releases/102864826/assets{?name,label}",
    "html_url": "https://github.com/svetoslav-maksimov/AreYouSleeping/releases/tag/0.1.0",
    "id": 102864826,
    "author": {
      "login": "svetoslav-maksimov",
      "id": 12965469,
      "node_id": "MDQ6VXNlcjEyOTY1NDY5",
      "avatar_url": "https://avatars.githubusercontent.com/u/12965469?v=4",
      "gravatar_id": "",
      "url": "https://api.github.com/users/svetoslav-maksimov",
      "html_url": "https://github.com/svetoslav-maksimov",
      "followers_url": "https://api.github.com/users/svetoslav-maksimov/followers",
      "following_url": "https://api.github.com/users/svetoslav-maksimov/following{/other_user}",
      "gists_url": "https://api.github.com/users/svetoslav-maksimov/gists{/gist_id}",
      "starred_url": "https://api.github.com/users/svetoslav-maksimov/starred{/owner}{/repo}",
      "subscriptions_url": "https://api.github.com/users/svetoslav-maksimov/subscriptions",
      "organizations_url": "https://api.github.com/users/svetoslav-maksimov/orgs",
      "repos_url": "https://api.github.com/users/svetoslav-maksimov/repos",
      "events_url": "https://api.github.com/users/svetoslav-maksimov/events{/privacy}",
      "received_events_url": "https://api.github.com/users/svetoslav-maksimov/received_events",
      "type": "User",
      "site_admin": false
    },
    "node_id": "RE_kwDOJfzGc84GIZe6",
    "tag_name": "0.1.0",
    "target_commitish": "main",
    "name": "0.1.0",
    "draft": false,
    "prerelease": true,
    "created_at": "2023-05-13T09:28:33Z",
    "published_at": "2023-05-13T09:33:15Z",
    "assets": [
      {
        "url": "https://api.github.com/repos/svetoslav-maksimov/AreYouSleeping/releases/assets/107989994",
        "id": 107989994,
        "node_id": "RA_kwDOJfzGc84Gb8vq",
        "name": "are-you-sleeping-0.1.0.zip",
        "label": null,
        "uploader": {
          "login": "svetoslav-maksimov",
          "id": 12965469,
          "node_id": "MDQ6VXNlcjEyOTY1NDY5",
          "avatar_url": "https://avatars.githubusercontent.com/u/12965469?v=4",
          "gravatar_id": "",
          "url": "https://api.github.com/users/svetoslav-maksimov",
          "html_url": "https://github.com/svetoslav-maksimov",
          "followers_url": "https://api.github.com/users/svetoslav-maksimov/followers",
          "following_url": "https://api.github.com/users/svetoslav-maksimov/following{/other_user}",
          "gists_url": "https://api.github.com/users/svetoslav-maksimov/gists{/gist_id}",
          "starred_url": "https://api.github.com/users/svetoslav-maksimov/starred{/owner}{/repo}",
          "subscriptions_url": "https://api.github.com/users/svetoslav-maksimov/subscriptions",
          "organizations_url": "https://api.github.com/users/svetoslav-maksimov/orgs",
          "repos_url": "https://api.github.com/users/svetoslav-maksimov/repos",
          "events_url": "https://api.github.com/users/svetoslav-maksimov/events{/privacy}",
          "received_events_url": "https://api.github.com/users/svetoslav-maksimov/received_events",
          "type": "User",
          "site_admin": false
        },
        "content_type": "application/x-zip-compressed",
        "state": "uploaded",
        "size": 66687299,
        "download_count": 0,
        "created_at": "2023-05-13T09:58:55Z",
        "updated_at": "2023-05-13T09:59:59Z",
        "browser_download_url": "https://github.com/svetoslav-maksimov/AreYouSleeping/releases/download/0.1.0/are-you-sleeping-0.1.0.zip"
      }
    ],
    "tarball_url": "https://api.github.com/repos/svetoslav-maksimov/AreYouSleeping/tarball/0.1.0",
    "zipball_url": "https://api.github.com/repos/svetoslav-maksimov/AreYouSleeping/zipball/0.1.0",
    "body": "Pre-release. Do not use."
  }
]
     *    
     */
    public record ReleaseDto
    {
        public string? Html_url { get; set; }
        public long? Id { get; set; }
        public string? Tag_name { get; set; }
        public DateTime? Published_at { get; set; }
        public List<ReleaseAssetDto>? Assets { get; set; }
    }

    public record ReleaseAssetDto
    {
        public string? Name { get; set; }
        public string? Content_type { get; set; }
        public string? Browser_download_url { get; set; }
    }
}
