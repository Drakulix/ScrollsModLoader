using System;

namespace ScrollsModLoader.Interfaces
{
	public interface IOkStringsCancelCallback : ICancelCallback
	{
		void PopupOk (string popupType, string username, string password);
	}
}

