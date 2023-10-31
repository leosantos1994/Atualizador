using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using UpdaterService.Interfaces;
using UpdaterService.Model;

namespace UpdaterService.Handler
{
    public class APIHandler
    {
        public static HttpClient? _Client;
        private static void InitClient(IConfigSettings config)
        {
            if (_Client == null)
            {
                _Client = new HttpClient();
                _Client.Timeout = new TimeSpan(0, 1, 16);
            }
        }

        public static MidModel.ServiceModel FindUpdateRequest(IConfigSettings config, out string errors)
        {
            errors = "";

            InitClient(config);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{config.ApiURL}/update/GetService/");
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("Accept", "*/*");
            string content = System.Text.Json.JsonSerializer.Serialize(config.Clients);
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");

            HttpResponseMessage responseMessage = _Client.SendAsync(request).Result;

            if (responseMessage.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                errors = "Nenhum conteúdo localizado para atualização.";
            }
            else if (responseMessage.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                errors = "Erro interno da API.";
            }
            else if (!responseMessage.IsSuccessStatusCode)
            {
                errors = "Exceção não tratada pela API.  Status code" + responseMessage.StatusCode;
            }

            if (!string.IsNullOrEmpty(errors))
                return null;

            var responseContent = JsonConvert.DeserializeObject<MidModel.ServiceModel>(responseMessage.Content.ReadAsStringAsync().Result);

            FileContentResult file = DownloadFiles(config, responseContent, out errors);

            if (!string.IsNullOrEmpty(errors))
                return null;

            if (!CreateFile(config, responseContent, new MemoryStream(file.FileContents), out errors))
                return null;

            return responseContent;
        }

        public static FileContentResult DownloadFiles(IConfigSettings config, MidModel.ServiceModel responseModel, out string error)
        {
            HttpResponseMessage response = null;
            try
            {
                error = "";

                var downloadClient = new HttpClient();

                downloadClient.Timeout = new TimeSpan(0, 5, 0);

                var bytes = downloadClient.GetByteArrayAsync($"{config.ApiURL}/update/download/{responseModel.VersionFileId}").Result;

                return new FileContentResult(bytes, "application/zip");
            }
            catch (Exception e)
            {
                error = "Erro ao buscar arquivo da atualização. " + e.Message + "\n " + response?.StatusCode.ToString() + response?.Content.ReadAsStringAsync().Result;
            }
            return null;
        }

        private static bool CreateFile(IConfigSettings cfg, MidModel.ServiceModel reponseService, Stream file, out string error)
        {
            try
            {
                string servicePath = Path.Combine(cfg.ServiceWorkDir, Constants.Constants.ServiceFilesFolderName);

                if (!Directory.Exists(servicePath))
                    new DirectoryInfo(servicePath).Create();

                error = "";

                string filePath = Path.Combine(servicePath, Constants.Constants.FileName);

                reponseService.PatchFilesPath = filePath;

                using (var fileStream = File.Create(filePath))
                {
                    file.Seek(0, SeekOrigin.Begin);
                    file.CopyTo(fileStream);
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.ToString();
                return false;
            }
        }


        public static MidModel.ServiceModel SendUpdateInformation(ResponseModel response, IConfigSettings config)
        {
            try
            {
                Task<HttpResponseMessage> message = _Client.PostAsJsonAsync($"{config.ApiURL}/update/patch/{config.Clients}", response);
                message.Result.EnsureSuccessStatusCode();
                return JsonConvert.DeserializeObject<MidModel.ServiceModel>(message.Result.Content.ReadAsStringAsync().Result);
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao enviar novas informações.", e);
            }
        }

        public static void SendStatusInformation(IConfigSettings config, Guid serviceId, int progress)
        {
            try
            {
                HttpRequestMessage requestMessage = new HttpRequestMessage();
                requestMessage.Method = new HttpMethod("POST");
                requestMessage.RequestUri = new Uri($"{config.ApiURL}/update/putstatus/{serviceId}/{progress}");
                Task<HttpResponseMessage> message = _Client.SendAsync(requestMessage);
                message.Result.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao enviar status de atualização.", e);
            }
        }
    }
}