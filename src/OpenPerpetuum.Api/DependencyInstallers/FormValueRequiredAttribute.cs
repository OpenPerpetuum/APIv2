using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using System;

namespace OpenPerpetuum.Api.DependencyInstallers
{
	public sealed class FormValueRequiredAttribute : ActionMethodSelectorAttribute
    {
		private readonly string name;

		public FormValueRequiredAttribute(string name)
		{
			this.name = name;
		}

		public override bool IsValidForRequest(RouteContext context, ActionDescriptor action)
		{
			// Wheeeeeeee!!! :D
			return !(string.Equals(context.HttpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(context.HttpContext.Request.Method, "HEAD", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(context.HttpContext.Request.Method, "DELETE", StringComparison.OrdinalIgnoreCase) ||
				string.Equals(context.HttpContext.Request.Method, "TRACE", StringComparison.OrdinalIgnoreCase) ||
				string.IsNullOrEmpty(context.HttpContext.Request.ContentType) ||
				!context.HttpContext.Request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) ||
				string.IsNullOrEmpty(context.HttpContext.Request.Form[name]));
		}
	}
}
