using Grpc.Core;

namespace Billing.Services
{
    public class BillingService : Billing.BillingBase
    {
        private readonly ILogger<BillingService> _logger;
        public BillingService(ILogger<BillingService> logger)
        {
            _logger = logger;
        }


        public override async Task ListUsers(None request, IServerStreamWriter<UserProfile> responseStream, ServerCallContext context)
        {
            var listUsers = DB.GetUserProfiles();

            foreach (var user in listUsers)
            {
                await responseStream.WriteAsync(user);
            }
        }

        public override Task<Response> CoinsEmission(EmissionAmount request, ServerCallContext context)
        {
            Response response = DB.CoinsEmission(request.Amount);

            return Task.FromResult(response);
        }

        public override Task<Response> MoveCoins(MoveCoinsTransaction request, ServerCallContext context)
        {
            Response response = DB.MoveCoin(request);

            return Task.FromResult(response);
        }

        public override Task<Coin> LongestHistoryCoin(None request, ServerCallContext context)
        {
            Coin coin = DB.GetLongestHistoryCoin();

            return Task.FromResult(coin);
        }

    }
}
