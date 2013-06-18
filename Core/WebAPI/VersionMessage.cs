using System;

namespace ScrollsModLoader
{
	public class VersionMessage : Message
	{
		public new String msg;
		private VersionField data;

		public VersionMessage ()
		{
		}

		public int version() {
			return data.version;
		}
	}

	internal class VersionField {
		public int version;
	}
}

