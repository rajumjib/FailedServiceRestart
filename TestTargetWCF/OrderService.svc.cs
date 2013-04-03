using System;
using System.ComponentModel;
using System.Net.Mail;
using System.Text;

namespace TestTargetWCF
{
    public class OrderService : IOrderService
    {
        private bool _mailSent;

        private static int _requestNo = 0;
        #region IOrderService Members

        public bool GetOrderStatus(string orderID)
        {
            return true;
        }

        public Order SalesOrder(Order model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            _requestNo++;
            if (_requestNo % 3 == 0)
            {
                throw new Exception("This is an service error occours every 3rd request");
            }

            /*
            var client = new SmtpClient("localhost");
            var from = new MailAddress("ibuybusinesses@gmail.com", "Andy " + (char) 0xD8 + " Martynyak ", Encoding.UTF8);
            var to = new MailAddress("ibuybusinesses@gmail.com");
            var message = new MailMessage(from, to);

            message.Body = "A client has been asking for this service, Time:" + DateTime.Now.ToString();
            message.Body += Environment.NewLine;
            message.BodyEncoding = Encoding.UTF8;

            message.Subject = "Responce of testing service";
            message.SubjectEncoding = Encoding.UTF8;

            client.SendCompleted += SendCompletedCallback;

            string userState = "Sending Mail...";
            client.SendAsync(message, userState);

            // If cancel command received
            if (false && mailSent == false)
            {
                client.SendAsyncCancel();
            }

            // Clean up.
            message.Dispose();
            */

            return model;
        }

        #endregion

        private void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            var token = (string)e.UserState;

            if (e.Cancelled)
            {
                //("[{0}] Send canceled.", token);
            }
            if (e.Error != null)
            {
                //("[{0}] {1}", token, e.Error.ToString());
            }
            else
            {
                //("Message sent.");
            }
            _mailSent = true;
        }
    }
}