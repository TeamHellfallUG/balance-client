using System;
using System.Net;
using System.Collections.Specialized;

namespace Balance.Http
{
	public class Inquiry
	{
		public Methods Method { get; set; }
		public String Endpoint { get; set; }
		public WebHeaderCollection Headers { get; set; }
		public NameValueCollection Body { get; set; }
		public ICredentials Auth { get; set; }
		public NameValueCollection Query { get; set; }
		public Int32 Timeout { get; set; }

		public Inquiry()
		{
			Timeout = 5000;
		}

		public Inquiry(Methods method, String endpoint, WebHeaderCollection headers)
		{
			Timeout = 5000;

			Method = method;
			Endpoint = endpoint;
			Headers = headers;
		}

		public Inquiry(Methods method, String endpoint, WebHeaderCollection headers, NameValueCollection body) 
		{
			Timeout = 5000;

			Method = method;
			Endpoint = endpoint;
			Headers = headers;
			Body = body;
		}

		public Inquiry(Methods method, String endpoint, NameValueCollection body)
		{
			Timeout = 5000;

			Method = method;
			Endpoint = endpoint;
			Body = body;
		}
	}
}
