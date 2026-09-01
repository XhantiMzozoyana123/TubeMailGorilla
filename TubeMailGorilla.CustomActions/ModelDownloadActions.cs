using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using WixToolset.Dtf.WindowsInstaller;

namespace TubeMailGorilla.CustomActions
{
    public class ModelDownloadActions
    {
        private const string ModelFileName = "Llama-3.2-3B-Instruct-Q4_K_M.gguf";
        private const string DefaultModelUrl =
            "https://huggingface.co/bartowski/Llama-3.2-3B-Instruct-GGUF/resolve/main/Llama-3.2-3B-Instruct-Q4_K_M.gguf";

        [CustomAction]
        public static ActionResult DownloadLlamaModel(Session session)
        {
            session.Log("Begin DownloadLlamaModel");

            try
            {
                string installFolder = session.CustomActionData.ContainsKey("INSTALLFOLDER")
                    ? session.CustomActionData["INSTALLFOLDER"]
                    : null;

                if (string.IsNullOrEmpty(installFolder))
                {
                    session.Log("INSTALLFOLDER not set - skipping model download.");
                    return ActionResult.Success;
                }

                string modelUrl = session.CustomActionData.ContainsKey("MODELDOWNLOADURL")
                    && !string.IsNullOrEmpty(session.CustomActionData["MODELDOWNLOADURL"])
                    ? session.CustomActionData["MODELDOWNLOADURL"]
                    : DefaultModelUrl;

                string targetPath = Path.Combine(installFolder, ModelFileName);

                if (File.Exists(targetPath))
                {
                    var existing = new FileInfo(targetPath);
                    if (existing.Length > 1024 * 1024 * 1024)
                    {
                        session.Log($"Model already present at {targetPath} ({existing.Length} bytes) - skipping download.");
                        return ActionResult.Success;
                    }
                    session.Log("Incomplete model file found - redownloading.");
                    File.Delete(targetPath);
                }

                session.Message(InstallMessage.ActionStart, new Record("DownloadingLlamaModel", "Downloading AI model (approx. 1.9 GB). This may take several minutes...", "Please wait"));

                int reportPercent = 0;
                var result = DownloadModelAsync(session, modelUrl, targetPath, pct =>
                {
                    var whole = (int)pct;
                    if (whole >= reportPercent + 5)
                    {
                        reportPercent = whole;
                        session.Message(InstallMessage.ActionData, new Record("DownloadingLlamaModel", $"Downloading AI model... {whole}% complete"));
                        session.Log($"Model download progress: {whole}%");
                    }
                }).GetAwaiter().GetResult();

                if (!result)
                {
                    session.Log("Model download failed.");
                    try { if (File.Exists(targetPath)) File.Delete(targetPath); } catch { }
                    return ActionResult.Success; // don't fail the whole install
                }

                var downloaded = new FileInfo(targetPath);
                session.Log($"Model downloaded to {targetPath} ({downloaded.Length} bytes).");
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log($"DownloadLlamaModel exception: {ex}");
                return ActionResult.Success; // install succeeds even if download fails
            }
        }

        private static async Task<bool> DownloadModelAsync(Session session, string url, string targetPath, Action<double> reportProgress)
        {
            try
            {
                using (var handler = new HttpClientHandler())
                using (var client = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(60) })
                {
                    using (var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        long? total = response.Content.Headers.ContentLength;
                        reportProgress(0);

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 16, useAsync: true))
                        {
                            var buffer = new byte[1 << 16];
                            long read = 0;
                            int n;
                            while ((n = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fs.WriteAsync(buffer, 0, n);
                                read += n;
                                if (total.HasValue && total.Value > 0)
                                    reportProgress(read * 100.0 / total.Value);
                            }
                            await fs.FlushAsync();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                session.Log($"Model download error: {ex.Message}");
                return false;
            }
        }
    }
}