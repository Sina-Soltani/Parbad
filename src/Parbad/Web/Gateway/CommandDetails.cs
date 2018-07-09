using Parbad.Core;

namespace Parbad.Web.Gateway
{
    internal class CommandDetails
    {
        public CommandDetails(GatewayCommandType commandType, long orderNumber, Money amount, string redirectUrl)
        {
            CommandType = commandType;
            OrderNumber = orderNumber;
            Amount = amount;
            RedirectUrl = redirectUrl;
        }

        public GatewayCommandType CommandType { get; }

        public long OrderNumber { get; }

        public Money Amount { get; }

        public string RedirectUrl { get; }
    }
}