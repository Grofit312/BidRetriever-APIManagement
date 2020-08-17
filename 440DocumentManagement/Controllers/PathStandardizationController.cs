using _440DocumentManagement.Helpers;
using _440DocumentManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace _440DocumentManagement.Controllers
{
	[Produces("application/json")]
	[Route("api")]
	public class PathStandardizationController : Controller
	{
		[HttpPost]
		[Route("StandardizeProjectName")]
		public IActionResult Post(ProjectNameStandardizationRequest request)
		{
			if (request.project_name == null)
			{
				return BadRequest(new
				{
					status = "Please provide project_name"
				});
			}

			return Ok(new
			{
				project_name = ValidationHelper.ValidateProjectName(request.project_name),
				status = "success"
			});
		}

		[HttpPost]
		[Route("StandardizeDestinationPath")]
		public IActionResult Post(DestinationPathStandardizationRequest request)
		{
			if (request.destination_path == null)
			{
				return BadRequest(new
				{
					status = "Please provide destination_path"
				});
			}

			return Ok(new
			{
				destination_path = ValidationHelper.ValidateDestinationPath(request.destination_path),
				status = "success"
			});
		}
	}
}