using System;

namespace StudioCore.Application;

public class GithubAsset
{
    public string url { get; set; }
    public int id { get; set; }
    public string node_id { get; set; }
    public string name { get; set; }
    public object label { get; set; }
    public GithubUploader uploader { get; set; }
    public string content_type { get; set; }
    public string state { get; set; }
    public int size { get; set; }
    public string digest { get; set; }
    public int download_count { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public string browser_download_url { get; set; }
}

public class GithubUploader
{
    public string login { get; set; }
    public int id { get; set; }
    public string node_id { get; set; }
    public string avatar_url { get; set; }
    public string gravatar_id { get; set; }
    public string url { get; set; }
    public string html_url { get; set; }
    public string followers_url { get; set; }
    public string following_url { get; set; }
    public string gists_url { get; set; }
    public string starred_url { get; set; }
    public string subscriptions_url { get; set; }
    public string organizations_url { get; set; }
    public string repos_url { get; set; }
    public string events_url { get; set; }
    public string received_events_url { get; set; }
    public string type { get; set; }
    public string user_view_type { get; set; }
    public bool site_admin { get; set; }
}
