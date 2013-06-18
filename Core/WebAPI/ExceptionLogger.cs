using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace ScrollsModLoader {
	class ExceptionLogger
	{
		private Mod m;
		private Repo r;
	
		public ExceptionLogger(Mod m, Repo r)
		{
			this.m = m;
			this.r = r;
		}
	
		public void logException(Exception e)
		{
			NameValueCollection logParams = getLogParams(e);
	
			makeRequest(logParams);
		}
	
		private NameValueCollection getLogParams(Exception e)
		{
			NameValueCollection lp = new NameValueCollection();
			lp.Add("os", Platform.getOS().ToString());
			lp.Add("modversion", Convert.ToString(m.version));
			lp.Add("exception", e.ToString());
	
			return lp;
		}
	
		private void makeRequest(NameValueCollection lp)
		{
			using (WebClient wb = new WebClient())
			{
				byte[] response = wb.UploadValues(r.url + "/crash/mod/" + m.id, "POST", lp);
				System.Diagnostics.Debug.WriteLine(Encoding.ASCII.GetString(response));
			}
		}
	}
}

