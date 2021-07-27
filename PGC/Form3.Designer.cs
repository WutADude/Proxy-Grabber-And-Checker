
namespace PGC
{
    partial class Form3
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form3));
            this.headersAndValuesRichTextBox = new System.Windows.Forms.RichTextBox();
            this.applyHeadersButton = new MetroFramework.Controls.MetroButton();
            this.infoLabel = new MetroFramework.Controls.MetroLabel();
            this.SuspendLayout();
            // 
            // headersAndValuesRichTextBox
            // 
            this.headersAndValuesRichTextBox.AcceptsTab = true;
            this.headersAndValuesRichTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.headersAndValuesRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.headersAndValuesRichTextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(86)))), ((int)(((byte)(86)))), ((int)(((byte)(86)))));
            this.headersAndValuesRichTextBox.Location = new System.Drawing.Point(23, 63);
            this.headersAndValuesRichTextBox.Name = "headersAndValuesRichTextBox";
            this.headersAndValuesRichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.headersAndValuesRichTextBox.Size = new System.Drawing.Size(448, 218);
            this.headersAndValuesRichTextBox.TabIndex = 1;
            this.headersAndValuesRichTextBox.Text = resources.GetString("headersAndValuesRichTextBox.Text");
            this.headersAndValuesRichTextBox.Click += new System.EventHandler(this.richTextBox1_Click);
            this.headersAndValuesRichTextBox.Leave += new System.EventHandler(this.richTextBox1_Leave);
            // 
            // applyHeadersButton
            // 
            this.applyHeadersButton.BackColor = System.Drawing.Color.Transparent;
            this.applyHeadersButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.applyHeadersButton.Cursor = System.Windows.Forms.Cursors.Default;
            this.applyHeadersButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(86)))), ((int)(((byte)(86)))), ((int)(((byte)(86)))));
            this.applyHeadersButton.Location = new System.Drawing.Point(23, 287);
            this.applyHeadersButton.Name = "applyHeadersButton";
            this.applyHeadersButton.Size = new System.Drawing.Size(448, 33);
            this.applyHeadersButton.TabIndex = 8;
            this.applyHeadersButton.Text = "Сохранить заголовки";
            this.applyHeadersButton.UseCustomBackColor = true;
            this.applyHeadersButton.UseCustomForeColor = true;
            this.applyHeadersButton.UseSelectable = true;
            this.applyHeadersButton.Click += new System.EventHandler(this.metroButton3_Click);
            // 
            // infoLabel
            // 
            this.infoLabel.AutoSize = true;
            this.infoLabel.Cursor = System.Windows.Forms.Cursors.Help;
            this.infoLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(86)))), ((int)(((byte)(86)))), ((int)(((byte)(86)))));
            this.infoLabel.Location = new System.Drawing.Point(-1, 5);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(21, 19);
            this.infoLabel.TabIndex = 9;
            this.infoLabel.Text = "🛈";
            this.infoLabel.UseCustomForeColor = true;
            this.infoLabel.Click += new System.EventHandler(this.metroLabel1_Click);
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(494, 328);
            this.Controls.Add(this.infoLabel);
            this.Controls.Add(this.applyHeadersButton);
            this.Controls.Add(this.headersAndValuesRichTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form3";
            this.Resizable = false;
            this.ShadowType = MetroFramework.Forms.MetroFormShadowType.DropShadow;
            this.Style = MetroFramework.MetroColorStyle.Purple;
            this.Text = "Заголовки (Headders)";
            this.TextAlign = MetroFramework.Forms.MetroFormTextAlign.Center;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form3_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.RichTextBox headersAndValuesRichTextBox;
        private MetroFramework.Controls.MetroButton applyHeadersButton;
        private MetroFramework.Controls.MetroLabel infoLabel;
    }
}