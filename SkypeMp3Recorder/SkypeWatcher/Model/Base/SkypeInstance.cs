using System;
using System.Collections.Generic;
using System.Xml;
using log4net;
using SkypeMp3Recorder.SkypeWatcher.Finder;

namespace SkypeMp3Recorder.SkypeWatcher.Model.Base
{
    public delegate void CallStateChanged(SkypeInstance instance, SkypeCall call, SkypeCallState state);

    public abstract class SkypeInstance {
        internal static readonly ILog log = LogManager.GetLogger("SkypeInstance");
        protected bool _watching = false;

        public SkypeClientTypeEnum ClientType { get; set; }
        public string DataPath { get; set; }
        public string DefaultMicrophone { get; protected set; }
        public string DefaultSpeakers { get; protected set; }

        public SkypeProfile[] Profiles { get; set; }

        protected abstract void DetectAudioSettings();
        protected abstract void DetectProfiles();

        protected abstract void DetectNewCalls(string path);

        public abstract void StartWatching();
        public abstract void StopWatching();

        public event CallStateChanged CallChanged;

        private SkypeCall _prevCall;
        private SkypeCallState _prevCallState;

        protected void OnCallChanged(SkypeCall call, SkypeCallState state) {
            if (_prevCall != null) {
                if (call.CallId.Equals(_prevCall.CallId) && state == _prevCallState)
                    return;
            }

            _prevCall = call;
            _prevCallState = state;
            CallChanged?.Invoke(this, call, state);
        }

        protected virtual bool IsNewCall(string callId) {
            return true;
        }

        protected virtual void ParseCall(string call)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(call);

                var nodes = doc.GetElementsByTagName("partlist");
                if (nodes.Count > 0)
                {
                    var rootNote = nodes[0];
                    var type = rootNote.Attributes["type"].Value;
                    var callId = rootNote.Attributes["callId"].Value;

                    var users = new List<SkypeUser>();

                    foreach (XmlNode partNode in rootNote.ChildNodes)
                    {
                        if ("part".Equals(partNode.Name))
                        {
                            var skypeId = partNode.Attributes["identity"].Value;
                            var user = new SkypeUser() { SkypeId = skypeId };

                            foreach (XmlNode nameNode in partNode.ChildNodes)
                            {
                                if ("name".Equals(nameNode.Name))
                                {
                                    user.DisplayName = nameNode.InnerText;
                                    break;
                                }
                            }

                            users.Add(user);
                        }
                    }

                    var skypeCall = new SkypeCall()
                    {
                        CallId = callId,
                        From = users.Count > 0 ? users[0] : null,
                        To = users.Count > 1 ? users[1] : null
                    };

                    SkypeCallState state = SkypeCallState.Unknown;
                    if ("started".Equals(type))
                    {
                        state = SkypeCallState.Started;
                    }
                    else if ("missed".Equals(type))
                    {
                        state = SkypeCallState.Missed;
                    }
                    else if ("ended".Equals(type))
                    {
                        state = SkypeCallState.Finished;
                    }

                    if(state != SkypeCallState.Started || IsNewCall(callId))
                        OnCallChanged(skypeCall, state);
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error parsing call: {call}", ex);
            }
        }


        public virtual void Initialize() {
            DetectProfiles();
            DetectAudioSettings();
        }
    }
}
