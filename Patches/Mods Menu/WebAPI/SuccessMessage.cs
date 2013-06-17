using System;

namespace ScrollsModLoader
{
	public abstract class SuccessMessage : Message
	{
		public new string msg;
		public object data;

		public SuccessMessage ()
		{
		}

		public bool successful() {
			return (this.msg.Equals ("success")); // api call succeeded
		}
	}
}

