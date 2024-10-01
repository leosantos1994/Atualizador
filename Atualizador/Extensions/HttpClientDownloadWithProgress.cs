using Serilog;

namespace System.Net.Http
{
    public static class HttpClientDownloadWithProgress
    {
        public delegate void ProgressChangedHandler(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage);

        public static event ProgressChangedHandler ProgressChanged;

        public static async Task DownloadAndSaveFile(this HttpClient httpClient, string downloadloadUrl, string destinationFilePath, CancellationToken cancellationToken)
        {
            ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
            {
                Log.Information($"Executando download =>  {progressPercentage}% ({totalBytesDownloaded}/{totalFileSize})");
            };

            using (HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(downloadloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                await DownloadFileFromHttpResponseMessage(httpResponseMessage, destinationFilePath, cancellationToken);
            }
        }

        private static async Task DownloadFileFromHttpResponseMessage(HttpResponseMessage response, string destinationFilePath, CancellationToken cancellationToken)
        {
            if (!response.IsSuccessStatusCode)
                throw new Exception("File dowload error. " + response.Content.ReadAsStringAsync().Result);

            response.EnsureSuccessStatusCode();

            long? totalBytes = response.Content.Headers.ContentLength;

            using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                await ProcessContentStream(totalBytes, contentStream, destinationFilePath, cancellationToken);
        }

        private static async Task ProcessContentStream(long? totalDownloadSize, Stream contentStream, string destinationFilePath, CancellationToken cancellationToken)
        {
            long totalBytesRead = 0L;
            long readCount = 0L;
            byte[] buffer = new byte[8192];
            bool isMoreToRead = true;

            using (var fileStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
            {
                do
                {
                    int bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead == 0)
                    {
                        isMoreToRead = false;
                        TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                        continue;
                    }

                    await fileStream.WriteAsync(buffer, 0, bytesRead);

                    totalBytesRead += bytesRead;
                    readCount += 1;

                    TriggerProgressChanged(totalDownloadSize, totalBytesRead);
                }
                while (isMoreToRead);
            }
        }

        private static void TriggerProgressChanged(long? totalDownloadSize, long totalBytesRead)
        {
            if (ProgressChanged == null)
                return;

            double? progressPercentage = null;
            if (totalDownloadSize.HasValue)
                progressPercentage = Math.Round((double)totalBytesRead / totalDownloadSize.Value * 100, 2);

            ProgressChanged(totalDownloadSize, totalBytesRead, progressPercentage);
        }
    }
}