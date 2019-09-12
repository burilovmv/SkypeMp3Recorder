namespace SkypeMp3Recorder
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmbSpkrs = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkUseSkypeSpk = new System.Windows.Forms.CheckBox();
            this.cmbMics = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkUseSkypeMic = new System.Windows.Forms.CheckBox();
            this.btnChooseFolder = new System.Windows.Forms.Button();
            this.txtRecordingPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmbSpkrs);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.chkUseSkypeSpk);
            this.groupBox1.Controls.Add(this.cmbMics);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.chkUseSkypeMic);
            this.groupBox1.Controls.Add(this.btnChooseFolder);
            this.groupBox1.Controls.Add(this.txtRecordingPath);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(601, 181);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Recording settings";
            // 
            // cmbSpkrs
            // 
            this.cmbSpkrs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSpkrs.FormattingEnabled = true;
            this.cmbSpkrs.Location = new System.Drawing.Point(117, 135);
            this.cmbSpkrs.Name = "cmbSpkrs";
            this.cmbSpkrs.Size = new System.Drawing.Size(446, 21);
            this.cmbSpkrs.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(56, 138);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Speakers:";
            // 
            // chkUseSkypeSpk
            // 
            this.chkUseSkypeSpk.AutoSize = true;
            this.chkUseSkypeSpk.Location = new System.Drawing.Point(117, 112);
            this.chkUseSkypeSpk.Name = "chkUseSkypeSpk";
            this.chkUseSkypeSpk.Size = new System.Drawing.Size(178, 17);
            this.chkUseSkypeSpk.TabIndex = 6;
            this.chkUseSkypeSpk.Text = "Use speakers from Skype profile";
            this.chkUseSkypeSpk.UseVisualStyleBackColor = true;
            this.chkUseSkypeSpk.CheckedChanged += new System.EventHandler(this.chkUseSkypeSpk_CheckedChanged);
            // 
            // cmbMics
            // 
            this.cmbMics.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMics.FormattingEnabled = true;
            this.cmbMics.Location = new System.Drawing.Point(117, 79);
            this.cmbMics.Name = "cmbMics";
            this.cmbMics.Size = new System.Drawing.Size(446, 21);
            this.cmbMics.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(45, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Microphone:";
            // 
            // chkUseSkypeMic
            // 
            this.chkUseSkypeMic.AutoSize = true;
            this.chkUseSkypeMic.Location = new System.Drawing.Point(117, 56);
            this.chkUseSkypeMic.Name = "chkUseSkypeMic";
            this.chkUseSkypeMic.Size = new System.Drawing.Size(190, 17);
            this.chkUseSkypeMic.TabIndex = 3;
            this.chkUseSkypeMic.Text = "Use microphone from Skype profile";
            this.chkUseSkypeMic.UseVisualStyleBackColor = true;
            this.chkUseSkypeMic.CheckedChanged += new System.EventHandler(this.chkUseSkypeMic_CheckedChanged);
            // 
            // btnChooseFolder
            // 
            this.btnChooseFolder.Location = new System.Drawing.Point(569, 23);
            this.btnChooseFolder.Name = "btnChooseFolder";
            this.btnChooseFolder.Size = new System.Drawing.Size(24, 23);
            this.btnChooseFolder.TabIndex = 2;
            this.btnChooseFolder.Text = "...";
            this.btnChooseFolder.UseVisualStyleBackColor = true;
            // 
            // txtRecordingPath
            // 
            this.txtRecordingPath.Location = new System.Drawing.Point(117, 23);
            this.txtRecordingPath.Name = "txtRecordingPath";
            this.txtRecordingPath.Size = new System.Drawing.Size(446, 20);
            this.txtRecordingPath.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Path to recordings:";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(12, 199);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(93, 199);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(620, 234);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsForm";
            this.Text = "Skype Recorder settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbSpkrs;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkUseSkypeSpk;
        private System.Windows.Forms.ComboBox cmbMics;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkUseSkypeMic;
        private System.Windows.Forms.Button btnChooseFolder;
        private System.Windows.Forms.TextBox txtRecordingPath;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}