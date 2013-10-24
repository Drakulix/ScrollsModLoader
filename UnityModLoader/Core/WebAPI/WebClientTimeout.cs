using System;
using System.Net;

namespace UnityModLoader
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

