using Microsoft.Win32;
using System;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace KSPSerialIODebugTool
{
	public class KSPSerialPort
	{
		private SettingsNStuff SettingsNStuff = new SettingsNStuff();
		public SerialPort Port;
		public string PortNumber;
		public Boolean DisplayFound = false;
		public Boolean ControlReceived = false;

		public VesselData VData;
		public ControlPacket CPacket;
		private HandShakePacket HPacket;

		public VesselControls VControls = new VesselControls();
		public VesselControls VControlsOld = new VesselControls();

		private byte[] buffer = new byte[255];
		private byte rx_len;
		private byte rx_array_inx;
		private int structSize;
		private byte id = 255;

		private const byte HSPid = 0, VDid = 1, Cid = 101; //hard coded values for packet IDS


		public void sendPacket(object anything)
		{
			byte[] Payload = StructureToByteArray(anything);
			byte header1 = 0xBE;
			byte header2 = 0xEF;
			byte size = (byte)Payload.Length;
			byte checksum = size;

			byte[] Packet = new byte[size + 4];

			//Packet = [header][size][payload][checksum];
			//Header = [Header1=0xBE][Header2=0xEF]
			//size = [payload.length (0-255)]

			for (int i = 0; i < size; i++)
			{
				checksum ^= Payload[i];
			}

			Payload.CopyTo(Packet, 3);
			Packet[0] = header1;
			Packet[1] = header2;
			Packet[2] = size;
			Packet[Packet.Length - 1] = checksum;

			Port.Write(Packet, 0, Packet.Length);
		}

		private void Begin()
		{
			if (PortNumber == null)
				PortNumber = SettingsNStuff.DefaultPort;

			Port = new SerialPort(PortNumber, SettingsNStuff.BaudRate, Parity.None, 8, StopBits.One) {ReceivedBytesThreshold = 3};
			Port.DataReceived += Port_ReceivedEvent;
			//Debug.Log("BATMAN: DtrEnable = " + Port.DtrEnable + " / RtsEnable = " + Port.RtsEnable);
		}

		//these are copied from the intarwebs, converts struct to byte array
		private byte[] StructureToByteArray(object obj)
		{
			int len = Marshal.SizeOf(obj);
			byte[] arr = new byte[len];
			IntPtr ptr = Marshal.AllocHGlobal(len);
			Marshal.StructureToPtr(obj, ptr, true);
			Marshal.Copy(ptr, arr, 0, len);
			Marshal.FreeHGlobal(ptr);
			return arr;
		}

		private object ByteArrayToStructure(byte[] bytearray, object obj)
		{
			int len = Marshal.SizeOf(obj);

			IntPtr i = Marshal.AllocHGlobal(len);

			Marshal.Copy(bytearray, 0, i, len);

			obj = Marshal.PtrToStructure(i, obj.GetType());

			Marshal.FreeHGlobal(i);

			return obj;
		}
		/*
     private T ReadUsingMarshalUnsafe<T>(byte[] data) where T : struct
     {
     unsafe
     {
     fixed (byte* p = &data[0])
     {
     return (T)Marshal.PtrToStructure(new IntPtr(p), typeof(T));
     }
     }
     }
    */
		 void initializeDataPackets()
		{
			VData = new VesselData();
			VData.id = VDid;

			HPacket = new HandShakePacket();
			HPacket.id = HSPid;
			HPacket.M1 = 3;
			HPacket.M2 = 1;
			HPacket.M3 = 4;

			CPacket = new ControlPacket();

			VControls.ControlGroup = new Boolean[11];
			VControlsOld.ControlGroup = new Boolean[11];
		}

		 public KSPSerialPort()
		{
			Debug.Log("KSPSerialPort: KSPSerialPort()");

			if (DisplayFound)
			{
				Debug.Log("KSPSerialIO: running...");
				Begin();
			}
			else
			{
				Debug.Log("KSPSerialIO: Compatible Version 0.17.6");
				Debug.Log("KSPSerialIO: Getting serial ports...");
				Debug.Log("KSPSerialIO: Output packet size: " + Marshal.SizeOf(VData).ToString() + "/255");
				initializeDataPackets();

				try
				{
					//Use registry hack to get a list of serial ports until we get system.io.ports
					RegistryKey SerialCOMSKey = Registry.LocalMachine.OpenSubKey(@"HARDWARE\\DEVICEMAP\\SERIALCOMM\\");

					Debug.Log("KSPSerialPort: Calling Begin()");
					Begin();
					Debug.Log("KSPSerialPort: Returned from Begin()");

					//print("KSPSerialIO: receive threshold " + Port.ReceivedBytesThreshold.ToString());

					if (SerialCOMSKey == null)
					{
						Debug.Log("KSPSerialIO: Dude do you even win32 serial port??");
					}
					else
					{
						Debug.Log("KSPSerialPort: SerialCOMSKey != null");
						String[] realports = SerialCOMSKey.GetValueNames(); // get list of all serial devices
						String[] names = new string[realports.Length + 1];  // make a new list with 1 extra, we put the default port first
						realports.CopyTo(names, 1);

						Debug.Log("KSPSerialIO: Found " + names.Length.ToString() + " serial ports");

						//look through all found ports for our display
						int j = 0;

						foreach (string PortName in names)
						{
							if (j == 0) // try default port first
							{
								PortNumber = SettingsNStuff.DefaultPort;
								Debug.Log("KSPSerialIO: trying default port " + PortNumber);
							}
							else
							{
								PortNumber = (string)SerialCOMSKey.GetValue(PortName);
								Debug.Log("KSPSerialIO: trying port " + PortName + " - " + PortNumber);
							}

							Port.PortName = PortNumber;

							j++;

							if (!Port.IsOpen)
							{
								try
								{
									Port.Open();
								}
								catch (Exception e)
								{
									Debug.Log("Error opening serial port " + Port.PortName + ": " + e.Message);
								}

								//secret handshake
								if (Port.IsOpen && (SettingsNStuff.HandshakeDisable == 0))
								{
									Thread.Sleep(SettingsNStuff.HandshakeDelay);
									//Port.DiscardOutBuffer();
									//Port.DiscardInBuffer();

									sendPacket(HPacket);

									//wait for reply
									int k = 0;

									while (Port.BytesToRead == 0 && k < 15 && !DisplayFound)
									{
										Thread.Sleep(100);
										k++;
									}

									Port.Close();
									if (DisplayFound)
									{
										Debug.Log("KSPSerialIO: found KSP Display at " + Port.PortName);
										break;
									}
									else
									{
										Debug.Log("KSPSerialIO: KSP Display not found");
									}
								}
								else if (Port.IsOpen && (SettingsNStuff.HandshakeDisable == 1))
								{
									DisplayFound = true;
									Debug.Log("KSPSerialIO: Handshake disabled, using " + Port.PortName);
									break;
								}
							}
							else
							{
								Debug.Log("KSPSerialIO: " + PortNumber + "is already being used.");
							}
						}
					}

				}
				catch (Exception e)
				{
					Debug.Print(e.Message);
				}
			}
		}

		public bool Handshaking = false;

		public void DoHandshake()
		{
			Handshaking = true;
			sendPacket(HPacket);
			Handshaking = false;
		}
		private string readline()
		{
			string result = null;
			char c;
			int j = 0;

			c = (char)Port.ReadByte();
			while (c != '\n' && j < 255)
			{
				result += c;
				c = (char)Port.ReadByte();
				j++;
			}
			return result;
		}

		private void Port_ReceivedEvent(object sender, SerialDataReceivedEventArgs e)
		{
			while (Port.BytesToRead > 0)
			{
				if (processCOM())
				{
					switch (id)
					{
						case HSPid:
							HPacket = (HandShakePacket)ByteArrayToStructure(buffer, HPacket);
							//Invoke("HandShake", 0);
							HandShake();

							if ((HPacket.M1 == 3) && (HPacket.M2 == 1) && (HPacket.M3 == 4))
							{
								DisplayFound = true;

							}
							else
							{
								DisplayFound = false;
							}
							break;
						case Cid:
							VesselControls();
							//Invoke("VesselControls", 0);
							break;
						default:
							//Invoke("Unimplemented", 0);
							Unimplemented();
							break;
					}
				}
			}
		}

		private bool processCOM()
		{
			byte calc_CS;

			if (rx_len == 0)
			{
				while (Port.ReadByte() != 0xBE)
				{
					if (Port.BytesToRead == 0)
						return false;
				}

				if (Port.ReadByte() == 0xEF)
				{
					rx_len = (byte)Port.ReadByte();
					id = (byte)Port.ReadByte();
					rx_array_inx = 1;

					switch (id)
					{
						case HSPid:
							structSize = Marshal.SizeOf(HPacket);
							break;
						case Cid:
							structSize = Marshal.SizeOf(CPacket);
							break;
					}

					//make sure the binary structs on both Arduino and plugin are the same size.
					if (rx_len != structSize || rx_len == 0)
					{
						rx_len = 0;
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			else
			{
				while (Port.BytesToRead > 0 && rx_array_inx <= rx_len)
				{
					buffer[rx_array_inx++] = (byte)Port.ReadByte();
				}
				buffer[0] = id;

				if (rx_len == (rx_array_inx - 1))
				{
					//seem to have got whole message
					//last uint8_t is CS
					calc_CS = rx_len;
					for (int i = 0; i < rx_len; i++)
					{
						calc_CS ^= buffer[i];
					}

					if (calc_CS == buffer[rx_array_inx - 1])
					{//CS good
						rx_len = 0;
						rx_array_inx = 1;
						return true;
					}
					else
					{
						//failed checksum, need to clear this out anyway
						rx_len = 0;
						rx_array_inx = 1;
						return false;
					}
				}
			}

			return false;
		}

		private void HandShake()
		{
			Debug.Log("KSPSerialIO: Handshake received - " + HPacket.M1.ToString() + HPacket.M2.ToString() + HPacket.M3.ToString());
		}

		private void VesselControls()
		{
			CPacket = (ControlPacket)ByteArrayToStructure(buffer, CPacket);

			VControls.SAS = BitMathByte(CPacket.MainControls, 7);
			VControls.RCS = BitMathByte(CPacket.MainControls, 6);
			VControls.Lights = BitMathByte(CPacket.MainControls, 5);
			VControls.Gear = BitMathByte(CPacket.MainControls, 4);
			VControls.Brakes = BitMathByte(CPacket.MainControls, 3);
			VControls.Precision = BitMathByte(CPacket.MainControls, 2);
			VControls.Abort = BitMathByte(CPacket.MainControls, 1);
			VControls.Stage = BitMathByte(CPacket.MainControls, 0);
			VControls.Pitch = (float)CPacket.Pitch / 1000.0F;
			VControls.Roll = (float)CPacket.Roll / 1000.0F;
			VControls.Yaw = (float)CPacket.Yaw / 1000.0F;
			VControls.TX = (float)CPacket.TX / 1000.0F;
			VControls.TY = (float)CPacket.TY / 1000.0F;
			VControls.TZ = (float)CPacket.TZ / 1000.0F;
			VControls.WheelSteer = (float)CPacket.WheelSteer / 1000.0F;
			VControls.Throttle = (float)CPacket.Throttle / 1000.0F;
			VControls.WheelThrottle = (float)CPacket.WheelThrottle / 1000.0F;

			for (int j = 1; j <= 10; j++)
			{
				VControls.ControlGroup[j] = BitMathUshort(CPacket.ControlGroup, j);
			}

			ControlReceived = true;
			//Debug.Log("KSPSerialIO: ControlPacket received");
		}

		private Boolean BitMathByte(byte x, int n)
		{
			return ((x >> n) & 1) == 1;
		}

		private Boolean BitMathUshort(ushort x, int n)
		{
			return ((x >> n) & 1) == 1;
		}

		private void Unimplemented()
		{
			Debug.Log("KSPSerialIO: Packet id unimplemented");
		}

		private void debug()
		{
			Debug.Log(Port.BytesToRead.ToString() + "BTR");
		}


		public void ControlStatus(int n, bool s)
		{
			if (s)
				VData.ActionGroups |= (UInt16)(1 << n);    // forces nth bit of x to be 1. all other bits left alone.
			else
				VData.ActionGroups &= (UInt16)~(1 << n);   // forces nth bit of x to be 0. all other bits left alone.
		}

		 void OnDestroy()
		{
			if (Port.IsOpen)
			{
				Port.Close();
				Port.DataReceived -= Port_ReceivedEvent;
				Debug.Log("KSPSerialIO: Port closed");
			}
		}
	}
}
