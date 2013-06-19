using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace ScrollsModLoader {
	public class ExceptionLogger
	{
		private String id;
		private int version;
		private String url;

		private ExceptionType type;

		public enum ExceptionType { MODLOADER, MOD };

		public ExceptionLogger(Mod m, Repo r)
		{
			this.version = m.version;
			this.url = r.url;
			this.id = m.id;

			this.type = ExceptionType.MOD;
		}

		// modloader errors can be constructed without parameters
		public ExceptionLogger()
		{
			this.version = ModLoader.getVersion();
			this.url = "http://mods.scrollsguide.com";

			this.type = ExceptionType.MODLOADER;
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
			lp.Add("version", Convert.ToString(version));
			lp.Add("exception", e.ToString());

			return lp;
		}

		private void makeRequest(NameValueCollection lp)
		{
			using (WebClient wb = new WebClient())
			{
				byte[] response = wb.UploadValues(getCrashDumpUrl(), "POST", lp);
				System.Diagnostics.Debug.WriteLine(Encoding.ASCII.GetString(response));
			}
		}

		private String getCrashDumpUrl()
		{
			if (type == ExceptionType.MOD)
			{
				return this.url + "/crash/mod/" + this.id;
			}
			else // type == ExceptionType.MODLOADER
			{
				return this.url + "/crash/modloader";
			}
		}
	}
}

