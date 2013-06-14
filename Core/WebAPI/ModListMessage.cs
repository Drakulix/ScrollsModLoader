using System;

namespace ScrollsModLoader
{
	public class ModListMessage : Message
	{
		public new String msg;
		public ModDataFieldDeserializer[] data;

		public class ModDataFieldDeserializer
		{
			//e.g. "id":"13423fca4f53a14a4b2716b539dbe9db","name":"GameReplay","description":"Record Games!","version":1,"versionCode":"1.0"

			public String id;
			public String name;
			public String description;
			public int version;
			public String versionCode;
		}
	}
}