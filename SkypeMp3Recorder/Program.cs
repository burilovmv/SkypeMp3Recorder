using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Microsoft.Win32;
using SkypeMp3Recorder.Properties;
using SkypeMp3Recorder.Recorder;
using SkypeMp3Recorder.SkypeWatcher.Finder;
using SkypeMp3Recorder.SkypeWatcher.Model;
using SkypeMp3Recorder.SkypeWatcher.Model.Base;

namespace SkypeMp3Recorder
{
    static class Program
    {
        internal static readonly ILog log = LogManager.GetLogger("App logger");

        private static Mutex _mutex;

        /// <summary>
        /// Tray icon
        /// </summary>
        private static NotifyIcon _trayIcon;

        private static AboutForm _aboutForm;
        private static SettingsForm _settingsForm;
        private static bool _running;
        private static SkypeInstance[] _skypeInstances;
        private static SkypeRecorder _recorder;

        #region logging
        /// <summary>
        /// Setups application logging
        /// <param name="fileName">Log file name</param>
        /// <param name="needDebug">If true, writes debug info to log file</param>
        /// </summary>
        public static void Log4Setup(string fileName, bool needDebug)
        {
            if (!LogManager.GetRepository().Configured)
            {
                Logger root = ((Hierarchy)LogManager.GetRepository()).Root;
                root.Level = needDebug ? Level.All : Level.Info;

                var fa = new RollingFileAppender();
                fa.Layout = new PatternLayout("%d{dd.MM.yyyy HH:mm:ss} (%logger) %-5p %m%n");
                fa.File = fileName;
                fa.ImmediateFlush = true;
                fa.AppendToFile = true;
                fa.RollingStyle = RollingFileAppender.RollingMode.Size;
                fa.MaxSizeRollBackups = 1;
                fa.MaximumFileSize = "10M";
                fa.ActivateOptions();
                root.AddAppender(fa);

                LogManager.GetRepository().Configured = true;
            }
        }
        #endregion

        private static void SetRunOnStartup() {
            using (var key =
                Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)) {
                if(key.GetValue("SkypeMP3Recorder") == null)
                    key.SetValue("SkypeMP3Recorder", Application.ExecutablePath);
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool firstInstance;
            _mutex = new Mutex(false, "Local\\ITS-SkypeMp3RecorderMutex", out firstInstance);
            if (!firstInstance)
            {
                return;
            }

            SetRunOnStartup();

            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\SkypeMp3Recorder";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ApplicationExit += Application_ApplicationExit;
            Application.ThreadException += Application_ThreadException;

            Log4Setup(
                Path.Combine(path, "recorder.log"),
                true);
            log.Info("Application started.");

            try {
                _skypeInstances = SkypeInstanceFinder.FindInstances();
                foreach (var instance in _skypeInstances) {
                    log.Info(
                        $"Found Skype instance, type={instance.ClientType}, path={instance.DataPath}, profiles={instance.Profiles.Length}, default mic={instance.DefaultMicrophone}, default speaker={instance.DefaultSpeakers}");
                    instance.CallChanged += Instance_CallChanged;
                    instance.StartWatching();
                }
            }
            catch (Exception ex) {
                log.Error("Error finding instances: " + ex.Message, ex);
            }

            _trayIcon = new NotifyIcon();
            _trayIcon.Icon = Resources.recorder;
            _trayIcon.Visible = true;
            _trayIcon.DoubleClick += SettingsClick;
            _trayIcon.ContextMenu = new ContextMenu();

            _trayIcon.ContextMenu.MenuItems.Add("About...", AboutClick);
            _trayIcon.ContextMenu.MenuItems.Add("Settings", SettingsClick);
            _trayIcon.ContextMenu.MenuItems.Add("Exit", ExitClick);

            _running = true;

            Application.Run();
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            log.Error("Unexpected exception", e.Exception);
        }

        private static void Instance_CallChanged(SkypeInstance instance, SkypeWatcher.Model.SkypeCall call, SkypeWatcher.Model.SkypeCallState state)
        {
            log.Info($"Call detected: {call.CallId}, {call.From} -> {call.To}, {state}");

            if (state == SkypeCallState.Started) {
                call.Started = DateTime.Now;

                if (_recorder == null ||!_recorder.IsRecording)
                {
                    _recorder = new SkypeRecorder();
                    _recorder.Instance = instance;
                    _recorder.Call = call;

                    _recorder.StartRecording();
                }

            } else if (state == SkypeCallState.Finished) {
                if (_recorder != null && _recorder.IsRecording) {
                    _recorder.StopRecording();
                    _recorder = null;
                }
            }
        }

        /// <summary>
        /// "About..." button pressed
        /// </summary>
        private static void AboutClick(object sender, EventArgs args)
        {
            if (_aboutForm == null || !_aboutForm.Visible)
            {
                _aboutForm = new AboutForm();
                _aboutForm.ShowDialog();
            }
            else
            {
                _aboutForm.Focus();
            }
        }

        /// <summary>
        /// "Settings" button pressed
        /// </summary>
        private static void SettingsClick(object sender, EventArgs args)
        {
            if (_settingsForm == null || !_settingsForm.Visible)
            {
                _settingsForm = new SettingsForm();

                if (_settingsForm.ShowDialog() == DialogResult.OK)
                {
                    //nothing
                }
            }
            else
            {
                _settingsForm.Focus();
            }
        }

        /// <summary>
        /// "Exit" button pressed
        /// </summary>
        private static void ExitClick(object sender, EventArgs args)
        {
            Application.Exit();
        }

        static void Application_ApplicationExit(object sender, EventArgs e)
        {
            _running = false;

            foreach (var instance in _skypeInstances)
            {
                instance.StopWatching();
            }

            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }

    }
}
