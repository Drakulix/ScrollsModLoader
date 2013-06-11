using System;
using System.IO;

	public static class Extensions
	{
		public static byte[] ReadToEnd(this System.IO.Stream stream)
		{
			long originalPosition = 0;

			if(stream.CanSeek)
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
				if(stream.CanSeek)
				{
					stream.Position = originalPosition; 
				}
			}
		}

		public static bool InheritsFrom(this Type type, Type baseType) {
			if (type == null)
				return false;
			if (type.Equals (baseType))
				return true;
			return type.BaseType.InheritsFrom(baseType);
		}

		public static bool EqualsReference(this Mono.Cecil.MethodDefinition definition, Mono.Cecil.MethodReference reference) {
			bool parameterMatch = (reference.Parameters.Count == definition.Parameters.Count);
			if (parameterMatch)
				for ( int i=0; i<reference.Parameters.Count; i++) {
				Mono.Cecil.ParameterDefinition refParam = reference.Parameters  [i];
				Mono.Cecil.ParameterDefinition defParam = definition.Parameters [i];
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
	}
	
	
