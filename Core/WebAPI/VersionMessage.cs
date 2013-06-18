using System;

namespace ScrollsModLoader
{
	public class VersionMessage : Message
	{
		public new String msg;
		public VersionField data;

		public VersionMessage ()
		{
		}

		public int version() {
			return data.version;
		}
	}

	public class VersionField {
		public int version;
	}
}

