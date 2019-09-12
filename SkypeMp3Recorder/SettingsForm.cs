using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using SkypeMp3Recorder.Util;

namespace SkypeMp3Recorder
{
    public partial class SettingsForm : Form {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e) {
            txtRecordingPath.Text = SettingsFile.Instance.SavePath;

            try {
                cmbMics.Items.AddRange(DeviceHelper.GetInputDevices());
            }
            catch (Exception ex) {
                MessageBox.Show("Error reading microphone list: " + ex.Message);
            }

            try {
                cmbSpkrs.Items.AddRange(DeviceHelper.GetOutputDevices());
            }
            catch (Exception ex) {
                MessageBox.Show("Error reading speakers list: " + ex.Message);
            }

            foreach (var micsItem in cmbMics.Items) {
                if (((AudioDevice) micsItem).Id.Equals(SettingsFile.Instance.DefaultMicrophone)) {
                    cmbMics.SelectedItem = micsItem;
                    break;
                }
            }
            foreach (var spkrItem in cmbSpkrs.Items) {
                if (((AudioDevice) spkrItem).Id.Equals(SettingsFile.Instance.DefaultSpeakers)) {
                    cmbSpkrs.SelectedItem = spkrItem;
                    break;
                }
            }

            chkUseSkypeMic.Checked = SettingsFile.Instance.UseSkypeMicrophone;
            chkUseSkypeSpk.Checked = SettingsFile.Instance.UseSkypeSpeakers;
        }


        private void chkUseSkypeMic_CheckedChanged(object sender, EventArgs e) {
            cmbMics.Enabled = !chkUseSkypeMic.Checked;
        }

        private void chkUseSkypeSpk_CheckedChanged(object sender, EventArgs e) {
            cmbSpkrs.Enabled = !chkUseSkypeSpk.Checked;
        }

        private void btnSave_Click(object sender, EventArgs e) {
            SettingsFile.Instance.SavePath = txtRecordingPath.Text;
            SettingsFile.Instance.UseSkypeMicrophone = chkUseSkypeMic.Checked;
            SettingsFile.Instance.UseSkypeSpeakers = chkUseSkypeSpk.Checked;
            SettingsFile.Instance.DefaultMicrophone = ((AudioDevice) cmbMics.SelectedItem)?.Id;
            SettingsFile.Instance.DefaultSpeakers = ((AudioDevice) cmbSpkrs.SelectedItem)?.Id;

            SettingsFile.Instance.Save();
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            Close();
        }
    }
}
