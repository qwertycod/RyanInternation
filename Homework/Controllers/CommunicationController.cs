using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

// System to show how 2 classes with interface works with DI
namespace Homework.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommunicationController : ControllerBase
    {
        private readonly ISender sendEmail;
        private readonly ISender sendSms;

        public CommunicationController(IEnumerable<ISender> senders)
        {
              sendSms = senders.FirstOrDefault(x => x is SendSms) as ISender;
              sendEmail = senders.FirstOrDefault(x => x is SendEmail) as ISender;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var res1 = sendEmail.SendMessage();
            var res2 = sendSms.SendMessage();

            return Ok(new { res1 = res1, res2 = res2 });
        }

        [HttpGet("GetFormatters")]      // need to register formatters to work
        public IActionResult GetFormatters([FromServices] FormatterCollection<IOutputFormatter> formatters)
        {
            return Ok(formatters.Select(f => f.GetType().Name));
        }
    }
}
