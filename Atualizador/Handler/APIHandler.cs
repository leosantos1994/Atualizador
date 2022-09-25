using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using UpdaterService.Model;

namespace UpdaterService.Handler
{
    public class APIHandler
    {
        public static MidModel.ServiceModel FindUpdateRequest(ConfigSettings config)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string url = $"{config.ApiURL}/update/get/{config.Client}";
                    client.Timeout = new TimeSpan(0, 0, 16);
                    Task<HttpResponseMessage> message = client.GetAsync(client.BaseAddress);
                    message.Result.EnsureSuccessStatusCode();
                    MidModel.ServiceModel response = JsonSerializer.Deserialize<MidModel.ServiceModel>(message.Result.Content.ReadAsStringAsync().Result);


                    Stream file = DownloadFiles(config, response).Result;

                    if (!CreateFile(config, response, file, out string error))
                        throw new Exception(error);

                    return response;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao buscar novas atualizações.", e);
            }
        }

        public static Task<Stream> DownloadFiles(ConfigSettings config, MidModel.ServiceModel responseModel)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string url = $"{config.ApiURL}/update/download?id={responseModel.Id}";
                    client.Timeout = new TimeSpan(0, 0, 16);
                    Task<HttpResponseMessage> message = client.GetAsync(client.BaseAddress);
                    message.Result.EnsureSuccessStatusCode();

                    var responseFile = message.Result.Content.ReadAsStreamAsync();

                    return responseFile;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao buscar novas atualizações.", e);
            }
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
            Directory.Delete(dir, true);
        }

        public static MidModel.ServiceModel SendUpdateInformation(ResponseModel response, ConfigSettings config)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string url = $"{config.ApiURL}/update/patch/{config.Client}";

                    client.Timeout = new TimeSpan(0, 0, 16);
                    Task<HttpResponseMessage> message = client.PostAsJsonAsync(url, response);
                    message.Result.EnsureSuccessStatusCode();
                    return JsonSerializer.Deserialize<MidModel.ServiceModel>(message.Result.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao enviar novas informações.", e);
            }
        }
    }
}