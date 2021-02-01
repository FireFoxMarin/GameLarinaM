using MatchThreeLarina.ResourceManager;
using Microsoft.Xna.Framework;
using System;

namespace MatchThreeLarina.GameLogic
{
    internal static class Timer
    {
        public static double timeToWait;

        private static double count;

        private static bool isExpired;
        private static Action callback;

        public static string TimeRemaining => "Time: " + Math.Round(timeToWait, 0);

        public static void Reset(float newTime = 60)
        {
            timeToWait = newTime;
            isExpired = false;
        }

        public static void AddListener(Action listener)
        {
            callback += listener;
        }

        public static void Tick(GameTime time)
        {
            if (!isExpired)
                timeToWait -= time.ElapsedGameTime.TotalSeconds;

            if (timeToWait <= 5f)
            {
                count += time.ElapsedGameTime.TotalSeconds;
                if (count >= 1f)
                {
                    Resources.TickSound.Play();
                    count = 0;
                }
            }

            if (timeToWait <= 0)
            {
                timeToWait = 0;
                callback?.Invoke();
            }

        }
    }
}
