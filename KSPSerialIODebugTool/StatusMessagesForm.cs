using System.Windows.Forms;

namespace KSPSerialIODebugTool
{

	public partial class StatusMessagesForm : Form
	{
		public StatusMessagesForm()
		{
			InitializeComponent();
			Debug.NewLogMessage += LogMessage;
			Debug.NewPrintMessage += PrintMessage;
			Debug.NewPostScreenMessage += PostScreenMessage;
		}

		private void LogMessage(string s)
		{
			if (richTextBoxMessages.InvokeRequired)
			{
				richTextBoxMessages.BeginInvoke(new Debug.LogMessageEventHandler(LogMessage), s);
			}
			else
			{
				richTextBoxMessages.AppendText(s + "\n");
			}
		}

		private void PrintMessage(string s)
		{
			if (richTextBoxMessages.InvokeRequired)
			{
				richTextBoxMessages.BeginInvoke(new Debug.PrintEventHandler(PrintMessage), s);
			}
			else
			{
				richTextBoxMessages.AppendText(s + "\n");
			}
		}

		private void PostScreenMessage(string s)
		{
			if (richTextBoxMessages.InvokeRequired)
			{
				richTextBoxMessages.BeginInvoke(new Debug.PostScreenMessageEventHandler(PostScreenMessage), s);
			}
			else
			{
				richTextBoxMessages.AppendText(s + "\n");
			}
		}

		private void StatusMessagesForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			Debug.NewLogMessage -= LogMessage;
			Debug.NewPrintMessage -= PrintMessage;
			Debug.NewPostScreenMessage -= PostScreenMessage;
		}
	}
}
