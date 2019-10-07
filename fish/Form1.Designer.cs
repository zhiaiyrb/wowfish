namespace fish
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.button1 = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.button2 = new System.Windows.Forms.Button();
            this.comboWasapiDevices = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(90, 123);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(186, 81);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(378, 36);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(9);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(2230, 1342);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(51, 1317);
            this.button2.Margin = new System.Windows.Forms.Padding(9);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(225, 69);
            this.button2.TabIndex = 2;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // comboWasapiDevices
            // 
            this.comboWasapiDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboWasapiDevices.FormattingEnabled = true;
            this.comboWasapiDevices.Location = new System.Drawing.Point(18, 271);
            this.comboWasapiDevices.Margin = new System.Windows.Forms.Padding(9, 8, 9, 8);
            this.comboWasapiDevices.Name = "comboWasapiDevices";
            this.comboWasapiDevices.Size = new System.Drawing.Size(355, 44);
            this.comboWasapiDevices.TabIndex = 15;
            this.comboWasapiDevices.SelectedIndexChanged += new System.EventHandler(this.comboWasapiLoopbackDevices_SelectedIndexChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(18F, 36F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(2913, 1548);
            this.Controls.Add(this.comboWasapiDevices);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load_1);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ComboBox comboWasapiDevices;
    }
}

