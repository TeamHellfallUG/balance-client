using System;
using System.Net;

namespace Balance.Http
{
	public class Response
	{
		public HttpStatusCode Status { get; private set; }
		public WebHeaderCollection Headers { get; private set; }
		public String Body { get; private set; }
		public Double ElapsedTime { get; private set; }
		public String Charset { get; private set; }
		public DateTime LastModified { get; private set; }

		public Response(HttpStatusCode status, WebHeaderCollection headers, String body,
		                Double elapsedTime, String charset, DateTime lastModified)
		{
			Status = status;
			Headers = headers;
			Body = body;
			ElapsedTime = elapsedTime;
			Charset = charset;
			LastModified = lastModified;
		}
	}
}
