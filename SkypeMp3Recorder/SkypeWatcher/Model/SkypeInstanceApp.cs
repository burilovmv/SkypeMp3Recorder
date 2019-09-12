using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using SkypeMp3Recorder.SkypeWatcher.Model.Base;

namespace SkypeMp3Recorder.SkypeWatcher.Model
{
    public class SkypeInstanceApp : SkypeInstanceClassic
    {
        protected override string GetDatabaseFilter() {
            return "main.db";
        }

        private Dictionary<string, SkypeCall> lastCalls = new Dictionary<string, SkypeCall>();
        private List<string> calls = new List<string>();

        protected override bool IsNewCall(string callId) {
            if (!calls.Contains(callId)) {
                calls.Add(callId);
                return true;
            }

            return false;
        }

        private string BuildDbKey(string path) {
            return Path.GetFileName(Path.GetDirectoryName(path)) + "_" + Path.GetFileName(path);
        }

        private static DateTime ConvertFromUnixTimestamp(long timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }

        private void DetectNewCalls(string path, bool generateEvents) {
            for (int i = 0; i < 5; i++) {
                try {
                    DetectNewCallsInternal(path, generateEvents);
                    break;
                } catch (Exception) { /* ignore */}
            }
        }

        private void DetectNewCallsInternal(string path, bool generateEvents) {
            log.Debug($"Detecting new calls at {path}");
            if (!File.Exists(path))
                return;

            var key = BuildDbKey(path);
            SkypeCall lastCall = null;
            if (lastCalls.ContainsKey(key)) {
                lastCall = lastCalls[key];
            }

            var connectString = $"Data Source={path};Version=3;Read Only=True;cache=shared; nolock=1";

            using (var sqliteConnection = new SQLiteConnection(connectString)) {
                sqliteConnection.Open();
                using (var command = sqliteConnection.CreateCommand()) {
                    string lastCallId = lastCall == null ? "0" : lastCall.CallId;

                    command.CommandText =
                        "select c.id, c.begin_timestamp, c.status, c.host_identity, cm.identity from Calls c, CallMembers cm\n" +
                        $"where cm.[call_db_id]=c.id and c.id>={lastCallId}\n" +
                        "order by c.begin_timestamp";

                    command.CommandType = CommandType.Text;
                    using (DbDataReader dbDataReader = command.ExecuteReader()) {
                        while (dbDataReader.Read()) {
                            SkypeCall call = new SkypeCall();
                            long callId = Convert.ToInt64(dbDataReader[0]);
                            call.CallId = callId.ToString();

                            int stateVal = dbDataReader[2] == DBNull.Value ? 0 : Convert.ToInt32(dbDataReader[2]);
                            switch (stateVal) {
                                case 4:
                                    call.State = SkypeCallState.Started;
                                    break;
                                case 6:
                                    call.State = SkypeCallState.Finished;
                                    break;
                                case 7:
                                case 8:
                                    call.State = SkypeCallState.Missed;
                                    break;
                                default:
                                    call.State = SkypeCallState.Unknown;
                                    break;
                            }

                            if (call.State == SkypeCallState.Started || dbDataReader[1] == DBNull.Value)
                                call.Started = DateTime.Now;
                            else
                                call.Started = ConvertFromUnixTimestamp((long) dbDataReader[1]);

                            call.From = new SkypeUser()
                                {SkypeId = dbDataReader[3].ToString(), DisplayName = dbDataReader[3].ToString()};
                            call.To = new SkypeUser()
                                {SkypeId = dbDataReader[4].ToString(), DisplayName = dbDataReader[4].ToString()};

                            if (call.State == SkypeCallState.Finished) {
                                call.Duration = DateTime.Now.Subtract(call.Started);
                            }

                            if (generateEvents && (lastCall == null || lastCall.CallId != call.CallId ||
                                                   call.State != lastCall.State)) {
                                OnCallChanged(call, call.State);
                            }

                            lastCall = call;
                        }

                        log.Debug($"Last found call for {key} is {lastCallId}");
                    }
                }
            }

            lastCalls[key] = lastCall;
        }

        protected override void DetectNewCalls(string path) {
            DetectNewCalls(path,true);
        }

        public override void Initialize() {
            base.Initialize();

            foreach (var profile in Profiles) {
                DetectNewCalls(Path.Combine(profile.ProfilePath, "main.db"), false);
            }
        }
    }
}
