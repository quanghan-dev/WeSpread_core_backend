using BLL.Dto.Payment.MoMo.IPN;
using BLL.Service;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace API.Controllers
{
    [EnableCors("MyPolicy")]
    [Route("core/api/ipn")]
    [ApiController]
    public class IPNController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IMoMoService _momoService;


        public IPNController(ILogger logger, IMoMoService moMoService)
        {
            _logger = logger;
            _momoService = moMoService;
        }

        [HttpPost("momo")]
        public IActionResult ReceiveIPN([FromBody] MoMoIPNRequest momoIPNRequest)
        {

            _logger.Information($"POST core/api/ipn/momo START Request: {JsonConvert.SerializeObject(momoIPNRequest)}");

            Stopwatch watch = new Stopwatch();
            watch.Start();

            MoMoIPNResponse momoIPNResponse = _momoService.ProcessIPN(momoIPNRequest);

            string json = JsonConvert.SerializeObject(momoIPNResponse);

            watch.Stop();

            _logger.Information($"POST core/api/ipn/momo END duration: {watch.ElapsedMilliseconds} ms -----------Response: {json}");

            return Ok(json);
        }
    }
}
