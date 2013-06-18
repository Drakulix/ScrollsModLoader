using System;
using System.Reflection;

public static class ScrollsExtension
{
	public static void ShowTextInput(this Popups popups, IOkStringCancelCallback callback, string loadedDeckName, string problems, string popupType, string header, string description, string okText)
	{
		popups.ShowSaveDeck (callback, loadedDeckName, problems);
		typeof(Popups).GetField ("popupType", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (popups, popupType);
		typeof(Popups).GetField ("header", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (popups, header);
		typeof(Popups).GetField ("description", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (popups, description);
		typeof(Popups).GetField ("okText", BindingFlags.Instance | BindingFlags.NonPublic).SetValue (popups, okText);
	}
}

