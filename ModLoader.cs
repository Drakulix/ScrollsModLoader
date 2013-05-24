using System;
using LinFu.AOP.Interfaces;

//namespace ScrollsModLoader {

	/*
	 * handels mod loading and debug file logging
	 * actual patches like logging network traffic, should be seperated in the future
	 * this class should only pass invokes through itself to the right mods
	 * 
	 * TO-DO: Add actual ModInterface ;)
	 * 
	 */

	public class CommunicatorReplace : IInterceptor
	{

		public object Intercept (IInvocationInfo info)
		{
			ModLoader.Log("Client: "+info.Arguments[0]);
			return info.TargetMethod.Invoke (info.Target, info.Arguments);
		}

	}

	public class SimpleMethodReplacementProvider : BaseMethodReplacementProvider 
	{
		protected override IInterceptor GetReplacement (object host, IInvocationInfo context)
		{
			return new CommunicatorReplace ();
		}
	}

	public class ModLoader : ICommListener
	{
		static ModLoader instance = null;
		private System.IO.StreamWriter log = null;

		public ModLoader ()
		{
			log = System.IO.File.CreateText("log.txt");
		}

		public static void Log(string s) {
			instance.log.WriteLine(s);
			instance.log.Flush ();
		}

		#region ICommListener implementation
		public void handleMessage (Message msg)
		{
			ModLoader.Log("Server: "+msg);
		}
		#endregion
		
		//initial game callback
		public static void Init() {
			instance = new ModLoader();
			App.Communicator.addListener(instance);
			MethodBodyReplacementProviderRegistry.SetProvider(new SimpleMethodReplacementProvider());
			
			//not needed I hope
			//((IModifiableType)App.Communicator).IsInterceptionDisabled = false;
		}
	}
