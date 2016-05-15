using KSPSerialIODebugTool.Properties;

namespace KSPSerialIODebugTool
{
	public class SettingsNStuff
	{
		public string DefaultPort;
		public double refreshrate;
		public int HandshakeDelay;
		public int HandshakeDisable;
		public int BaudRate;
		// Throttle and axis controls have the following settings:
		// 0: The internal value (supplied by KSP) is always used.
		// 1: The external value (read from serial packet) is always used.
		// 2: If the internal value is not zero use it, otherwise use the external value.
		// 3: If the external value is not zero use it, otherwise use the internal value.    
		public int PitchEnable;
		public int RollEnable;
		public int YawEnable;
		public int TXEnable;
		public int TYEnable;
		public int TZEnable;
		public int WheelSteerEnable;
		public int ThrottleEnable;
		public int WheelThrottleEnable;
		public double SASTol;

		 public SettingsNStuff()
		{
			//cfg["refresh"] = 0.08;
			//cfg["DefaultPort"] = "COM1";
			//cfg["HandshakeDelay"] = 2500;
			Debug.Print("KSPSerialIO: Loading settings...");


			DefaultPort = Settings.Default.DefaultPort;
			Debug.Print("KSPSerialIO: Default Port = " + DefaultPort);

			refreshrate = Settings.Default.refresh;
			Debug.Print("KSPSerialIO: Refreshrate = " + refreshrate);

			BaudRate = Settings.Default.BaudRate;
			Debug.Print("KSPSerialIO: BaudRate = " + BaudRate);

			HandshakeDelay = Settings.Default.HandshakeDelay;
			Debug.Print("KSPSerialIO: Handshake Delay = " + HandshakeDelay);

			HandshakeDisable = Settings.Default.HandshakeDisable;
			Debug.Print("KSPSerialIO: Handshake Disable = " + HandshakeDisable);

			PitchEnable = Settings.Default.PitchEnable;
			Debug.Print("KSPSerialIO: Pitch Enable = " + PitchEnable);

			RollEnable = Settings.Default.RollEnable;
			Debug.Print("KSPSerialIO: Roll Enable = " + RollEnable);

			YawEnable = Settings.Default.YawEnable;
			Debug.Print("KSPSerialIO: Yaw Enable = " + YawEnable);

			TXEnable = Settings.Default.TXEnable;
			Debug.Print("KSPSerialIO: Translate X Enable = " + TXEnable);

			TYEnable = Settings.Default.TYEnable;
			Debug.Print("KSPSerialIO: Translate Y Enable = " + TYEnable);

			TZEnable = Settings.Default.TZEnable;
			Debug.Print("KSPSerialIO: Translate Z Enable = " + TZEnable);

			WheelSteerEnable = Settings.Default.WheelSteerEnable;
			Debug.Print("KSPSerialIO: Wheel Steering Enable = " + WheelSteerEnable);

			ThrottleEnable = Settings.Default.ThrottleEnable;
			Debug.Print("KSPSerialIO: Throttle Enable = " + ThrottleEnable);

			WheelThrottleEnable = Settings.Default.WheelThrottleEnable;
			Debug.Print("KSPSerialIO: Wheel Throttle Enable = " + WheelThrottleEnable);

			SASTol = Settings.Default.SASTol;
			Debug.Print("KSPSerialIO: SAS Tol = " + SASTol);
		}
	}
}
