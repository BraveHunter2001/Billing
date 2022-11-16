namespace Billing.Services
{
    public class BillingService : Billing.BillingBase
    {
        private readonly ILogger<BillingService> _logger;
        public BillingService(ILogger<BillingService> logger)
        {
            _logger = logger;
        }



    }
}
