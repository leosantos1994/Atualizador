using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Json;
using UpdaterService.Model;

namespace UpdaterService.Handler
{
    public class APIHandler
    {
        public static HttpClient? _Client;
        private static void InitClient(ConfigSettings config)
        {
            if (_Client == null)
            {
                _Client = new HttpClient();
                _Client.Timeout = new TimeSpan(0, 1, 16);
            }
        }

        public static MidModel.ServiceModel FindUpdateRequest(ConfigSettings config, out string errors)
        {
            errors = "";

            InitClient(config);
            HttpResponseMessage responseMessage = _Client.GetAsync($"{config.ApiURL}/update/GetService/{config.Clients}").Result;

            if (responseMessage.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                errors = "Nenhum conteudo localizado para atualização.";
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

        public static FileContentResult DownloadFiles(ConfigSettings config, MidModel.ServiceModel responseModel, out string error)
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

        private static bool CreateFile(ConfigSettings cfg, MidModel.ServiceModel reponseService, Stream file, out string error)
        {
            try
            {
                string servicePath = Path.Combine(cfg.ServiceWorkDir, Constants.Constants.ServiceFilesFolderName);
                error = "";

                ClearServiceWorkDirectory(servicePath);

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

        private static void ClearServiceWorkDirectory(string dir)
        {
            ResponseService.Add($"Limpando diretório de serviço {dir}");

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            else
            {
                foreach (var item in Directory.EnumerateFiles(dir))
                {
                    File.Delete(item);
                }
            }
        }

        public static MidModel.ServiceModel SendUpdateInformation(ResponseModel response, ConfigSettings config)
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

        public static void SendStatusInformation(ConfigSettings config, Guid serviceId, int progress)
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