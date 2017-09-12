using System;
using System.Collections.Generic;

namespace OakBot.Model
{
    public static class ListExtensions
    {
        /// <summary>
        /// Fisher and Yates Shuffle the List one time.
        /// </summary>
        /// <param name="rng">Seeded Pseudo-Random Generator.</param>
        public static void Shuffle<T>(this IList<T> list, Random rng)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Fisher and Yates Shuffle the List 'amount' of times.
        /// </summary>
        /// <param name="rng">Seeded Pseudo-Random Generator.</param>
        /// <param name="amount">Amount of times the list is randomly shuffled.</param>
        public static void Shuffle<T>(this IList<T> list, Random rng, int amount)
        {
            while (amount > 0)
            {
                amount--;
                Shuffle(list, rng);
            }
        }
    }
}
