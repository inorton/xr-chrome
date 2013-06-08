using System;
using XR.Server.Http;

namespace XR.Framework
{
	public class RequestContextBase
	{
		public UriRequestEventArgs Request { get; private set; }

		public virtual bool Load (UriRequestEventArgs req)
		{
			req.Handled = true;
			req.SetResponseState (200);
			req.SetResponseType (GetContentType (req.Request.Url));
			return req.Handled;
		}

		public virtual string GetContentType (Uri requestUri)
		{
			return "text/html";
		}
	}
}

