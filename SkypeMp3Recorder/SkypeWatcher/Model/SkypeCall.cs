using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkypeMp3Recorder.SkypeWatcher.Model
{
    public enum SkypeCallState {
        Started,
        Finished,
        Missed,
        Unknown
    }

    public class SkypeCall
    {
        public DateTime Started { get; set; }
        public TimeSpan Duration { get; set; }
        public string CallId { get; set; }
        public SkypeUser From { get; set; }
        public SkypeUser To { get; set; }

        public SkypeCallState State { get; set; }

    }
}
