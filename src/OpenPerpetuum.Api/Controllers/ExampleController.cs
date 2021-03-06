﻿using Microsoft.AspNetCore.Mvc;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.Core.SharedIdentity.Authorisation;
using OpenPerpetuum.Core.SharedIdentity.Authorisation.Policy;
using System.Collections.Generic;

namespace OpenPerpetuum.Api.Controllers
{
	[Route("api/[controller]")]
    [ApiController]
    public class ExampleController : ApiControllerBase
    {
		public ExampleController(ICoreContext coreContext) : base(coreContext)
		{ }

        // GET api/values
        // Requires that the user identified by the Bearer has the OWNER permission in the OP DB
        [HttpGet("{id}"), RequiresPermission(Permission.OWNER)]
        public ActionResult<IEnumerable<string>> Get(int id)
        {
			if (id < 10)
				return BadRequest(new { Message = "Use ID 10 - 100 to generate NotFound. USe ID 101+ to return OK" });

			if (id <= 100)
				return NotFound();

			return Ok(new string[] { "value1", "value2", GenericContext.CurrentDateTime.ToString("yyyy/MM/dd HH:mm:ss") });
        }
    }
}
