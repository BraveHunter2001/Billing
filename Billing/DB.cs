using System.Runtime.CompilerServices;

namespace Billing
{
    public static class DB
    {
        const string SEPARATOR_HISTORY = "-";
        public class UserRating
        {
            public string Name { get; }
            private int rating;
            public int Rating { get { return rating; } private set { rating = value; } }

            public UserRating(string name, int rating)
            {
                Name = name;
                Rating = rating;
            }
        }

        private static List<UserRating> usersRating = new()
        {
            new UserRating("boris", 5000),
            new UserRating("maria", 1000),
            new UserRating("oleg", 800)
        };

        private static Dictionary<long, string> coins = new();
        private static int lastIdCoin = coins.Count;
        private static void AddNewCoin(string name)
        {
            coins.Add(lastIdCoin++, name);
        }
        private static void AddNewCoins(string name, int countCoins)
        {
            for(int i = 0; i < countCoins; i++)
            coins.Add(lastIdCoin++, name);
        }
        private static void UpdateHistoryCoin(long idCoin, string newName)
        {
            string lastHsitory = coins[idCoin];
            lastHsitory += SEPARATOR_HISTORY + newName;
            coins[idCoin] = lastHsitory;

        }
        private static string GetLastHistoryCoin(string history)
        {
            return history.Split(SEPARATOR_HISTORY).Last();
        }
        private static List<Coin> GetCoinsByName(string name)
        {
            var coinIds = coins.Where(
                o => GetLastHistoryCoin(o.Value) == name)
                .Select(o => o.Key)
                .ToList();
            List<Coin> coinsUser = new List<Coin>();

            foreach (var id in coinIds)
                coinsUser.Add(new Coin { Id = id, History = coins[id] });

            return coinsUser;
        }
        private static int GetCountCoinByName(string name)
        {
            var namesUser = coins.Where(o => GetLastHistoryCoin(o.Value) == name);
            return namesUser.Count();
        }
        public static List<UserProfile> GetUserProfiles()
        {
            List<UserProfile> users = new();

            foreach (UserRating user in usersRating)
            {
                int countCoins = GetCountCoinByName(user.Name);
                UserProfile profile = new UserProfile { Name = user.Name, Amount = countCoins };
                users.Add(profile);
            }

            return users;

        }
        public static Coin GetLongestHistoryCoin()
        {
            int max = 0;
            long keyAns = 0;

            foreach (var key in coins.Keys)
            {
                int countHis = coins[key].Split(SEPARATOR_HISTORY).Length;
                if (countHis >= max)
                {
                    max = countHis;
                    keyAns = key;
                }
            }

            return new Coin { Id = keyAns, History = coins[keyAns] };
        }
        public static Response MoveCoin(MoveCoinsTransaction transaction)
        {
            Response response = new Response();

            bool notExistSrcUser = usersRating.Where(o => o.Name == transaction.SrcUser).Count() == 0;
            bool notExistDstUser = usersRating.Where(o => o.Name == transaction.DstUser).Count() == 0;

            int countCoinSrcUser = GetCountCoinByName(transaction.SrcUser);

            if (notExistSrcUser)
            {
                response.Status = Response.Types.Status.Unspecified;
                response.Comment = $"User {transaction.SrcUser} dont exist ";
                return response;
            }
            if (notExistDstUser)
            {
                response.Status = Response.Types.Status.Unspecified;
                response.Comment = $"User {transaction.DstUser} dont exist ";
                return response;
            }

            if (countCoinSrcUser < transaction.Amount)
            {
                response.Status = Response.Types.Status.Failed;
                response.Comment = $"User {transaction.SrcUser} doesn't enough coins for transaction." +
                    $" He has {countCoinSrcUser} coins";
                return response;
            }

            List<Coin> coinsUser = GetCoinsByName(transaction.SrcUser);

            for (long i = 0; i < transaction.Amount; i++)
            {
                Coin coin = coinsUser.Last();
                UpdateHistoryCoin(coin.Id, transaction.DstUser);
                coinsUser.Remove(coin);
            }

            response.Status = Response.Types.Status.Ok;
            response.Comment = $"{transaction.DstUser} received {transaction.Amount} coins from {transaction.SrcUser}";

            return response;
        }

        public static Response CoinsEmission(long Amount)
        {
            if (Amount < usersRating.Count)
                return new Response
                {
                    Status = Response.Types.Status.Failed,
                    Comment = "not enough coins to Emission "
                };

            double maxRating = 0;
            foreach (var ur in usersRating)
                maxRating += ur.Rating;

            var sortedlistUser = usersRating.OrderBy(o => o.Rating).ToList();

            double[] probsUser = new double[sortedlistUser.Count];
            double sumProbsDelta = 0.0;
            double probOneCoin = 1.0 / Amount;
            int countMinimalprob = 0;


            for (int i = 0; i < sortedlistUser.Count; i++)
            {
                double deltaProb = sumProbsDelta / (Amount - countMinimalprob);
                double procentUser = sortedlistUser[i].Rating / maxRating - deltaProb;

                if (probOneCoin > procentUser)
                {
                    probsUser[i] = probOneCoin;
                    sumProbsDelta += (probOneCoin - procentUser);
                    countMinimalprob++;
                }
                else
                    probsUser[i] = procentUser;
            }

            int sumCoinWithouLast = 0;
            int[] coinsToUser = new int[sortedlistUser.Count];

            for (int i = 0; i < probsUser.Length - 1; i++)
            {
                coinsToUser[i] = (int)Math.Floor(probsUser[i] / probOneCoin);
                sumCoinWithouLast += (int)coinsToUser[i];
            }
            coinsToUser[coinsToUser.Length - 1] = (int)Amount - sumCoinWithouLast;


            for (int i = 0; i < coinsToUser.Length; i++)
                AddNewCoins(sortedlistUser[i].Name, coinsToUser[i]);

            return new Response
            {
                Status = Response.Types.Status.Ok,
                Comment = $"Successful Emission {Amount} Coins"
            };

        }

    }
}
