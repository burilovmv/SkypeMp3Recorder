using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.CoreAudioApi;

namespace SkypeMp3Recorder.Util
{
    public class DeviceHelper
    {
        public static AudioDevice[] GetInputDevices() {
            var inputDevices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            return inputDevices.Select(x => new AudioDevice() {Id = x.ID, Name = x.FriendlyName, Input = true})
                .ToArray();
        }

        public static AudioDevice[] GetOutputDevices() {
            var outputDevices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            return outputDevices.Select(x => new AudioDevice() { Id = x.ID, Name = x.FriendlyName, Input = false })
                .ToArray();
        }

        public static string GetDefaultMicrophone() {
            return new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications).ID;
        }
        public static string GetDefaultSpeaker() {
            return new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Communications).ID;
        }

        public static MMDevice GetDeviceById(string id) {
            return new MMDeviceEnumerator().GetDevice(id);
        }
    }
}
