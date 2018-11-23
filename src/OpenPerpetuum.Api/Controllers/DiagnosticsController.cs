using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenPerpetuum.Api.Configuration;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.Core.Genxy;
using OpenPerpetuum.Core.Killboard.DataModel;

namespace OpenPerpetuum.Api.Controllers
{
	[Route("api/[controller]")]
	public class DiagnosticsController : ApiControllerBase
	{
		private readonly IGenxyReader genxyReader;
		private readonly IOptions<TestData> testData;

		public DiagnosticsController(ICoreContext coreContext, IGenxyReader genxyReader, IOptions<TestData> testData) : base(coreContext)
		{
			this.genxyReader = genxyReader;
			this.testData = testData;
		}

		[HttpGet("ReadGenxyTest")]
		public IActionResult ReadGenxyTest()
		{
			if (testData.Value != null)
			{
				if (!string.IsNullOrWhiteSpace(testData.Value.TestKillData))
				{
					var killData = genxyReader.Deserialise<KillDataGenxy>(testData.Value.TestKillData);

					return Ok(killData);
				}
			}

			return NotFound();
		}
	}
}
