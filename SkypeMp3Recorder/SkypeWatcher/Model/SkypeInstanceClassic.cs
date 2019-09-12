using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using SkypeMp3Recorder.SkypeWatcher.Model.Base;

namespace SkypeMp3Recorder.SkypeWatcher.Model
{
    public class SkypeInstanceClassic : SkypeInstance {
        internal static readonly ILog log = LogManager.GetLogger("SkypeInstanceClassic");
        protected FileSystemWatcher[] _watchers;

        protected override void DetectAudioSettings() {
            if (Profiles == null || Profiles.Length == 0) {
                return;
            }

            var lastProfile = Profiles.OrderByDescending(x => x.LastUsed).FirstOrDefault();

            DefaultMicrophone = lastProfile.DefaultMicrophone;
            DefaultSpeakers = lastProfile.DefaultSpeakers;
        }

        private void DetectConfigFile(List<SkypeProfile> profiles, string path) {
            var files = Directory.GetFiles(path);
            foreach (var file in files) {
                if (Path.GetFileName(file).Equals(SkypeProfile.configFileName)) {
                    var fi = new FileInfo(Path.Combine(path, file));

                    var profile = new SkypeProfile()
                        {ProfilePath = path, User = new SkypeUser() { SkypeId = Path.GetFileName(path) }, LastUsed = fi.LastWriteTime};
                    profile.LoadSettings();

                    profiles.Add(profile);
                }
            }

            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs) {
                DetectConfigFile(profiles, dir);
            }
        }

        protected override void DetectProfiles() {
            var profiles = new List<SkypeProfile>();

            DetectConfigFile(profiles, DataPath);

            Profiles = profiles.ToArray();
        }

        protected override void DetectNewCalls(string path) {
        }

        protected virtual string GetDatabaseFilter() {
            return "main.db";
        }

        protected virtual void StartWatchingInternal() {
            string filter = GetDatabaseFilter();

            log.Debug($"Started watching for {DataPath}\\{filter} changes");

            var watchers = new List<FileSystemWatcher>();

            var watcher = new FileSystemWatcher(DataPath, filter);
            watcher.Changed += SkypeInstanceClassic_DatabaseChanged;
            watcher.EnableRaisingEvents = true;

            watchers.Add(watcher);
            foreach (var profile in Profiles) {
                log.Debug($"Started watching for {profile.ProfilePath}\\{filter} changes");

                watcher = new FileSystemWatcher(profile.ProfilePath, filter);
                watcher.Changed += SkypeInstanceClassic_DatabaseChanged;
                watcher.EnableRaisingEvents = true;

                watchers.Add(watcher);
            }

            _watchers = watchers.ToArray();
        }

        private void SkypeInstanceClassic_DatabaseChanged(object sender, FileSystemEventArgs e)
        {
            DetectNewCalls(e.FullPath);
        }

        public override void StartWatching() {
            log.Debug("Going to start watching, _watching=" + _watching);
            if (_watching)
                return;
            _watching = true;

            /*if (Profiles == null || Profiles.Length == 0) {
                return;
            }*/

            StartWatchingInternal();
        }

        public override void StopWatching() {
            if (!_watching)
                return;
            _watching = false;

            if (_watchers == null || _watchers.Length == 0)
                return;

            foreach (var watcher in _watchers) {
                log.Debug($"Finished watching for {watcher.Path} changes");

                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }

            _watchers = null;
        }
    }
}
