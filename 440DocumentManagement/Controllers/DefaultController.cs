using Microsoft.AspNetCore.Mvc;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("/")]
	public class DefaultController : Controller
	{
		[HttpGet]
		public string Get()
		{
			return "good";
		}
	}
}
