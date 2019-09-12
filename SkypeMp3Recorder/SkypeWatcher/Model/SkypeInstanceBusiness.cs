using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using log4net;
using SkypeMp3Recorder.SkypeWatcher.Model.Base;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Win32;

namespace SkypeMp3Recorder.SkypeWatcher.Model
{
    public class SkypeInstanceBusiness : SkypeInstance {
        internal static readonly ILog log = LogManager.GetLogger("SkypeInstanceBusiness");

        private LyncClient lyncClient;
        private Timer timer;

        public SkypeInstanceBusiness() {
            Profiles = new SkypeProfile[]{};
        }

        public static bool IsInstalled() {
            try {
                using (var officeKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Office")) {
                    if (officeKey == null)
                        return false;
                    foreach (string subKeyName in officeKey.GetSubKeyNames()) {
                        using (RegistryKey subKey = officeKey.OpenSubKey(subKeyName)) {
                            if (subKey == null)
                                return false;

                            foreach (string subKeyName2 in subKey.GetSubKeyNames()) {
                                if (subKeyName2 == "Lync") {
                                    return true;
                                }
                            }
                        }
                    }
                }
            } catch(Exception) { /* ignore */}

            return false;
        }

        private void LyncClient_StateChanged(object sender, ClientStateChangedEventArgs e)
        {
            if (lyncClient != null && lyncClient.Self != null && lyncClient.Self.Contact != null) {
                DetectProfiles();
            }
        }

        protected override void DetectAudioSettings() {
            // nothing to do
        }

        protected override void DetectProfiles() {
            var profiles = new List<SkypeProfile>();

            if (lyncClient != null && lyncClient.Self != null && lyncClient.Self.Contact != null) {
                profiles.Add(new SkypeProfile() {
                    LastUsed = DateTime.Now,
                    User = new SkypeUser() {
                        SkypeId = lyncClient.Self.Contact.Uri
                    }
                });
            }

            Profiles = profiles.ToArray();
        }

        protected override void DetectNewCalls(string path) {
            // nothing to do
        }

        private void LyncDetected() {
            if (lyncClient != null) {
                return;
            }

            log.Debug("Connecting to Skype For Business...");
            try {
                lyncClient = LyncClient.GetClient();
                lyncClient.StateChanged += LyncClient_StateChanged;

                lyncClient.ConversationManager.ConversationAdded += ConversationManager_ConversationAdded;
                lyncClient.ConversationManager.ConversationRemoved += ConversationManager_ConversationRemoved;
            }
            catch (Exception ex) {
                log.Error("Error connecting to Lync: " + ex.Message, ex);
            }
        }

        private void LyncNotDetected() {
            if (lyncClient != null)
            {
                lyncClient.ConversationManager.ConversationAdded -= ConversationManager_ConversationAdded;
                lyncClient.ConversationManager.ConversationRemoved -= ConversationManager_ConversationRemoved;

                lyncClient = null;
            }
        }

        public override void StartWatching() {
            if(timer!=null)
                return;

            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool exists = Process.GetProcessesByName("lync").Length > 0;

            if (exists)
            {
                LyncDetected();
            } else if (lyncClient != null) {
                LyncNotDetected();
            }
        }

        private void ConversationManager_ConversationAdded(object sender, Microsoft.Lync.Model.Conversation.ConversationManagerEventArgs e) {
            var callId = e.Conversation.Properties[ConversationProperty.Id].ToString();
            var fromId = e.Conversation.SelfParticipant.Properties[ParticipantProperty.Name].ToString();
            var toId = "";
            foreach (var participant in e.Conversation.Participants) {
                if (!participant.IsSelf) {
                    toId = participant.Properties[ParticipantProperty.Name].ToString();
                    break;
                }
            }

            OnCallChanged(
                new SkypeCall() {
                    CallId = callId, From = new SkypeUser() {SkypeId = fromId, DisplayName = fromId},
                    To = new SkypeUser() {SkypeId = toId, DisplayName = toId}, Started = DateTime.Now
                }, SkypeCallState.Started);
        }

        private void ConversationManager_ConversationRemoved(object sender, ConversationManagerEventArgs e)
        {
            var callId = e.Conversation.Properties[ConversationProperty.Id].ToString();

            OnCallChanged(
                new SkypeCall()
                {
                    CallId = callId
                }, SkypeCallState.Finished);
        }

        public override void StopWatching() {
            timer.Stop();
            LyncNotDetected();
        }
    }
}
