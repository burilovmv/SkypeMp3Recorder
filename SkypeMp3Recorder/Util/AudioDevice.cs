using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkypeMp3Recorder.Util
{
    public class AudioDevice
    {
        public bool Input { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }

        public override string ToString() {
            return Name;
        }
    }
}
