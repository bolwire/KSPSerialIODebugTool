namespace KSPSerialIODebugTool
{
	public static class Debug
	{
		public delegate void LogMessageEventHandler(string s);
		public static event LogMessageEventHandler NewLogMessage;

		public delegate void PrintEventHandler(string s);
		public static event PrintEventHandler NewPrintMessage;

		public delegate void PostScreenMessageEventHandler(string s);
		public static event PostScreenMessageEventHandler NewPostScreenMessage;

		public static void Log(string s)
		{
			NewLogMessage?.Invoke(@"[Debug Log] " + s);
		}

		public static void Print(string s)
		{
			NewPrintMessage?.Invoke(@"[Print] " + s);
		}

		public static void PostScreenMessage(string s)
		{
			NewPostScreenMessage?.Invoke(@"[PostScreenMessage] " + s);
		}
	}
}
