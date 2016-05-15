namespace KSPSerialIODebugTool
{
	partial class StatusMessagesForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.richTextBoxMessages = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// richTextBoxMessages
			// 
			this.richTextBoxMessages.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBoxMessages.Location = new System.Drawing.Point(0, 0);
			this.richTextBoxMessages.Name = "richTextBoxMessages";
			this.richTextBoxMessages.Size = new System.Drawing.Size(425, 289);
			this.richTextBoxMessages.TabIndex = 0;
			this.richTextBoxMessages.Text = "";
			// 
			// StatusMessagesForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(425, 289);
			this.Controls.Add(this.richTextBoxMessages);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "StatusMessagesForm";
			this.Text = "KSPSerialIO Debug Tool: Status Messages";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StatusMessagesForm_FormClosing);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox richTextBoxMessages;
	}
}