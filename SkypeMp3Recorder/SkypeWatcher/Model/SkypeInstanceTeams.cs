using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using Newtonsoft.Json.Linq;
using SkypeMp3Recorder.Extensions;

namespace SkypeMp3Recorder.SkypeWatcher.Model
{
    public class SkypeInstanceTeams : SkypeInstanceDesktop
    {
        internal static readonly ILog log = LogManager.GetLogger("SkypeInstanceTeams");

        protected override void DetectDatabaseFolders() {
            log.Debug("Detecting folders in " + Path.Combine(DataPath, "IndexedDB"));
            log.Debug("Detecting folders in " + Path.Combine(DataPath, "Local Storage"));

            var folders = new List<string>();

            folders.AddRange(Directory.GetDirectories(Path.Combine(DataPath, "Local Storage")));
            folders.AddRange(Directory.GetDirectories(Path.Combine(DataPath, "IndexedDB")));

            _databaseFolders = folders.ToArray();
        }

        protected override void ParseCall(string call) {
            log.Debug($"Parse call: {call}");
            try {
                var obj = JObject.Parse(call);

                SkypeCallState state = SkypeCallState.Unknown;
                string callId = null;
                SkypeUser from = null;
                SkypeUser to = null;

                JObject current = obj.GetValue("current") as JObject;
                if (current != null) {
                    var idObj = obj.GetValue("ids") as JObject;
                    callId = idObj.GetValue("callId").ToString();

                    var stateInt = current.GetValue("state").Value<int>();
                    if (stateInt == 3) {
                        state = SkypeCallState.Started;
                    }
                }
                else {
                    state = SkypeCallState.Finished;
                    callId = obj.GetValue("callId").ToString();

                    var fromObj = obj.GetValue("originatorParticipant") as JObject;
                    if (fromObj != null) {
                        from = new SkypeUser() {
                            SkypeId = fromObj.GetValue("id").ToString(),
                            DisplayName = fromObj.GetValue("displayName").ToString()
                        };
                    }

                    var toObj = obj.GetValue("targetParticipant") as JObject;
                    if (toObj != null) {
                        to = new SkypeUser()
                        {
                            SkypeId = toObj.GetValue("id").ToString(),
                            DisplayName = toObj.GetValue("displayName").ToString()
                        };
                    }
                }

                var skypeCall = new SkypeCall()
                {
                    CallId = callId,
                    From = from,
                    To = to
                };

                if (state != SkypeCallState.Started || IsNewCall(callId))
                    OnCallChanged(skypeCall, state);
            }
            catch (Exception ex) {

            }
        }

        protected override string[] ExtractCallRecords(string path, ref long offset)
        {
            var calls = new List<string>();

            var fi = new FileInfo(path);
            var root = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(path)));

            log.Debug($"Extracting changes from {path}, offset={offset}, root={root}");

            if (offset < fi.Length) {
                byte[] startPattern;

                if ("IndexedDB".Equals(root)) {
                    startPattern = Encoding.ASCII.GetBytes("{\"startTime\":");
                }
                else {
                    startPattern = Encoding.ASCII.GetBytes("{\"ids\":");
                }
                
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fileStream.Seek(offset, SeekOrigin.Begin);

                    long len = fi.Length - offset;
                    var buf = new byte[len];
                    len = fileStream.Read(buf, 0, (int)len);

                    int lastEnd = -1;
                    int start = buf.IndexOfSequence(startPattern, 0);
                    while (start >= 0) {
                        //log.Debug($"  found pattern at {start}");
                        int brackets = 1;
                        int end = 0;
                        int cur = start + 1;
                        while (brackets > 0 && cur < len) {
                            if (buf[cur] == (byte)0x7b) { // {
                                brackets++;

                                //log.Debug($"  offset={cur}, brackets={brackets}");
                            }

                            if (buf[cur] == (byte)0x7d) { // }
                                brackets--;

                                //log.Debug($"  offset={cur}, brackets={brackets}");
                            }

                            if (brackets == 0) {
                                end = cur;
                                break;
                            }
                            cur++;
                        }

                        if (end > 0)
                        {
                            var stringBuf = new byte[end - start + 1];
                            Array.Copy(buf, start, stringBuf, 0, stringBuf.Length);

                            calls.Add(Encoding.ASCII.GetString(stringBuf));
                            lastEnd = end;
                        }
                        else
                        {
                            end = start;
                        }

                        start = buf.IndexOfSequence(startPattern, end + 1);
                    }

                    if (lastEnd > 0)
                    {
                        offset += lastEnd;
                    }
                }
            }

            return calls.ToArray();
        }
    }
}
