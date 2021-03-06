﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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

    public static class ObservableCollectionExtensions
    {
        /// <summary>
        /// Adds a <see cref="IEnumerable{T}"/> range to a <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="items">Items to add to <see cref="ObservableCollection{T}"/>.</param>
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }

    public static class TimeSpanExtensions
    {
        /// <summary>
        /// TimeSpan Custom string output format. [x hour(s) x minute(s)] or [x minute(s)]
        /// </summary>
        public static string ToHoursMinutesString(this TimeSpan ts)
        {
            string format = "";

            if (ts.Hours > 1)
            {
                format += "h' hours '";
            }
            else if (ts.Hours == 1)
            {
                format += "h' hour '";
            }

            if (ts.Minutes == 1)
            {
                format += "m' minute'";
            }
            else
            {
                format += "m' minutes'";
            }
            return ts.ToString(format);
        }

        /// <summary>
        /// TimeSpan Custom string output format. [x minute(s) x second(s)] or [x second(s)]
        /// </summary>
        public static string ToMinutesSecondsString(this TimeSpan ts)
        {
            string format = "";

            if (ts.Minutes > 1)
            {
                format += "m' minutes '";
            }
            else if (ts.Minutes == 1)
            {
                format += "m' minute '";
            }

            if (ts.Seconds == 1)
            {
                format += "s' second'";
            }
            else
            {
                format += "s' seconds'";
            }
            return ts.ToString(format);
        }
    }
}
