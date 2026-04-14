namespace BorsaAgent.API.Constanst
{
    public static class StockFilterConstants
    {
        public const float MinClosePrice = 5f;
        public const float MinVolume = 1_000_000f;
        public const float MaxDailyReturn = 10f;   // BIST limiti
        public const int MinListingDays = 100; // En az 3 ay (iş günü bazında) geçmişi olsun
    }
}
