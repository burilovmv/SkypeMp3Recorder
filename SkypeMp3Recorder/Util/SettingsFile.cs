using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace SkypeMp3Recorder.Util
{
    public class SettingsFile
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static SettingsFile _instance;

        /// <summary>
        /// Settings file name
        /// </summary>
        private static string _settingsFileName;

        /// <summary>
        /// Returns singleton instance
        /// </summary>
        public static SettingsFile Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SettingsFile();
                }
                return _instance;
            }
        }

        private SettingsFile()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\SkypeMp3Recorder";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            _settingsFileName = Path.Combine(path, "recorder.conf");
            Load();
        }

        /// <summary>
        /// Loads settings
        /// </summary>
        public void Load()
        {
            SavePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\SkypeMp3Recorder\\Recordings";
            UseSkypeMicrophone = true;
            UseSkypeSpeakers = true;

            if (!File.Exists(_settingsFileName))
            {
                return;
            }

            using (var reader = new StreamReader(_settingsFileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (String.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    if (line.TrimStart(' ').StartsWith("#"))
                    {
                        continue;
                    }

                    if (line.Contains("=")) {
                        var parts = line.Split('=');
                        if ("UseSkypeMicrophone".Equals(parts[0].Trim(), StringComparison.InvariantCultureIgnoreCase)) {
                            UseSkypeMicrophone = Boolean.Parse(parts[1].Trim());
                        }
                        else if ("DefaultMicrophone".Equals(parts[0].Trim(), StringComparison.InvariantCultureIgnoreCase)) {
                            try {
                                DefaultMicrophone = parts[1].Trim();
                            } catch(Exception) { /* ignore */}
                        }
                        else if ("UseSkypeSpeakers".Equals(parts[0].Trim(), StringComparison.InvariantCultureIgnoreCase)) {
                            UseSkypeSpeakers = Boolean.Parse(parts[1].Trim());
                        }
                        else if ("DefaultSpeakers".Equals(parts[0].Trim(), StringComparison.InvariantCultureIgnoreCase)) {
                            try
                            {
                                DefaultSpeakers = parts[1].Trim();
                            }
                            catch (Exception) { /* ignore */}
                        }
                        else if ("SavePath".Equals(parts[0].Trim(), StringComparison.InvariantCultureIgnoreCase)) {
                            SavePath = parts[1].Trim();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Saves settings
        /// </summary>
        public void Save()
        {
            try
            {
                using (var writer = new StreamWriter(_settingsFileName, false))
                {
                    writer.WriteLine("# SkypeMp3Recorder v." + Assembly.GetExecutingAssembly().GetName().Version + " settings file.");
                    writer.WriteLine("# Copyright © IT Service Plus 2019");
                    writer.WriteLine();
                    writer.WriteLine("[Main]");
                    writer.WriteLine($"UseSkypeMicrophone={UseSkypeMicrophone}");
                    writer.WriteLine($"DefaultMicrophone={DefaultMicrophone}");
                    writer.WriteLine($"UseSkypeSpeakers={UseSkypeSpeakers}");
                    writer.WriteLine($"DefaultSpeakers={DefaultSpeakers}");
                    writer.WriteLine($"SavePath={SavePath}");

                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Error saving UI settings: " + ex.Message);
            }
        }

        public bool UseSkypeMicrophone { get; set; }
        public bool UseSkypeSpeakers { get; set; }
        public string DefaultMicrophone { get; set; }
        public string DefaultSpeakers { get; set; }
        public string SavePath { get; set; }
    }
}
