using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using NAudio.CoreAudioApi;
using NAudio.Lame;
using NAudio.Wave;
using SkypeMp3Recorder.SkypeWatcher.Model;
using SkypeMp3Recorder.SkypeWatcher.Model.Base;
using SkypeMp3Recorder.Util;

namespace SkypeMp3Recorder.Recorder
{
    public class SkypeRecorder {
        internal static readonly ILog log = LogManager.GetLogger("SkypeRecorder");

        public bool IsRecording { get; private set; }
        public SkypeCall Call { get; set; }
        public SkypeInstance Instance { get; set; }

        private string _spkSourceFile;
        private WasapiCapture spkSource = null;
        private WaveFileWriter spkWaveFile = null;

        private string _micSourceFile;
        private WasapiCapture micSource = null;
        private WaveFileWriter micWaveFile = null;

        private object _writeLock = new object();

        public void StartRecording() {
            if (IsRecording)
                return;

            try {
                log.Debug("Starting recording...");

                log.Debug($"  checking if save path {SettingsFile.Instance.SavePath} exists...");
                if (!Directory.Exists(SettingsFile.Instance.SavePath))
                {
                    Directory.CreateDirectory(SettingsFile.Instance.SavePath);
                }

                var speakerId = SettingsFile.Instance.DefaultSpeakers; //TODO: check skype profile and find matching
                if (String.IsNullOrEmpty(speakerId)) {
                    speakerId = DeviceHelper.GetDefaultSpeaker();
                }

                var microphoneId = SettingsFile.Instance.DefaultMicrophone;
                if (String.IsNullOrEmpty(microphoneId)) {
                    microphoneId = DeviceHelper.GetDefaultMicrophone();
                }

                log.Debug($"Using devices: mic = {microphoneId}, spkr = {speakerId}");

                if (!String.IsNullOrEmpty(speakerId)) 
                    spkSource = new WasapiLoopbackCapture(DeviceHelper.GetDeviceById(speakerId));
                else 
                    spkSource = new WasapiLoopbackCapture();

                if(!String.IsNullOrEmpty(microphoneId))
                    micSource = new WasapiCapture(DeviceHelper.GetDeviceById(microphoneId));
                else
                    micSource = new WasapiCapture();

                //spkSource.WaveFormat = new WaveFormat(44100, 1);
                spkSource.DataAvailable += spkSource_DataAvailable;
                spkSource.RecordingStopped += spkSource_RecordingStopped;
                //micSource.WaveFormat = new WaveFormat(44100, 1);
                micSource.DataAvailable += micSource_DataAvailable;
                micSource.RecordingStopped += micSource_RecordingStopped;

                var guid = Guid.NewGuid();
                _micSourceFile = Path.Combine(Path.GetTempPath(), $"{guid}_in.wav");
                micWaveFile = new WaveFileWriter(_micSourceFile, micSource.WaveFormat);
                _spkSourceFile = Path.Combine(Path.GetTempPath(), $"{guid}_out.wav");
                spkWaveFile = new WaveFileWriter(_spkSourceFile, micSource.WaveFormat);

                log.Debug($"Using temp files: {_micSourceFile}, {_spkSourceFile}");

                micSource.StartRecording();
                spkSource.StartRecording();

                log.Debug($"Started recording...");

                IsRecording = true;
            }
            catch (Exception ex) {
                log.Error("Error starting capture: " + ex.Message, ex);
            }
        }

        void micSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (micWaveFile != null)
            {
                micWaveFile.Write(e.Buffer, 0, e.BytesRecorded);
                micWaveFile.Flush();
            }
        }
        void spkSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (spkWaveFile != null)
            {
                spkWaveFile.Write(e.Buffer, 0, e.BytesRecorded);
                spkWaveFile.Flush();
            }
        }

        void micSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (micSource != null)
            {
                micSource.Dispose();
                micSource = null;
            }

            if (micWaveFile != null)
            {
                micWaveFile.Dispose();
                micWaveFile = null;
            }

            lock (_writeLock) {
                convertToMp3();
                IsRecording = false;
            }
        }

        void spkSource_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (spkSource != null)
            {
                spkSource.Dispose();
                spkSource = null;
            }

            if (spkWaveFile != null)
            {
                spkWaveFile.Dispose();
                spkWaveFile = null;
            }

            lock (_writeLock)
            {
                convertToMp3();
                IsRecording = false;
            }
        }

        private void addMixerStream(WaveMixerStream32 mixer, string fileName) {
            // create a wave stream and a channel object
            var reader = new WaveFileReader(fileName);
            var channel = new WaveChannel32(reader)
            {
                //Set the volume
                Volume = 1.0f
            };

            mixer.AddInputStream(channel);
        }

        private string buildFileName(int i) {
            string postfix = i > 0 ? $"_{i}" : "";
            return $"{Call.Started:yyyy-MM-dd} time {Call.Started:HH_mm}_{(int) Call.Duration.TotalSeconds}{postfix}.mp3";
        }

        private void convertToMp3() {
            if (IsRecording)
                return;

            int i = 0;
            var outputFile = Path.Combine(SettingsFile.Instance.SavePath, buildFileName(i));
            while (File.Exists(outputFile)) {
                outputFile = Path.Combine(SettingsFile.Instance.SavePath, buildFileName(++i));
            }

            try {
                log.Debug($"Generating mp3: {outputFile}");

                var tag = new ID3TagData();
                tag.UserDefinedText.Add("CallId", Call.CallId);
                tag.UserDefinedText.Add("From", Call.From?.SkypeId);
                tag.UserDefinedText.Add("FromDisplayName", Call.From?.DisplayName);
                tag.UserDefinedText.Add("To", Call.To?.SkypeId);
                tag.UserDefinedText.Add("ToDisplayName", Call.To?.DisplayName);

                var mixer = new WaveMixerStream32 {
                    AutoStop = true
                };

                log.Debug($"  adding wave input: {_micSourceFile}");
                addMixerStream(mixer, _micSourceFile);
                log.Debug($"  adding wave input: {_spkSourceFile}");
                addMixerStream(mixer, _spkSourceFile);

                log.Debug($"  encoding");
                var wave32 = new Wave32To16Stream(mixer);
                var mp3Writer = new LameMP3FileWriter(outputFile, wave32.WaveFormat, LAMEPreset.VBR_90, tag);
                wave32.CopyTo(mp3Writer);

                // close all streams
                wave32.Close();
                mp3Writer.Close();

                log.Debug($"  finished, removing temp files");

                File.Delete(_micSourceFile);
                File.Delete(_spkSourceFile);
            }
            catch (Exception ex) {
                log.Error("Error generating mp3: " + ex.Message, ex);
            }
        }

        public void StopRecording() {
            if (!IsRecording)
                return;

            Call.Duration = DateTime.Now.Subtract(Call.Started);

            log.Debug($"Finishing recording, call duration = {Call.Duration}");

            micSource.StopRecording();
            spkSource.StopRecording();
        }
    }
}
