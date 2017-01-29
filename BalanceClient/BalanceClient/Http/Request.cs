using System;
using System.Threading;
using System.Diagnostics;
using System.Net;

using BetterHttpClient;

namespace Balance.Http
{
	public class Request
	{
		public delegate void ResponseDelegate(Exception exception, Response response);
		public delegate void LogDelegate(String message);

		private String host;
		private Int32 port;
		private String scheme;
		private LogDelegate _log;

		public Request()
		{
			this.host = "localhost";
			this.port = 80;
			this.scheme = "http";
		}

		public Request(String host) {
			this.host = host;
			this.port = 80;
			this.scheme = "http";
		}

		public Request(String host, Int32 port) { 
			this.host = host;
			this.port = port;
			this.scheme = "http";
		}

		public Request(String host, Int32 port, String scheme) { 
			this.host = host;
			this.port = port;
			this.scheme = scheme;
		}

		public Request(String host, Int32 port, String scheme, LogDelegate log)
		{
			this.host = host;
			this.port = port;
			this.scheme = scheme;
			this._log = log;
		}

		public Request(String host, String scheme)
		{
			this.host = host;
			this.port = 0;
			this.scheme = scheme;
		}

		public Request(String host, String scheme, LogDelegate log)
		{
			this.host = host;
			this.port = 0;
			this.scheme = scheme;
			this._log = log;
		}

		private void log(String message)
		{
			if (this._log != null)
			{
				this._log("[Request(HttpClient)]: " + message);
			}
		}

		private String getPath(String endpoint) {
			
			if (endpoint == null) {
				endpoint = "/";
			}

			if (port != 0) { 
				return scheme + "://" + host + ":" + port + endpoint;
			}

			return scheme + "://" + host + endpoint;
		}

		public void request(Inquiry inquiry, ResponseDelegate callback) {

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			String url = getPath(inquiry.Endpoint);
			log("call to " + inquiry.Method.ToString() + " : " + url);

			Thread t = new Thread(() => {

				try
				{
					HttpClient client = new HttpClient();

					client.Encoding = System.Text.Encoding.UTF8;
					client.Headers = inquiry.Headers;
					client.AcceptEncoding = "gzip, deflate";
					client.UserAgent = "BalanceClient";
					client.Referer = "None";
					client.AllowAutoRedirect = false;
					client.NumberOfAttempts = 1;
					client.Timeout = TimeSpan.FromMilliseconds(inquiry.Timeout);

					if (inquiry.Auth != null) {
						client.Credentials = inquiry.Auth;
					}

					if (inquiry.Query != null) {
						client.QueryString = inquiry.Query;
					}

					String result = null;

					switch (inquiry.Method)
					{
						case Methods.GET:
							result = client.Get(url);
							break;

						case Methods.POST:
							result = client.Post(url, inquiry.Body);
							break;
					}

					if (client.LastResponse == null) {
						callback(new Exception("LastResponse is missing on HttpClient after a request has been made."), null);
						return;
					}

					HttpWebResponse lastResponse = (HttpWebResponse)client.LastResponse;

					stopwatch.Stop();
					Response response = new Response(lastResponse.StatusCode, 
					                                 client.ResponseHeaders, result, 
					                                 stopwatch.Elapsed.TotalMilliseconds, 
					                                 lastResponse.CharacterSet,
					                                 lastResponse.LastModified);
					
					stopwatch = null;
					client = null;

					log("call done " + inquiry.Method.ToString() + " : " + url + ", took: " + response.ElapsedTime + " ms.");
					callback(null, response);
				}
				catch (Exception exception){
					callback(exception, null);
				}
			});

			t.Start();
		}

	}
}
