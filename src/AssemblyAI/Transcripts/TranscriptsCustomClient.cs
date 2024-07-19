#nullable enable

namespace AssemblyAI;

public partial class TranscriptsClient
{
    public Task<Transcript> SubmitAsync(FileInfo audioFile) => SubmitAsync(audioFile, new TranscriptOptionalParams());

    public async Task<Transcript> SubmitAsync(FileInfo audioFile, TranscriptOptionalParams transcriptParams)
    {
        using var audioFileStream = audioFile.OpenRead();
        return await SubmitAsync(audioFileStream, transcriptParams).ConfigureAwait(false);
    }

    public Task<Transcript> SubmitAsync(Stream audioFileStream) =>
        SubmitAsync(audioFileStream, new TranscriptOptionalParams());

    public async Task<Transcript> SubmitAsync(Stream audioFileStream, TranscriptOptionalParams transcriptParams)
    {
        var fileUpload = await _assemblyAIClient.Files.UploadAsync(audioFileStream).ConfigureAwait(false);
        return await SubmitAsync(new Uri(fileUpload.UploadUrl), transcriptParams).ConfigureAwait(false);
    }

    public Task<Transcript> SubmitAsync(Uri audioFileUrl) => SubmitAsync(audioFileUrl, new TranscriptOptionalParams());

    public async Task<Transcript> SubmitAsync(Uri audioFileUrl, TranscriptOptionalParams transcriptParams)
    {
        return await SubmitAsync(CreateParams(audioFileUrl, transcriptParams)).ConfigureAwait(false);
    }

    public Task<Transcript> TranscribeAsync(FileInfo audioFile) =>
        TranscribeAsync(audioFile, new TranscriptOptionalParams());

    public async Task<Transcript> TranscribeAsync(FileInfo audioFile, TranscriptOptionalParams transcriptParams)
    {
        using var audioFileStream = audioFile.OpenRead();
        return await TranscribeAsync(audioFileStream, transcriptParams).ConfigureAwait(false);
    }

    public Task<Transcript> TranscribeAsync(Stream audioFileStream) =>
        TranscribeAsync(audioFileStream, new TranscriptOptionalParams());

    public async Task<Transcript> TranscribeAsync(Stream audioFileStream, TranscriptOptionalParams transcriptParams)
    {
        var fileUpload = await _assemblyAIClient.Files.UploadAsync(audioFileStream).ConfigureAwait(false);
        return await TranscribeAsync(new Uri(fileUpload.UploadUrl), transcriptParams).ConfigureAwait(false);
    }

    public Task<Transcript> TranscribeAsync(Uri audioFileUrl) =>
        TranscribeAsync(audioFileUrl, new TranscriptOptionalParams());

    public async Task<Transcript> TranscribeAsync(Uri audioFileUrl, TranscriptOptionalParams transcriptParams)
    {
        var transcript = await SubmitAsync(CreateParams(audioFileUrl, transcriptParams)).ConfigureAwait(false);
        transcript = await WaitUntilReady(transcript.Id).ConfigureAwait(false);
        return transcript;
    }

    public async Task<Transcript> WaitUntilReady(string id)
    {
        var transcript = await GetAsync(id).ConfigureAwait(false);
        while (transcript.Status != TranscriptStatus.Completed && transcript.Status != TranscriptStatus.Error)
        {
            await Task.Delay(1000).ConfigureAwait(false);
            transcript = await GetAsync(transcript.Id).ConfigureAwait(false);
        }

        return transcript;
    }

    private TranscriptParams CreateParams(Uri audioFileUrl, TranscriptOptionalParams transcriptParams)
    {
        return new TranscriptParams
        {
            AudioUrl = audioFileUrl.ToString()
            // TODO: map other parameters
        };
    }

    public Task<TranscriptList> ListAsync() => ListAsync(new ListTranscriptParams());

    /// <summary>
    /// Retrieve a list of transcripts you created.
    /// Transcripts are sorted from newest to oldest. The previous URL always points to a page with older transcripts.
    /// </summary>
    public async Task<TranscriptList> ListAsync(string listUrl)
    {
        // this would be easier to just call the given URL,
        // but the raw client doesn't let us make requests to full URL
        // so we'll parse the querystring and pass it to `ListAsync`.
        
        var queryString = listUrl.Substring(listUrl.IndexOf('?') + 1)
            .Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(k => k.Split('='))
            .Where(k => k.Length == 2)
            .ToLookup(a => a[0], a => Uri.UnescapeDataString(a[1])
                , StringComparer.OrdinalIgnoreCase);
        var listTranscriptParams = new ListTranscriptParams();
        if (queryString.Contains("limit"))
        {
            listTranscriptParams.Limit = int.Parse(queryString["limit"].First());
        }

        if (queryString.Contains("status"))
        {
            listTranscriptParams.Status =
                (TranscriptStatus)Enum.Parse(typeof(TranscriptStatus), queryString["limit"].First());
        }

        if (queryString.Contains("created_on"))
        {
            listTranscriptParams.CreatedOn = queryString["created_on"].First();
        }

        if (queryString.Contains("before_id"))
        {
            listTranscriptParams.BeforeId = queryString["before_id"].First();
        }
        
        if (queryString.Contains("after_id"))
        {
            listTranscriptParams.AfterId = queryString["after_id"].First();
        }
        
        if (queryString.Contains("throttled_only"))
        {
            listTranscriptParams.ThrottledOnly = bool.Parse(queryString["throttled_only"].First());
        }
        
        return await ListAsync(listTranscriptParams).ConfigureAwait(false);
    }
}