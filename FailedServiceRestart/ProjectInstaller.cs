using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.IO;
using System.ServiceProcess;

namespace FailedServiceRestart
{
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        private const string ServiceName = "WatchManService";

        public ProjectInstaller()
        {
            //InitializeComponent();
            try
            {
                var serviceProcessInstaller = new ServiceProcessInstaller
                                                  {
                                                      Account = ServiceAccount.LocalSystem
                                                  };

                var serviceInstaller = new ServiceInstaller
                                           {
                                               ServiceName = ServiceName,
                                               Description = "Monitoring Service",
                                               StartType = ServiceStartMode.Automatic
                                           };

                Installers.Add(serviceProcessInstaller);
                Installers.Add(serviceInstaller);
            }
            catch (Exception)
            {
            }
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            var targetDir = Context.Parameters["TargetDir"];

            var serviceName = Context.Parameters["ServiceName"];

            var mailTo = Context.Parameters["MailTo"];
            var mailFrom = Context.Parameters["MailFrom"];

            var mailServer = Context.Parameters["MailServer"];

            var mailServerPort = Context.Parameters["MailServerPort"];
            var mailServerUserName = Context.Parameters["MailServerUserName"];
            var mailServerPassword = Context.Parameters["MailServerPassword"];

            WriteAppConfig(targetDir, serviceName, mailTo, mailFrom, mailServer, mailServerPort, mailServerUserName, mailServerPassword);
        }

        protected override void OnBeforeUninstall(IDictionary savedState)
        {

            var service = new ServiceController(ServiceName);
            try
            {
                if (service.Status == ServiceControllerStatus.Running && service.CanStop)
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMilliseconds(20000));
                }
            }
            catch (Exception)
            {
            }

            base.OnBeforeUninstall(savedState);
        }

        protected override void OnAfterInstall(IDictionary savedState)
        {
            base.OnAfterInstall(savedState);

            var service = new ServiceController(ServiceName);
            try
            {
                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMilliseconds(20000));
                }
            }
            catch (Exception)
            {
            }

        }

        private void WriteAppConfig(string targetDir, string serviceName, string mailTo, string mailFrom, string mailServer, string mailServerPort, string mailServerUserName, string mailServerPassword)
        {

            var userConfiguration = new Dictionary<string, string>
                                        {
                                            {"ServiceName",serviceName},
                                            {"MailTo",mailTo},
                                            {"MailFrom",mailFrom},
                                            {"MailServer",mailServer},
                                            {"MailServerPort",mailServerPort},
                                            {"MailServerUserName",mailServerUserName},
                                            {"MailServerPassword",mailServerPassword}
                                        };

            var configFilePath = Path.Combine(targetDir, "FailedServiceRestart.exe");
            ConfigGenerator.WriteExternalAppConfig(configFilePath, userConfiguration);

            //configFilePath = Path.Combine(targetDir, "FailedServiceRestart.vshost.exe.config");
            //ConfigGenerator.WriteExternalAppConfig(configFilePath, userConfiguration);

        }
    }

    public class ConfigGenerator
    {
        public static void WriteExternalAppConfig(string configFilePath, IDictionary<string, string> userConfiguration)
        {

            var config = ConfigurationManager.OpenExeConfiguration(configFilePath);
            foreach (var pair in userConfiguration)
            {
                config.AppSettings.Settings[pair.Key].Value = pair.Value;
            }
            config.Save();

            /*
            using (var xw = new XmlTextWriter(configFilePath, Encoding.UTF8))
            {
                xw.Formatting = Formatting.Indented;
                xw.Indentation = 4;
                xw.WriteStartDocument();
                xw.WriteStartElement("appSettings");

                foreach (var pair in userConfiguration)
                {
                    xw.WriteStartElement("add");
                    xw.WriteAttributeString("key", pair.Key);
                    xw.WriteAttributeString("value", pair.Value);
                    xw.WriteEndElement();
                }

                xw.WriteEndElement();
                xw.WriteEndDocument();
            }
            */
        }
    }
}