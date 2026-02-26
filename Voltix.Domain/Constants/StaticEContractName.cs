using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Domain.Constants
{
    public static class StaticEContractName
    {
        public const string EContractDealerTier1 = "DEALER_TIER_1";
        public const string EContractDealerTier2 = "DEALER_TIER_2";
        public const string EContractDealerTier3 = "DEALER_TIER_3";
        public const string EContractDealerTier4 = "DEALER_TIER_4";
        public const string EContractDealerTier5 = "DEALER_TIER_5";

        public const string EContractDepositCustomerOrder = "CUSTOMER_DEPOSIT_CONTRACT";
        public const string EContractPayFullCustomerOrder = "CUSTOMER_PAY_FULL_E_CONTRACT";
        public const string EContractPayRemainderCustomerOrder = "CUSTOMER_PAY_REMAINDER_E_CONTRACT";
        public const string EContractInvoiceConfirmBookingEV = "E_CONTRACT_INVOICE_CONFIRM_BOOKING_EV";
    }
}
