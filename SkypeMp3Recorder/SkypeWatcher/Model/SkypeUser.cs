using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkypeMp3Recorder.SkypeWatcher.Model
{
    public class SkypeUser
    {
        public string SkypeId { get; set; }
        public string DisplayName { get; set; }

        public override string ToString() {
            if (!String.IsNullOrEmpty(DisplayName)) {
                return $"{SkypeId} ({DisplayName})";
            }
            return SkypeId;
        }
    }
}
