using System.Runtime.Serialization;
using System.ServiceModel;

namespace TestTargetWCF
{
    [ServiceContract]
    public interface IOrderService
    {
        [OperationContract]
        bool GetOrderStatus(string orderID);

        [OperationContract]
        Order SalesOrder(Order model);
    }

    [DataContract]
    public class Order
    {
        [DataMember]
        public string OrderID { get; set; }

        [DataMember]
        public bool IsActive { get; set; }
    }
}