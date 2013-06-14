using System;

namespace ScrollsModLoader
{
	public class RepoInfoMessage : Message
	{
		public new String msg;
		public InfoDataFieldDeserializer data;

		public RepoInfoMessage ()
		{
		}

		public class InfoDataFieldDeserializer
		{
			public String name;
			public String url;
			public int version;
			public int mods;

			public override String ToString() {
				return name+url;
			}
		}
	}
}

