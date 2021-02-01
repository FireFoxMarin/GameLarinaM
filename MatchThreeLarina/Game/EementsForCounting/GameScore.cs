namespace MatchThreeLarina.GameLogic
{
    internal static class GameScore
    {
        public static int Score;

        public static string ScoreString => "Your score: " + Score;

        public static void Add(int amount)
        {
            Score += amount;
        }

        public static void Reset()
        {
            Score = 0;
        }
    }
}
