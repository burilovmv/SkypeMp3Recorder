using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using SkypeMp3Recorder.SkypeWatcher.Model;
using SkypeMp3Recorder.SkypeWatcher.Model.Base;

namespace SkypeMp3Recorder.SkypeWatcher.Finder
{
    public enum SkypeClientTypeEnum {
        App,
        Classic,
        Desktop,
        Business,
        Teams
    }

    public class SkypeInstanceFinder
    {
        internal static readonly ILog log = LogManager.GetLogger("SkypeInstanceFinder");

        private static string GetSkypeDataSkypeApp() {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            
            string path1 = Path.Combine(folderPath, "Packages");
            if (!Directory.Exists(path1))
                return null;

            foreach (string directory in Directory.GetDirectories(path1))
            {
                try
                {
                    if (directory.Contains("Microsoft.SkypeApp_"))
                    {
                        string path2 = Path.Combine(directory, "LocalState");
                        if (Directory.Exists(path2))
                            return path2;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error GetSkypeDataSkypeApp()", ex);
                }
            }

            return null;
        }

        private static string GetSkypeFolder(SkypeClientTypeEnum clientType) {
            switch (clientType)
            {
                case SkypeClientTypeEnum.App:
                    return GetSkypeDataSkypeApp();
                case SkypeClientTypeEnum.Classic:
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Skype");
                case SkypeClientTypeEnum.Desktop:
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "Skype for Desktop");
                case SkypeClientTypeEnum.Teams:
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "Teams");
                default:
                    return null;
            }
        }

        public static SkypeInstance[] FindInstances() {
            var list = new List<SkypeInstance>();

            var folder = GetSkypeFolder(SkypeClientTypeEnum.App);
            if(folder!=null && Directory.Exists(folder))
                list.Add(new SkypeInstanceApp() { ClientType = SkypeClientTypeEnum.App, DataPath = folder});

            folder = GetSkypeFolder(SkypeClientTypeEnum.Classic);
            if(folder!= null && Directory.Exists(folder))
                list.Add(new SkypeInstanceClassic() { ClientType = SkypeClientTypeEnum.Classic, DataPath = folder});

            folder = GetSkypeFolder(SkypeClientTypeEnum.Desktop);
            if(folder!= null && Directory.Exists(folder))
                list.Add(new SkypeInstanceDesktop() { ClientType = SkypeClientTypeEnum.Desktop, DataPath = folder});

            folder = GetSkypeFolder(SkypeClientTypeEnum.Teams);
            if(folder!= null && Directory.Exists(folder))
                list.Add(new SkypeInstanceTeams() { ClientType = SkypeClientTypeEnum.Teams, DataPath = folder});

            if(SkypeInstanceBusiness.IsInstalled())
                list.Add(new SkypeInstanceBusiness() { ClientType = SkypeClientTypeEnum.Business });

            foreach (var instance in list) {
                try {
                    instance.Initialize();
                }
                catch (Exception ex) {
                    log.Error($"Error initializing {instance.ClientType} instance ", ex);
                }
            }

            return list.ToArray();
        }
    }
}
