using System;
using System.IO;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using GameReplay.Mod;
using JsonFx.Json;

public static class Extensions
{
	public static byte[] ReadToEnd(this System.IO.Stream stream)
	{
		long originalPosition = 0;

		if (stream.CanSeek)
		{
			originalPosition = stream.Position;
			stream.Position = 0;
		}

		try
		{
			byte[] readBuffer = new byte[4096];

			int totalBytesRead = 0;
			int bytesRead;

			while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
			{
				totalBytesRead += bytesRead;

				if (totalBytesRead == readBuffer.Length)
				{
					int nextByte = stream.ReadByte();
					if (nextByte != -1)
					{
						byte[] temp = new byte[readBuffer.Length * 2];
						Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
						Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
						readBuffer = temp;
						totalBytesRead++;
					}
				}
			}

			byte[] buffer = readBuffer;
			if (readBuffer.Length != totalBytesRead)
			{
				buffer = new byte[totalBytesRead];
				Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
			}
			return buffer;
		}
		finally
		{
			if (stream.CanSeek)
			{
				stream.Position = originalPosition;
			}
		}
	}

	public static bool InheritsFrom(this Type type, Type baseType)
	{
		if (type == null)
			return false;
		if (type.Equals(baseType))
			return true;
		return type.BaseType.InheritsFrom(baseType);
	}

	public static bool EqualsReference(this Mono.Cecil.MethodDefinition definition, Mono.Cecil.MethodReference reference)
	{
		bool parameterMatch = (reference.Parameters.Count == definition.Parameters.Count);
		if (parameterMatch)
			for (int i = 0; i < reference.Parameters.Count; i++)
			{
				Mono.Cecil.ParameterDefinition refParam = reference.Parameters[i];
				Mono.Cecil.ParameterDefinition defParam = definition.Parameters[i];
				if (!refParam.ParameterType.FullName.Equals(defParam.ParameterType.FullName))
					parameterMatch = false;
			}

		return ((reference.CallingConvention.Equals(definition.CallingConvention)
				 && (reference.DeclaringType.FullName.Equals(definition.DeclaringType.FullName))
				 && (reference.Name.Equals(definition.Name))
				 && (reference.ReturnType.ReturnType.FullName.Equals(definition.ReturnType.ReturnType.FullName))
				 && (reference.GenericParameters.Count == definition.GenericParameters.Count) && parameterMatch));
	}

	public static void DeleteDirectory(string target_dir)
	{
		target_dir = Path.GetFullPath(target_dir);

		string[] files = Directory.GetFiles(target_dir);
		string[] dirs = Directory.GetDirectories(target_dir);

		foreach (string file in files)
		{
			File.SetAttributes(file, FileAttributes.Normal);
			File.Delete(file);
		}

		foreach (string dir in dirs)
		{
			DeleteDirectory(dir);
		}

		Directory.Delete(target_dir, false);
	}

	public static ResultMessage HttpUploadFile(string url, string file, string paramName, string contentType, NameValueCollection nvc)
	{
		string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
		byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

		HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
		wr.ContentType = "multipart/form-data; boundary=" + boundary;

		wr.Method = "POST";
		wr.KeepAlive = true;
		wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

		Stream rs = wr.GetRequestStream();

		string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
		foreach (string key in nvc.Keys)
		{
			rs.Write(boundarybytes, 0, boundarybytes.Length);
			string formitem = string.Format(formdataTemplate, key, nvc[key]);
			byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
			rs.Write(formitembytes, 0, formitembytes.Length);
		}
		rs.Write(boundarybytes, 0, boundarybytes.Length);

		string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
		string header = string.Format(headerTemplate, paramName, file, contentType);
		byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
		rs.Write(headerbytes, 0, headerbytes.Length);

		FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
		byte[] buffer = new byte[4096];
		int bytesRead = 0;
		while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
		{
			rs.Write(buffer, 0, bytesRead);
		}
		fileStream.Close();

		byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
		rs.Write(trailer, 0, trailer.Length);
		rs.Close();

		ResultMessage result = new ResultMessage();
		result.msg = "fail";
		result.data = "Please try again later :)";
		WebResponse wresp = null;
		try
		{
			wresp = wr.GetResponse();
			Stream stream2 = wresp.GetResponseStream();
			StreamReader reader2 = new StreamReader(stream2);

			String fromServer = reader2.ReadToEnd();

			Console.WriteLine(string.Format("File uploaded, server response is: {0}", fromServer));

			JsonReader r = new JsonReader();
			result = r.Read(fromServer, System.Type.GetType("ResultMessage")) as ResultMessage;
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error uploading file", ex);
			if (wresp != null)
			{
				wresp.Close();
				wresp = null;
			}
		}
		finally
		{
			wr = null;
		}

		return result;
	}

	public static long ToUnixTimestamp(DateTime d)
	{
		var duration = d - new DateTime(1970, 1, 1, 0, 0, 0);
		return (long)duration.TotalSeconds;
	}

	/*
	public static void ShowSharePopup(this Popups popups, String title, String description)
	{
		typeof(Popups).GetField("overlay.enabled", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(popups, true);
	}
	 * */
}