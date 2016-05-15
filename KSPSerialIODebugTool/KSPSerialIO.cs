using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using KSPSerialIODebugTool.Properties;
using Timer = System.Windows.Forms.Timer;

namespace KSPSerialIODebugTool
{
	#region Structs
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct VesselData
	{
		public byte id;             //1
		public float AP;            //2
		public float PE;            //3
		public float SemiMajorAxis; //4
		public float SemiMinorAxis; //5
		public float VVI;           //6
		public float e;             //7
		public float inc;           //8
		public float G;             //9
		public int TAp;             //10
		public int TPe;             //11
		public float TrueAnomaly;   //12
		public float Density;       //13
		public int period;          //14
		public float RAlt;          //15
		public float Alt;           //16
		public float Vsurf;         //17
		public float Lat;           //18
		public float Lon;           //19
		public float LiquidFuelTot; //20
		public float LiquidFuel;    //21
		public float OxidizerTot;   //22
		public float Oxidizer;      //23
		public float EChargeTot;    //24
		public float ECharge;       //25
		public float MonoPropTot;   //26
		public float MonoProp;      //27
		public float IntakeAirTot;  //28
		public float IntakeAir;     //29
		public float SolidFuelTot;  //30
		public float SolidFuel;     //31
		public float XenonGasTot;   //32
		public float XenonGas;      //33
		public float LiquidFuelTotS;//34
		public float LiquidFuelS;   //35
		public float OxidizerTotS;  //36
		public float OxidizerS;     //37
		public UInt32 MissionTime;  //38
		public float deltaTime;     //39
		public float VOrbit;        //40
		public UInt32 MNTime;       //41
		public float MNDeltaV;      //42
		public float Pitch;         //43
		public float Roll;          //44
		public float Heading;       //45
		public UInt16 ActionGroups; //46  status bit order:SAS, RCS, Light, Gear, Brakes, Abort, Custom01 - 10 
		public byte SOINumber;      //47  SOI Number (decimal format: sun-planet-moon e.g. 130 = kerbin, 131 = mun)
		public byte MaxOverHeat;    //48  Max part overheat (% percent)
		public float MachNumber;    //49
		public float IAS;           //50  Indicated Air Speed
		public byte CurrentStage;   //51  Current stage number
		public byte TotalStage;     //52  TotalNumber of stages
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct HandShakePacket
	{
		public byte id;
		public byte M1;
		public byte M2;
		public byte M3;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ControlPacket
	{
		public byte id;
		public byte MainControls;                  //SAS RCS Lights Gear Brakes Precision Abort Stage 
		public byte Mode;                          //0 = stage, 1 = docking, 2 = map
		public ushort ControlGroup;                //control groups 1-10 in 2 bytes
		public byte AdditionalControlByte1;        //other stuff
		public byte AdditionalControlByte2;
		public short Pitch;                        //-1000 -> 1000
		public short Roll;                         //-1000 -> 1000
		public short Yaw;                          //-1000 -> 1000
		public short TX;                           //-1000 -> 1000
		public short TY;                           //-1000 -> 1000
		public short TZ;                           //-1000 -> 1000
		public short WheelSteer;                   //-1000 -> 1000
		public short Throttle;                     // 0 -> 1000
		public short WheelThrottle;                // 0 -> 1000
	};

	public struct VesselControls
	{
		public Boolean SAS;
		public Boolean RCS;
		public Boolean Lights;
		public Boolean Gear;
		public Boolean Brakes;
		public Boolean Precision;
		public Boolean Abort;
		public Boolean Stage;
		public int Mode;
		public Boolean[] ControlGroup;
		public float Pitch;
		public float Roll;
		public float Yaw;
		public float TX;
		public float TY;
		public float TZ;
		public float WheelSteer;
		public float Throttle;
		public float WheelThrottle;
	};

	public struct IOResource
	{
		public float Max;
		public float Current;
	}

	#endregion

	#region enums
	enum enumAG : int
	{
		SAS,
		RCS,
		Light,
		Gear,
		Brakes,
		Abort,
		Custom01,
		Custom02,
		Custom03,
		Custom04,
		Custom05,
		Custom06,
		Custom07,
		Custom08,
		Custom09,
		Custom10,
	};

	#endregion

	public partial class KSPSerialIO : Form
	{
		//Do not inialize here
		private StatusMessagesForm MessageForm;
		private SettingsNStuff SettingsNStuff;
		private KSPSerialPort KSPSerialPort;

		public double refreshrate = 1.0f;


		public KSPSerialIO()
		{
			InitializeComponent();
		}

		private void KSPSerialIO_Load(object sender, EventArgs e)
		{
			//These are initialized here so that the main form is loaded and ready
			MessageForm = new StatusMessagesForm();
			MessageForm.Show();

			SettingsNStuff = new SettingsNStuff();
			KSPSerialPort = new KSPSerialPort();
			
			//Are we ready now ?
			Awake();
		}

		void Awake()
		{
			Debug.PostScreenMessage("IO awake");
			if (!KSPSerialPort.DisplayFound)
			{
				//Take all the fun away
				buttonStart.Enabled =
					buttonStop.Enabled =
						buttonUpdate.Enabled = buttonUpdateOnce.Enabled = buttonHandshake.Enabled = buttonHandshakeOnce.Enabled = false;

				//Display bad news
				MessageBox.Show(
					@"OOPS! We didn't detect your controller/display. Please make sure it is connected and restart the application.",
					@"Arduino not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}

			refreshrate = SettingsNStuff.refreshrate;
		}

		void Start()
		{
			if (KSPSerialPort.DisplayFound)
			{
				if (!KSPSerialPort.Port.IsOpen)
				{
					Debug.PostScreenMessage("Starting serial port " + KSPSerialPort.Port.PortName);

					try
					{
						KSPSerialPort.Port.Open();
						Thread.Sleep(SettingsNStuff.HandshakeDelay);
					}
					catch (Exception e)
					{
						Debug.PostScreenMessage("Error opening serial port " + KSPSerialPort.Port.PortName);
						Debug.PostScreenMessage(e.Message);
					}
				}
				else
				{
					Debug.PostScreenMessage("Using serial port " + KSPSerialPort.Port.PortName);

					if (SettingsNStuff.HandshakeDisable == 1)
						Debug.PostScreenMessage("Handshake disabled");
				}

				Thread.Sleep(200);
			}
			else
			{
				Debug.PostScreenMessage("No display found");
			}

			//If we opened the com port we need to enable/disable buttons
			if (KSPSerialPort.Port.IsOpen)
			{
				buttonStart.Enabled = false;
				buttonStop.Enabled =
					buttonUpdate.Enabled = buttonUpdateOnce.Enabled = buttonHandshake.Enabled = buttonHandshakeOnce.Enabled = true;
			}
		}

		private bool _updatingControls = false;
		private void UpdateControls()
		{
			if (KSPSerialPort.Port.IsOpen)
			{
				_updatingControls = true;

				#region outputs

				KSPSerialPort.VData.AP = Convert.ToSingle(textBoxOrbitalApoapsis.Text);
				KSPSerialPort.VData.PE = Convert.ToSingle(textBoxOrbitalPeriapsis.Text);
				KSPSerialPort.VData.SemiMajorAxis = Convert.ToSingle(textBoxOrbitalSemiMajorAxis.Text);
				KSPSerialPort.VData.SemiMinorAxis = Convert.ToSingle(textBoxOrbitalSemiMinorAxis.Text);
				KSPSerialPort.VData.e = Convert.ToSingle(textBoxOrbitalEccentricity.Text);
				KSPSerialPort.VData.inc = Convert.ToSingle(textBoxOrbitalInclination.Text);
				KSPSerialPort.VData.TAp = Convert.ToInt32(textBoxOrbitalTimeToAP.Text);
				KSPSerialPort.VData.TPe = Convert.ToInt32(textBoxOrbitalTimeToPE.Text);
				KSPSerialPort.VData.TrueAnomaly = Convert.ToSingle(textBoxOrbitalTrueAnomaly.Text);
				KSPSerialPort.VData.period = Convert.ToInt32(textBoxOrbitalPeriod.Text);
				KSPSerialPort.VData.VVI = Convert.ToSingle(textBoxVerticalSpeed.Text);
				KSPSerialPort.VData.G = Convert.ToSingle(textBoxGForce.Text);
				KSPSerialPort.VData.Density = Convert.ToSingle(textBoxAtmDensity.Text);
				KSPSerialPort.VData.RAlt = Convert.ToSingle(textBoxRadarAlt.Text);
				KSPSerialPort.VData.Alt = Convert.ToSingle(textBoxAltitude.Text);
				KSPSerialPort.VData.Vsurf = Convert.ToSingle(textBoxSurfaceSpeed.Text);
				KSPSerialPort.VData.Lat = Convert.ToSingle(textBoxLatitude.Text);
				KSPSerialPort.VData.Lon = Convert.ToSingle(textBoxLongitude.Text);
				KSPSerialPort.VData.LiquidFuelTot = Convert.ToSingle(textBoxLiquidFuelMax.Text);
				KSPSerialPort.VData.LiquidFuel = Convert.ToSingle(textBoxLiquidFuelCurrent.Text);
				//I don't know what these are for
				//KSPSerialPort.VData.LiquidFuelTotS = (float)ProspectForResourceMax("LiquidFuel", ActiveEngines);
				//KSPSerialPort.VData.LiquidFuelS = (float)ProspectForResource("LiquidFuel", ActiveEngines);
				KSPSerialPort.VData.OxidizerTot = Convert.ToSingle(textBoxOxidizerMax.Text);
				KSPSerialPort.VData.Oxidizer = Convert.ToSingle(textBoxOxidizerCurrent.Text);
				//These either
				//KSPSerialPort.VData.OxidizerTotS = (float)ProspectForResourceMax("Oxidizer", ActiveEngines);
				//KSPSerialPort.VData.OxidizerS = (float)ProspectForResource("Oxidizer", ActiveEngines);
				KSPSerialPort.VData.EChargeTot = Convert.ToSingle(textBoxElectricalChargeMax.Text);
				KSPSerialPort.VData.ECharge = Convert.ToSingle(textBoxElectricalChargeCurrent.Text);
				KSPSerialPort.VData.MonoPropTot = Convert.ToSingle(textBoxMonoPropMax.Text);
				KSPSerialPort.VData.MonoProp = Convert.ToSingle(textBoxMonoPropCurrent.Text);
				KSPSerialPort.VData.IntakeAirTot = Convert.ToSingle(textBoxIntakeAirMax.Text);
				KSPSerialPort.VData.IntakeAir = Convert.ToSingle(textBoxIntakeAirCurrent.Text);
				KSPSerialPort.VData.SolidFuelTot = Convert.ToSingle(textBoxSolidFuelMax.Text);
				KSPSerialPort.VData.SolidFuel = Convert.ToSingle(textBoxSolidFuelCurrent.Text);
				KSPSerialPort.VData.XenonGasTot = Convert.ToSingle(textBoxXenonGasMax.Text);
				KSPSerialPort.VData.XenonGas = Convert.ToSingle(textBoxXenonGasCurrent.Text);
				KSPSerialPort.VData.MissionTime = Convert.ToUInt32(textBoxMissionTime.Text);
				KSPSerialPort.VData.deltaTime = Convert.ToSingle(textBoxDeltaTime.Text);
				KSPSerialPort.VData.VOrbit = Convert.ToSingle(textBoxOrbitalVelocity.Text);
				KSPSerialPort.VData.Roll = Convert.ToSingle(textBoxRoll.Text);
				KSPSerialPort.VData.Pitch = Convert.ToSingle(textBoxPitch.Text);
				KSPSerialPort.VData.Heading = Convert.ToSingle(textBoxHeading.Text);
				KSPSerialPort.ControlStatus((int) enumAG.RCS, checkBoxRCS.Checked);
				KSPSerialPort.ControlStatus((int) enumAG.SAS, checkBoxSAS.Checked);
				KSPSerialPort.ControlStatus((int) enumAG.Light, checkBoxLights.Checked);
				KSPSerialPort.ControlStatus((int) enumAG.Gear, checkBoxGear.Checked);
				KSPSerialPort.ControlStatus((int) enumAG.Brakes, checkBoxBrakes.Checked);
				KSPSerialPort.ControlStatus((int) enumAG.Abort, checkBoxAbort.Checked);
				KSPSerialPort.ControlStatus((int) enumAG.Custom01, checkBoxCG1.Checked);
				KSPSerialPort.ControlStatus((int) enumAG.Custom02, checkBoxCG2.Checked);
				KSPSerialPort.ControlStatus((int) enumAG.Custom03, checkBoxCG3.Checked);
				KSPSerialPort.ControlStatus((int) enumAG.Custom04, checkBoxCG4.Checked);
				KSPSerialPort.ControlStatus((int) enumAG.Custom05, checkBoxCG5.Checked);
				KSPSerialPort.ControlStatus((int) enumAG.Custom06, checkBoxCG6.Checked);
				KSPSerialPort.ControlStatus((int) enumAG.Custom07, checkBoxCG7.Checked);
				KSPSerialPort.ControlStatus((int) enumAG.Custom08, checkBoxCG8.Checked);
				KSPSerialPort.ControlStatus((int) enumAG.Custom09, checkBoxCG9.Checked);
				KSPSerialPort.ControlStatus((int) enumAG.Custom10, checkBoxCG10.Checked);
				KSPSerialPort.VData.SOINumber = Convert.ToByte(textBoxSOI.Text);
				KSPSerialPort.VData.MaxOverHeat = Convert.ToByte(textBoxMaxOverheat.Text);
				KSPSerialPort.VData.MachNumber = Convert.ToSingle(textBoxMachNumber.Text);
				KSPSerialPort.VData.IAS = Convert.ToSingle(textBoxIndicatedAirSpeed.Text);
				KSPSerialPort.VData.CurrentStage = Convert.ToByte(textBoxCurrentStage.Text);
				KSPSerialPort.VData.TotalStage = Convert.ToByte(textBoxStageCount.Text);

				KSPSerialPort.sendPacket(KSPSerialPort.VData);

				#endregion


				#region inputs
				if (KSPSerialPort.ControlReceived)
				{
					checkBoxInputRCS.Checked = KSPSerialPort.VControls.RCS;
					checkBoxInputSAS.Checked = KSPSerialPort.VControls.SAS;
					checkBoxInputLights.Checked = KSPSerialPort.VControls.Lights;
					checkBoxInputGear.Checked = KSPSerialPort.VControls.Gear;
					checkBoxInputBrakes.Checked = KSPSerialPort.VControls.Brakes;
					checkBoxInputPrecision.Checked = KSPSerialPort.VControls.Precision;
					checkBoxInputAbort.Checked = KSPSerialPort.VControls.Abort;
					checkBoxInputStage.Checked = KSPSerialPort.VControls.Stage;

					//================ control groups

					checkBoxInputCG1.Checked = KSPSerialPort.VControls.ControlGroup[1];
					checkBoxInputCG2.Checked = KSPSerialPort.VControls.ControlGroup[2];
					checkBoxInputCG3.Checked = KSPSerialPort.VControls.ControlGroup[3];
					checkBoxInputCG4.Checked = KSPSerialPort.VControls.ControlGroup[4];
					checkBoxInputCG5.Checked = KSPSerialPort.VControls.ControlGroup[5];
					checkBoxInputCG6.Checked = KSPSerialPort.VControls.ControlGroup[6];
					checkBoxInputCG7.Checked = KSPSerialPort.VControls.ControlGroup[7];
					checkBoxInputCG8.Checked = KSPSerialPort.VControls.ControlGroup[8];
					checkBoxInputCG9.Checked = KSPSerialPort.VControls.ControlGroup[9];
					checkBoxInputCG10.Checked = KSPSerialPort.VControls.ControlGroup[10];

					textBoxInputMode.Text = KSPSerialPort.VControls.Mode.ToString();
					textBoxInputPitch.Text = KSPSerialPort.VControls.Pitch.ToString(CultureInfo.InvariantCulture);
					textBoxInputRoll.Text = KSPSerialPort.VControls.Roll.ToString(CultureInfo.InvariantCulture);
					textBoxInputYaw.Text = KSPSerialPort.VControls.Yaw.ToString(CultureInfo.InvariantCulture);

					textBoxInputTX.Text = KSPSerialPort.VControls.TX.ToString(CultureInfo.InvariantCulture);
					textBoxInputTY.Text = KSPSerialPort.VControls.TY.ToString(CultureInfo.InvariantCulture);
					textBoxInputTZ.Text = KSPSerialPort.VControls.TZ.ToString(CultureInfo.InvariantCulture);

					textBoxInputWheelSteer.Text = KSPSerialPort.VControls.WheelSteer.ToString(CultureInfo.InvariantCulture);
					textBoxInputThrottle.Text = KSPSerialPort.VControls.Throttle.ToString(CultureInfo.InvariantCulture);
					textBoxInputWheelThrottle.Text = KSPSerialPort.VControls.WheelThrottle.ToString(CultureInfo.InvariantCulture);


					KSPSerialPort.ControlReceived = false;
				} //end ControlReceived
				#endregion

				_updatingControls = false;

			}


		}

		void Stop()
		{
			if (KSPSerialPort.Port.IsOpen)
			{
				KSPSerialPort.Port.Close();
				Debug.PostScreenMessage("Port closed");
			}

			buttonStart.Enabled = true;
			buttonStop.Enabled =
					buttonUpdate.Enabled = buttonUpdateOnce.Enabled = buttonHandshake.Enabled = buttonHandshakeOnce.Enabled = false;

		}

		private void buttonStart_Click(object sender, EventArgs e)
		{
			Start();
		}

		private bool _autoUpdate;
		private Timer _udTimer;
		private void buttonUpdate_Click(object sender, EventArgs e)
		{
			if (!_autoUpdate)
			{
				_autoUpdate = true;
				buttonUpdate.Text = @"Stop U/D";

				_udTimer = new Timer {Interval = 500};
				_udTimer.Tick += (s, a) =>
				{
					UpdateControls();
				};
				_udTimer.Enabled = true;
			}

			else
			{
				_autoUpdate = false;
				buttonUpdate.Text = @"Start U/D";

				_udTimer.Enabled = false;
				_udTimer.Dispose();
			}
			
		}

		private bool _autoHandshake;
		private Timer _hsTimer;
		private void buttonHandshake_Click(object sender, EventArgs e)
		{
			if (!_autoHandshake)
			{
				_autoHandshake = true;
				buttonHandshake.Text = @"Stop H/S";

				_hsTimer = new Timer {Interval = 1500};
				_hsTimer.Tick += (s, a) =>
				{
					KSPSerialPort.DoHandshake();
				};
				_hsTimer.Enabled = true;
			}

			else
			{
				_autoHandshake = false;
				buttonHandshake.Text = @"Start H/S";

				_hsTimer.Enabled = false;
				_hsTimer.Dispose();
			}
		}

		private void buttonSaveDefaults_Click(object sender, EventArgs e)
		{
			DialogResult dr = MessageBox.Show(@"Overwrite defaults ? You can reset defaults anytime.", @"Overwrite default",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (dr != DialogResult.Yes)
				return;

			Settings.Default.Save();
		}

		private void buttonResetDefaults_Click(object sender, EventArgs e)
		{
			DialogResult dr = MessageBox.Show(@"Reset defaults ? This action cannot be undone!", @"Reset to default",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (dr != DialogResult.Yes)
				return;

			Settings.Default.Reset();
			Settings.Default.Reload();
		}

		private void linkLabelKSPSerialIODebugToolRepo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("https://github.com/bolwire/KSPSerialIODebugTool");
		}

		private void linkLabelKSPSerialIODebugToolThread_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("http://forum.kerbalspaceprogram.com/index.php?/topic/139842-wiphardware-plugin-software-a-debugging-tool-for-kspserialio/");
		}

		private void linkLabelKSPSerialIOThread_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("http://forum.kerbalspaceprogram.com/index.php?/topic/60281-hardware-plugin-arduino-based-physical-display-serial-port-io-tutorial-22-april/&page=1");
		}

		private void buttonStop_Click(object sender, EventArgs e)
		{
			//Stop U/D & H/S if needed
			if (_autoUpdate)
				buttonUpdate_Click(sender, e);

			if (_autoHandshake)
				buttonHandshake_Click(sender, e);

			//Let any current communications finish before closing the port
			while (true)
			{
				if (_updatingControls || KSPSerialPort.Handshaking)
					continue;

				Stop();
				break;
			}
		}

		private void buttonUpdateOnce_Click(object sender, EventArgs e)
		{
			UpdateControls();
		}

		private void buttonHandshakeOnce_Click(object sender, EventArgs e)
		{
			KSPSerialPort.DoHandshake();
		}

		private void buttonShowAboutForm_Click(object sender, EventArgs e)
		{
			AboutForm aboutForm = new AboutForm();
			aboutForm.ShowDialog();
		}

		private void checkBoxShowStatusMessages_Click(object sender, EventArgs e)
		{
			while (true)
			{
				if (checkBoxShowStatusMessages.Checked)
				{
					if (MessageForm.IsDisposed)
					{
						MessageForm = new StatusMessagesForm();
						MessageForm.Show();
					}
					else
					{
						MessageForm.Close();
						continue;
					}
				}
				else
				{
					if (!MessageForm.IsDisposed)
					{
						MessageForm.Close();
					}
				}
				break;
			}
		}
	}
}
