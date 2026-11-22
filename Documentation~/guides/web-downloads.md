# Web downloads

The Core package lets your editor tools download files from the web: images to display in a custom window, data fetched from an API, files your tool caches on the user's machine... It works for any editor code, whether it's in a package or in your project.

You describe each file with a request (where to download it, how to check if it's already there, and what to do with the downloaded content), and pass it to a queue. The queue takes care of the rest:

- It doesn't download a file that is already available, or already being downloaded.
- It retries the failed downloads, and waits for the network to come back when it's lost.
- It limits the number of downloads in progress, per queue and for all the queues.
- It displays the downloads in the *Background Tasks* window (from Unity 2020.1).
- It aborts the downloads before a domain reload, so scripts can reload safely.

> This feature is only available in the editor (in the `SideXP.Core.EditorOnly` namespace). To download game content at runtime, prefer a tool made for it, as [Addressables](https://docs.unity3d.com/Packages/com.unity.addressables@latest) with [Cloud Content Delivery](https://unity.com/products/cloud-content-delivery): they handle versioning, integrity checks and caching for shipped games.

## Downloading a file

As an example, let's make a tool that displays thumbnails downloaded from a server. Create an `EditorDownloadQueue` in a static field, and request a `FileDownloadRequest` to download a thumbnail and store it on the user's machine:

```cs
using UnityEditorInternal;
using SideXP.Core.EditorOnly;

public static class Thumbnails
{
    private static readonly EditorDownloadQueue s_downloads = new EditorDownloadQueue("Download thumbnails");

    public static string GetPath(string id)
    {
        return $"{PathEditorUtility.UserCachePath}/My Company/My Tool/{id}.png";
    }

    public static void Request(string id)
    {
        var request = new FileDownloadRequest($"https://example.com/thumbnails/{id}.png", GetPath(id));
        s_downloads.Request(request, r =>
        {
            if (r.Status == EDownloadRequestStatus.Succeeded)
                InternalEditorUtility.RepaintAllViews();
        });
    }
}
```

The thumbnail is downloaded only if its file doesn't exist yet. The callback is called once the file is stored, or once the download has been given up. If the file already exists, it's called right away.

`PathEditorUtility.UserCachePath` is the user's cache folder, shared by every project and Unity version on the machine (`%LOCALAPPDATA%` on Windows, `~/Library/Caches` on macOS, `~/.cache` on Linux). Store your files in a subfolder named after your company or tool. You can also store them anywhere else, as in the project's `Library` folder for files that only make sense for one project.

> The name of a queue is displayed in the *Background Tasks* window. It also identifies the downloads it gave up, which are remembered until the editor is closed, so give each queue a unique name.

## Downloading data in memory

To use the downloaded content right away instead of storing it (as the response of an API), request a `MemoryDownloadRequest`:

```cs
s_downloads.Request(new MemoryDownloadRequest("https://example.com/api/news.json"), r =>
{
    if (r.Status == EDownloadRequestStatus.Succeeded)
        News = JsonUtility.FromJson<NewsList>(((MemoryDownloadRequest)r).GetText());
});
```

The content is kept in the request: requesting the same request again completes right away. Create a new request to download the file again (as to refresh the data).

## Following a request

Every request has a `Key` that identifies the file (the path of the stored file for a `FileDownloadRequest`, or the URL for a `MemoryDownloadRequest`). Requesting a key that is already in the queue doesn't download it again: `Request()` returns the request already in the queue, and its callback is also called once it's done.

Use the returned request to follow the download, as to display a progress bar:

| Property | Description |
|---|---|
| `Status` | `None` (not requested yet), `Waiting`, `Downloading`, `Succeeded` or `Failed`. |
| `IsDone` | Whether the request succeeded or failed. |
| `Progress` | The progress of the current download, between 0 and 1. |
| `Error` | The reason why the last attempt failed. Set while the download waits to be retried, and once it's given up. |

The queue also gives `Count` (the number of requests waiting or downloading), `IsPending(key)`, `IsGivenUp(key)`, and the `RequestCompleted` event, called for every request that was in the queue once it's done.

## Built-in requests

| Request | Key | Available when | Handling |
|---|---|---|---|
| `FileDownloadRequest(url, filePath)` | The file path | The file exists. | Writes the downloaded file. Override `Process()` to change its content before it's written. |
| `MemoryDownloadRequest(url)` | The URL | The file has been downloaded by this request. | Keeps the downloaded content in `Data`, also available as text with `GetText()`. |
| `DelegateDownloadRequest(key, url, handle, isAvailable)` | The given key | The `isAvailable` function returns true. | Calls the `handle` function. |

Files are written with `FileEditorUtility.WriteFileAtomic()`: the content is written to a temporary file, then moved into place. So several editors can download the same file at the same time, and a file is never partially written.

## Custom requests

For anything else, inherit from `DownloadRequest`. For example, to convert each downloaded thumbnail before storing it:

```cs
using System.IO;
using SideXP.Core.EditorOnly;

public class ThumbnailRequest : DownloadRequest
{
    public ThumbnailRequest(string id)
    {
        Id = id;
    }

    public string Id { get; }

    public override string Key => Id;

    public override string Url => $"https://example.com/thumbnails/{Id}.png";

    public override string DisplayName => $"Thumbnail {Id}";

    public override bool IsAvailable()
    {
        return File.Exists(Thumbnails.GetPath(Id));
    }

    protected override EDownloadHandlingResult Handle(byte[] data, out string error)
    {
        if (!TryConvert(data, out byte[] converted))
        {
            // The server would send the same file again, so don't retry
            error = "The thumbnail is not a valid image.";
            return EDownloadHandlingResult.GiveUp;
        }

        try
        {
            FileEditorUtility.WriteFileAtomic(Thumbnails.GetPath(Id), converted, false);
            error = null;
            return EDownloadHandlingResult.Success;
        }
        catch (IOException exception)
        {
            // The file may be locked, the next attempt may work
            error = exception.Message;
            return EDownloadHandlingResult.Retry;
        }
    }
}
```

`Handle()` returns one of these outcomes:

| Outcome | Effect |
|---|---|
| `Success` | The request succeeds. |
| `Retry` | The download is retried later, as a failed download. |
| `GiveUp` | The request fails right away, and the key won't be downloaded again by this queue. |

`IsAvailable()` is checked when the file is requested, and again before each download, so a file made available in the meantime (as by another editor) is not downloaded. Exceptions thrown by `IsAvailable()` or `Handle()` are logged: they count as "not available" and as `Retry`.

> `Handle()` is called on the main thread, from editor updates, so keep it short. The downloaded content is kept in memory until it's handled, so this feature is meant for small and medium files, not for files of hundreds of megabytes.

## Failures and retries

When a download fails, the queue decides what to do from the reason of the failure:

- **Missing file (HTTP 404)**: the download is given up right away.
- **No connection, proxy error or timeout**: every download of the queue is paused, since the others would fail the same way. The pause doubles after each consecutive network error.
- **Other errors**: the download is retried after a delay that doubles after each failure. After too many failures, it's given up.

Given up downloads are remembered until the editor is closed, so they're not attempted again after each domain reload. Call `ClearFailures()` to attempt them again (as after the user fixed a proxy setting).

## Settings

Pass `EditorDownloadQueueSettings` to the queue to change how it runs. Every value has a default, so only set the ones to change:

```cs
var settings = new EditorDownloadQueueSettings
{
    MaxConcurrentDownloads = 2,
    RetrySettings = new DownloadRetrySettings { MaxAttempts = 3 },
};
var queue = new EditorDownloadQueue("Download thumbnails", settings);
```

| Setting | Default | Description |
|---|---|---|
| `MaxConcurrentDownloads` | `4` | The maximum number of downloads of this queue in progress at the same time. |
| `RetrySettings` | Default retry settings | Defines when failed downloads are attempted again (see below). |
| `ShouldReportProgress` | `true` | Whether the downloads are displayed in the *Background Tasks* window. |
| `ShouldRememberGivenUp` | `true` | Whether the given up downloads are remembered until the editor is closed. Keys must not contain line breaks. |

| Retry setting | Default | Description |
|---|---|---|
| `MaxAttempts` | `5` | The maximum number of attempts of a download, network errors not included. |
| `FirstRetryDelay` | `2` | The delay before the first retry, in seconds. |
| `FirstNetworkPause` | `10` | The pause after a first network error, in seconds. |
| `MaxNetworkPause` | `300` | The maximum pause after consecutive network errors, in seconds. |

All the queues also share `EditorDownloadQueue.MaxTotalConcurrentDownloads` (`20` by default). Free download slots are given to the queues in turn, so a queue with many requests doesn't hold back the others. By default, a request times out after `WebDownloader.DefaultTimeoutSeconds` (30 seconds): override `TimeoutSeconds` in your request class for large files.

## Lower-level tools

The queue is built on smaller tools that you can use on their own:

- `WebDownloader.Start(url, timeoutSeconds)` starts a single download, polled from your own code with `IsDone`, `Progress` and `Result`.
- `DownloadRetryPolicy` decides when failed downloads can be attempted again, from the outcome of each one.
- `FileEditorUtility` writes files atomically, and deletes files and empty folders without throwing.
- `EditorProgressTask` displays a task in the *Background Tasks* window, and does nothing on Unity versions without it.

> The Core package itself uses these tools to download its [editor icons](editor-icons.md) on demand.
