using System;
using System.Net;

namespace ScrollsModLoader
{
	public class WebClientTimeOut : WebClient
	{
		protected override WebRequest GetWebRequest(Uri address)
		{
			WebRequest webRequest = base.GetWebRequest(address);
			webRequest.Timeout = 5000;
			return webRequest;
		}
	}
}

