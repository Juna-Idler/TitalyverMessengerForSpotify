﻿
namespace TitalyverMessengerForSpotify
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ButtonGetCurrentPlaying = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label_Countdown = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // ButtonGetCurrentPlaying
            // 
            this.ButtonGetCurrentPlaying.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonGetCurrentPlaying.Location = new System.Drawing.Point(12, 165);
            this.ButtonGetCurrentPlaying.Name = "ButtonGetCurrentPlaying";
            this.ButtonGetCurrentPlaying.Size = new System.Drawing.Size(215, 58);
            this.ButtonGetCurrentPlaying.TabIndex = 0;
            this.ButtonGetCurrentPlaying.Text = "Get Current Playing";
            this.ButtonGetCurrentPlaying.UseVisualStyleBackColor = true;
            this.ButtonGetCurrentPlaying.Click += new System.EventHandler(this.GetCurrentPlaying_Click);
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(12, 12);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(431, 147);
            this.textBox1.TabIndex = 1;
            // 
            // label_Countdown
            // 
            this.label_Countdown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label_Countdown.AutoSize = true;
            this.label_Countdown.Location = new System.Drawing.Point(375, 208);
            this.label_Countdown.Name = "label_Countdown";
            this.label_Countdown.Size = new System.Drawing.Size(68, 15);
            this.label_Countdown.TabIndex = 2;
            this.label_Countdown.Text = "Next:30.000";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(455, 235);
            this.Controls.Add(this.label_Countdown);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.ButtonGetCurrentPlaying);
            this.Name = "Form1";
            this.Text = "TitalyverMessengerForSpotify";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ButtonGetCurrentPlaying;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label_Countdown;
        private System.Windows.Forms.Timer timer1;
    }
}

