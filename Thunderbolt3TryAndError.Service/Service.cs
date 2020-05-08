using SimpleWMI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Thunderbolt3TryAndError.Service
{
    partial class Service : ServiceBase
    {
        System.Timers.Timer timer;

        public Service()
        {
            InitializeComponent();
            this.timer = new System.Timers.Timer();
            this.timer.AutoReset = false;
            this.timer.Elapsed += timer_Elapsed;
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            string graphicsCardPath = GetRegistrySetting("graphicsCardPath");
            string pciRootClassRaw = GetRegistrySetting("pciRootClass");
            string pciRootPath = GetRegistrySetting("pciRootPath");
            try
            {
                graphicsCardPath = GetRegistrySetting("graphicsCardPath");
                pciRootClassRaw = GetRegistrySetting("pciRootClass");
                pciRootPath = GetRegistrySetting("pciRootPath");
            }
            catch (Exception ex)
            {
                base.EventLog.WriteEntry(string.Format("Error while Reading Settings: {0}", ex), EventLogEntryType.Error);
            }

            if (!IsNullOrEmptyOrWhiteSpace(graphicsCardPath))
            {
                foreach (dynamic graphicsCard in WMIQuery.GetAllObjects(Win32.VideoController))
                {
                    if (graphicsCard.Status == "Error" && graphicsCard.ConfigManagerErrorCode == 43)
                    {
                        base.EventLog.WriteEntry(string.Format("VideoController found with Error 43: Name:\"{0}\", DeviceID:\"{1}\"", graphicsCard.Name, graphicsCard.PNPDeviceID), EventLogEntryType.Information);

                        if (graphicsCard.PNPDeviceID == graphicsCardPath)
                        {
                            if (!IsNullOrEmptyOrWhiteSpace(pciRootClassRaw))
                            {
                                if (!IsNullOrEmptyOrWhiteSpace(pciRootPath))
                                {
                                    //Guid pciRootClass = new Guid("{4d36e97d-e325-11ce-bfc1-08002be10318}");
                                    //Guid pciRootClass = new Guid(pciRootClassRaw);

                                    if (Guid.TryParse(pciRootClassRaw, out Guid pciRootClass))
                                    {
                                        try
                                        {
                                            base.EventLog.WriteEntry("Disabling Device...");
                                            DisableDevice.DeviceHelper.SetDeviceEnabled(pciRootClass, pciRootPath, false);
                                        }
                                        catch (Exception ex)
                                        {
                                            base.EventLog.WriteEntry(string.Format("Disabling Device failed: {0}", ex), EventLogEntryType.Error);
                                            //base.Stop();
                                            //return;
                                        }

                                        try
                                        {
                                            base.EventLog.WriteEntry("Enabling Device...");
                                            DisableDevice.DeviceHelper.SetDeviceEnabled(pciRootClass, pciRootPath, true);
                                        }
                                        catch (Exception ex)
                                        {
                                            base.EventLog.WriteEntry(string.Format("Enabling Device failed: {0}", ex), EventLogEntryType.Error);
                                            //base.Stop();
                                            //return;
                                        }
                                    }
                                    else
                                    {
                                        base.EventLog.WriteEntry("Couldnt parse \"pciRootClassRaw\" GUID", EventLogEntryType.Warning);
                                    }
                                }
                                else
                                {
                                    base.EventLog.WriteEntry("\"pciRootPath\" is empty, null or whitespace", EventLogEntryType.Warning);
                                }
                            }
                            else
                            {
                                base.EventLog.WriteEntry("\"pciRootClassRaw\" is empty, null or whitespace", EventLogEntryType.Warning);
                            }
                        }
                    }
                }
            }
            else
            {
                base.EventLog.WriteEntry("\"graphicsCardPath\" is empty, null or whitespace", EventLogEntryType.Warning);
            }

            this.timer.Start();
        }

        protected override void OnStart(string[] args)
        {
            // TODO: Hier Code hinzufügen, um den Dienst zu starten.
            base.EventLog.WriteEntry("Starting Service", EventLogEntryType.Information);

            double checkInterval = double.Parse(GetRegistrySetting("checkInterval", "5000"));
            this.timer.Interval = checkInterval;

            this.timer.Start();
        }

        protected override void OnStop()
        {
            // TODO: Hier Code zum Ausführen erforderlicher Löschvorgänge zum Anhalten des Dienstes hinzufügen.
            base.EventLog.WriteEntry("Stopping Service", EventLogEntryType.Information);

            if (this.timer.Enabled)
            {
                this.timer.Stop();
            }
        }

        private static string GetRegistrySetting(string name, string standard = "")
        {
            string settingRootName = "Thunderbolt3TryAndErrorService";
            Microsoft.Win32.RegistryKey regSoftware = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE", true);
            string[] subkeyNames = regSoftware.GetSubKeyNames();
            bool subkeyFound = false;

            for (int i = 0; i < subkeyNames.Length; i++)
            {
                string subkeyName = subkeyNames[i];
                if (subkeyName == settingRootName)
                {
                    subkeyFound = true;
                    break;
                }
            }

            if (!subkeyFound)
            {
                regSoftware = regSoftware.CreateSubKey(settingRootName, true);
            }
            else
            {
                regSoftware = regSoftware.OpenSubKey(settingRootName, true);
            }

            string[] valueNames = regSoftware.GetValueNames();
            if (!valueNames.Contains(name))
            {
                regSoftware.SetValue(name, standard, Microsoft.Win32.RegistryValueKind.String);
            }

            string settingValue = (string)regSoftware.GetValue(name, standard);
            return settingValue;
        }

        public static bool IsNullOrEmptyOrWhiteSpace(string @string)
        {
            if (!string.IsNullOrEmpty(@string))
            {
                if (!string.IsNullOrWhiteSpace(@string))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }
    }
}
