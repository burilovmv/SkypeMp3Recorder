using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using log4net;
using SkypeMp3Recorder.Extensions;
using SkypeMp3Recorder.SkypeWatcher.Model.Base;

namespace SkypeMp3Recorder.SkypeWatcher.Model
{
    public class SkypeInstanceDesktop : SkypeInstanceClassic {
        internal static readonly ILog log = LogManager.GetLogger("SkypeInstanceDesktop");
        protected string[] _databaseFolders;
        private Dictionary<string, long> _lastOffset;
        private Dictionary<string, Thread> _readers;

        protected virtual void DetectDatabaseFolders() {
            try {
                log.Debug("Detecting folders in " + Path.Combine(DataPath, "IndexedDB"));
                var folders = new List<string>();

                //folders.AddRange(Directory.GetDirectories(Path.Combine(DataPath, "Local Storage")));
                folders.AddRange(Directory.GetDirectories(Path.Combine(DataPath, "IndexedDB")));

                log.Debug("Folders found: " + folders.Count);

                _databaseFolders = folders.ToArray();
            }
            catch (Exception ex) {
                log.Error("WEIRD: Skype for Desktop database not found!", ex);
            }
        }

        public override void Initialize() {
            base.Initialize();

            _readers = new Dictionary<string, Thread>();
            DetectDatabaseFolders();
        }

        protected override void StartWatchingInternal() {
            log.Debug($"StartWatchingInternal folders=" + _databaseFolders?.Length);

            if (_databaseFolders == null || _databaseFolders.Length == 0)
                return;

            _lastOffset = new Dictionary<string, long>();

            _watchers = new FileSystemWatcher[_databaseFolders.Length];
            for (int i = 0; i < _databaseFolders.Length; i++) {
                EnumerateLogFiles(_databaseFolders[i]);

                log.Debug($"Started watching for {_databaseFolders[i]}\\*.log changes");
                _watchers[i] = new FileSystemWatcher(_databaseFolders[i], "*.log");
                _watchers[i].Changed += SkypeInstanceDesktop_DatabaseChanged;
                _watchers[i].EnableRaisingEvents = true;
            }
        }

        private void ReaderThreadProc(object param) {
            var path = (string) param;

            try {
                var reader = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (StreamReader sr = new StreamReader(reader)) {
                    sr.ReadToEnd();
                    while (true)
                    {
                        if (!sr.EndOfStream) {
                            sr.ReadToEnd();
                            DetectNewCalls(path);
                        }

                        while (sr.EndOfStream)
                            Thread.Sleep(100);
                    }
                }
            } catch(Exception) { /* ignore */}
        }

        private void ReopenFile(string file) {
            if (!_readers.ContainsKey(file)) {
                log.Debug($"Starting listening for {file} changes");
                Thread t = new Thread(ReaderThreadProc);
                t.Start(file);
                _readers.Add(file, t);
            }
        }

        public override void StopWatching() {
            base.StopWatching();

            foreach (var reader in _readers) {
                try
                {
                    reader.Value.Abort();
                }
                catch (Exception) { /* ignore */}
            }
        }

        private void EnumerateLogFiles(string path) {
            var files = Directory.GetFiles(path, "*.log", SearchOption.AllDirectories);
            FileInfo lastFile = null;

            foreach (var file in files) {
                var fi = new FileInfo(file);
                var dbKey = BuildDbFileKey(file);

                if (lastFile == null || fi.LastWriteTime > lastFile.LastWriteTime) {
                    lastFile = fi;
                }

                _lastOffset[dbKey] = fi.Length;
            }

            if (lastFile != null) {
                ReopenFile(lastFile.FullName);
            }
        }

        private void SkypeInstanceDesktop_DatabaseChanged(object sender, FileSystemEventArgs e)
        {
            DetectNewCalls(e.FullPath);
        }

        private string BuildDbFileKey(string path) {
            return Path.GetFileName(Path.GetDirectoryName(path)) + "_" + Path.GetFileName(path);
        }

        protected override void DetectNewCalls(string path) {
            ReopenFile(path);

            var dbKey = BuildDbFileKey(path);

            long offset = 0;
            if (_lastOffset.ContainsKey(dbKey)) {
                offset = _lastOffset[dbKey];
            }

            var calls = ExtractCallRecords(path, ref offset);

            foreach (var call in calls) {
                ParseCall(call);
            }

            _lastOffset[dbKey] = offset;
        }

        protected virtual string[] ExtractCallRecords(string path, ref long offset) {
            var calls = new List<string>();

            var fi = new FileInfo(path);

            if (offset < fi.Length) {
                var startPattern = Encoding.Unicode.GetBytes("<partlist type=\"");
                var endPattern = Encoding.Unicode.GetBytes("</partlist>");

                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                    fileStream.Seek(offset, SeekOrigin.Begin);

                    long len = fi.Length - offset;
                    var buf = new byte[len];
                    len = fileStream.Read(buf, 0, (int)len);

                    int lastEnd = -1;
                    int start = buf.IndexOfSequence(startPattern, 0);
                    while (start >= 0) {
                        int end = buf.IndexOfSequence(endPattern, start);
                        if (end > 0) {
                            var stringBuf = new byte[end - start + endPattern.Length];
                            Array.Copy(buf, start, stringBuf, 0, stringBuf.Length);

                            calls.Add(Encoding.Unicode.GetString(stringBuf));
                            lastEnd = end;
                        }
                        else {
                            end = start + startPattern.Length - endPattern.Length;
                        }
                        
                        start = buf.IndexOfSequence(startPattern, end + endPattern.Length);
                    }

                    if (lastEnd > 0) {
                        offset += lastEnd + endPattern.Length;
                    }
                }
            }

            return calls.ToArray();
        }
    }
}
