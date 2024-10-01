using MidModel;
using Newtonsoft.Json;
using Serilog;
using System.Net.Http.Json;
using System.Text;
using UpdaterService.Interfaces;
using UpdaterService.Model;

namespace UpdaterService.Handler
{
    public class APIHandler
    {
        public static HttpClient? _httpClient;
        private static void InitClient(IConfigSettings config)
        {
            if (_httpClient == null)
            {
                _httpClient = new HttpClient();
                _httpClient.Timeout = new TimeSpan(0, 1, 16);
            }
        }

        public static async Task<ServiceModel> FindUpdateRequest(IConfigSettings config)
        {
            string errors = "";

            InitClient(config);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{config.ApiURL}/update/GetService/");
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("Accept", "*/*");
            string content = System.Text.Json.JsonSerializer.Serialize(config.Clients);
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");

            Log.Information($"Executando requisição para a api: {config.ApiURL} com os dados de cliente: {config.Clients}");

            HttpResponseMessage responseMessage = _httpClient.SendAsync(request).Result;

            if (responseMessage.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                errors = "Nenhum conteúdo localizado para atualização.";
            }
            else if (responseMessage.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                errors = "Erro interno da API.";
            }
            else if (responseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                errors = "Endpoint da API não localizado ou a API não estava disponível.";
            }
            else if (!responseMessage.IsSuccessStatusCode)
            {
                errors = "Exceção não tratada pela API.  Status code" + responseMessage.StatusCode;
            }

            CheckDownloadError(errors);

            ServiceModel responseContent = JsonConvert.DeserializeObject<ServiceModel>(responseMessage.Content.ReadAsStringAsync().Result);

            errors = await DownloadAndSaveFiles(config, responseContent);

            CheckDownloadError(errors);

            return responseContent;
        }

        private static void CheckDownloadError(string error)
        {
            if (!string.IsNullOrEmpty(error))
                throw new Exception(error);
        }

        public static async Task<string> DownloadAndSaveFiles(IConfigSettings config, ServiceModel responseModel)
        {
            CancellationTokenSource cts = new(600000); //10 minutos
            CancellationToken cancellationToken = cts.Token;
            try
            {
                Log.Information($"Atualização para o cliente localizada, buscando arquivos.");

                GetFilePath(config, responseModel, out string fileCreationError);

                if (!string.IsNullOrEmpty(fileCreationError))
                {
                    return fileCreationError;
                }

                await _httpClient.DownloadAndSaveFile($"{config.ApiURL}/update/download/{responseModel.VersionFileId}", responseModel.PatchFilesPath, cancellationToken);
            }
            catch (OperationCanceledException e)
            {
                return "Verifique sua conexão, tente conectar em um cabo de rede para melhor conectividade ou procure uma rede mais rápida para executar o processo e tente novamente.";
            }
            catch (Exception e)
            {
                return "Erro ao executar operação " + e.Message;
            }

            return "";
        }

        private static void GetFilePath(IConfigSettings cfg, ServiceModel reponseService, out string error)
        {
            try
            {
                Log.Information($"Criando pastas de atualização no ambiente de trabalho.");

                string servicePath = Path.Combine(cfg.ServiceWorkDir, Constants.Constants.ServiceFilesFolderName);

                if (!Directory.Exists(servicePath))
                    new DirectoryInfo(servicePath).Create();

                error = "";

                string fileFullPath = Path.Combine(servicePath, Constants.Constants.FileName);

                reponseService.PatchFilesPath = fileFullPath;
            }
            catch (Exception ex)
            {
                error = ex.ToString();
            }
        }


        public static MidModel.ServiceModel SendUpdateInformation(ResponseModel response, IConfigSettings config)
        {
            try
            {
                Task<HttpResponseMessage> message = _httpClient.PostAsJsonAsync($"{config.ApiURL}/update/patch/{config.Clients}", response);
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
                Task<HttpResponseMessage> message = _httpClient.SendAsync(requestMessage);
                message.Result.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao enviar status de atualização.", e);
            }
        }
    }
}