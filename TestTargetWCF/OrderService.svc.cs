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