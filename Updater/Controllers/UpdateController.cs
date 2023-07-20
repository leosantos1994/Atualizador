using Microsoft.AspNetCore.Mvc;
using MidModel;
using Updater.Repository;
using Updater.Repository.Interfaces;

namespace Updater.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UpdateController : ControllerBase
    {
        private IVersionFileRepository _FileRepository;
        private IVersionRepository _VersionRepository;
        private IServiceRepository _ServiceRepository;

        public UpdateController(AppDBContext context)
        {
            this._VersionRepository = new VersionRepository(context);
            this._FileRepository = new VersionFileRepository(context);
            this._ServiceRepository = new ServiceRepository(context);
        }

        [HttpGet("GetService/{client}")]
        public async Task<ActionResult> Get(string client)
        {
            try
            {
                string[] clients = client.Split(';');
                ServiceModel service = null;
                foreach (var clientName in clients)
                {
                    service = _ServiceRepository.Get(x => x.ClientName.ToLower() == clientName.ToLower() && x.ScheduleProgress == ScheduleProgress.Waiting && x.ScheduledDate <= DateTime.Now);

                    if (service != null)
                        break;
                }

                return service is null ? NoContent() : Ok(service);
            }
            catch (Exception e)
            {
                return Problem(title: "Erro ao processar solicitação", detail: e.Message);
            }
        }

        [HttpPost("PutStatus/{serviceId}/{progress}")]
        public async Task<ActionResult> PutStatus(Guid serviceId, int progress)
        {
            try
            {
                var service = _ServiceRepository.Get(serviceId);
                service.ScheduleProgress = (ScheduleProgress)progress;
                _ServiceRepository.Update(service);

                return Ok("Status alterado com sucesso");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("Download/{id}")]
        public async Task<ActionResult> DownloadFile(Guid id)
        {
            try
            {
                var file = _FileRepository.Get(id);
                if (file == null)
                    return NotFound("Arquivo não localizado");

                return File(file.File, "application/octet-stream", file.FileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}