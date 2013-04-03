using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using FailedServiceRestart.TOrderService;

namespace FailedServiceRestart
{
    public class Watcher
    {
        const string SSource = "WatchManService";
        const string SLog = "Application";

        private Thread _watchThread;

        public void StartWatcher()
        {
            try
            {
                _watchThread = new Thread(WatchMethod);
                _watchThread.Start();
            }
            catch (Exception)
            {
            }
        }

        public void StopWatcher()
        {
            try
            {
                if (_watchThread.IsAlive)
                    _watchThread.Abort();
            }
            catch (Exception)
            {
            }
        }

        public void WatchMethod()
        {

            var sEvent = "";
            
            bool isResponseNormal = true;
            while (true)
            {
                try
                {
                    // TODO: SOAP Service configuration will be loaded from configuration file, 
                    // if not then we need to configure binding of SOAP service programmitically

                    /*
                    var serviceBinding = new BasicHttpBinding();
                    var serviceEndpoint = new EndpointAddress("http://localhost/OrderService");
                    var serviceChannelFactory = new ChannelFactory<IOrderService>(myBinding, myEndpoint);

                    IOrderService client = null;

                    try
                    {
                        client = serviceChannelFactory.CreateChannel();
                        client.SalesOrder(new Order());
                        ((ICommunicationObject)client).Close();
                    }
                    catch
                    {
                        if (client != null)
                        {
                            ((ICommunicationObject)client).Abort();
                        }
                    }
                    */

                    var service = new TOrderService.OrderServiceClient();
                    service.SalesOrder(new Order());

                    isResponseNormal = true;
                }
                catch (Exception e)
                {
                    isResponseNormal = false;
                    sEvent = "Detect Failure of Service Event - Restarting Service, Time: " +
                             DateTime.Now.ToString() +
                             "\nFailed due to: " + e.Message +
                             ", from: " + e.Source +
                             ", for more: " + e.HelpLink;
                }
                finally
                {
                    if (!isResponseNormal)
                    {
                        sEvent += "\nRestarting Service, Time: " + DateTime.Now.ToString();

                        if (!EventLog.SourceExists(SSource))
                            EventLog.CreateEventSource(SSource, SLog);

                        EventLog.WriteEntry(SSource, sEvent, EventLogEntryType.Warning);

                        var serviceName = ConfigurationManager.AppSettings["ServiceName"];
                        RestartService(serviceName, 20000); // 20 Sec


                        var mailServer = ConfigurationManager.AppSettings["MailServer"].Trim();
                        var client = new SmtpClient(mailServer);
                        if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["MailServerPort"].Trim()))
                        {
                            int mailServerPort;
                            if (int.TryParse(ConfigurationManager.AppSettings["MailServerPort"], out mailServerPort))
                                client.Port = mailServerPort;

                            if (mailServerPort == 465)
                                client.EnableSsl = true;
                        }

                        client.Timeout = 10000;
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;

                        client.UseDefaultCredentials = false;

                        var mailServerUserName = ConfigurationManager.AppSettings["MailServerUserName"].Trim();
                        var mailServerPassword = ConfigurationManager.AppSettings["MailServerPassword"].Trim();
                        if (!string.IsNullOrEmpty(mailServerUserName) && !string.IsNullOrEmpty(mailServerPassword))
                        {
                            client.Credentials = new NetworkCredential(mailServerUserName, mailServerPassword);
                        }

                        var mailFrom = ConfigurationManager.AppSettings["MailFrom"].Trim();
                        var from = new MailAddress(mailFrom);

                        var mailTo = ConfigurationManager.AppSettings["MailTo"].Trim();
                        var to = new MailAddress(mailTo);

                        var message = new MailMessage(from, to)
                                          {
                                              Body =
                                                  "The service is not responding in normal manner, Service has been restarted at " +
                                                  DateTime.Now.ToString()
                                          };
                        message.Body += Environment.NewLine;
                        message.Body += "For more information please ckeck Application Event Log in Windows Event Viewer from the server";
                        message.BodyEncoding = Encoding.UTF8;

                        message.Subject = "Failure of responding";
                        message.SubjectEncoding = Encoding.UTF8;

                        message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                        const string userState = "Sending Mail...";
                        client.SendAsync(message, userState);

                        message.Dispose();
                    }
                }

                Thread.Sleep(60000); // 1 Min
            }
        }

        public void RestartService(string serviceName, int timeoutMilliseconds)
        {
            var sEvent =
                "Restarting Service, Service Name: " + serviceName + " Time: " + DateTime.Now.ToString();
            var sType = EventLogEntryType.Information;

            var service = new ServiceController(serviceName);
            try
            {
                int millisec1 = Environment.TickCount;
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                if (service.Status == ServiceControllerStatus.Running && service.CanStop)
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

                    sEvent += "\nSuccessfully stop Service, Service Name: " + serviceName + " Time: " +
                              DateTime.Now.ToString();
                }

                int millisec2 = Environment.TickCount;
                timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

                service.Refresh();
                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);

                    sEvent += "\nSuccessfully start Service, Service Name: " + serviceName + " Time: " +
                              DateTime.Now.ToString();
                }
                if (service.Status == ServiceControllerStatus.Paused)
                {
                    service.Continue();
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);

                    sEvent += "\nSuccessfully resume Service, Service Name: " + serviceName + " Time: " +
                              DateTime.Now.ToString();
                }
            }
            catch (Exception e)
            {
                sEvent += "\nFailed to restart Service, Service Name: " + serviceName + " Time: " +
                          DateTime.Now.ToString();
                sEvent += "\nFailed to restart Service, due to: " + e.Message +
                          ", from: " + e.Source +
                          ", for more: " + e.HelpLink;

                sType = EventLogEntryType.Error;

            }
            finally
            {
                if (!EventLog.SourceExists(SSource))
                    EventLog.CreateEventSource(SSource, SLog);

                EventLog.WriteEntry(SSource, sEvent, sType);
            }
        }
    }
}