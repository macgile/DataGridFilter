﻿#region (c) 2022 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : FilterDataGrid
// Projet     : DemoApp.Net7.0, DemoApp.Net8.0, DemoApp.Net9.0
// File       : RandomGenerator.cs
// Created    : 13/06/2023
//

#endregion

using System;
using System.Diagnostics;
using System.Globalization;

// ReSharper disable UnusedMember.Global
// ReSharper disable CheckNamespace

namespace SharedModelView.ModelView
{
    public static class RandomGenerator
    {
        #region Public Constructors

        static RandomGenerator()
        {
            Rnd = new Random();
        }

        #endregion Public Constructors

        #region Private Methods

        /// <summary>
        ///     GenerateName
        /// </summary>
        /// <returns></returns>
        private static string GenerateName(bool debug = false)
        {
            var name = "";

            // Capitalize the first letter
            name += Consonants[Rnd.Next(Consonants.Length)]?.ToUpper(CultureInfo.CurrentCulture);
            name += Vowels[Rnd.Next(Vowels.Length)];

            var nameLength = name.Length;

            // set the final name size
            var len = Rnd.Next(5, 8 + nameLength);

            while (nameLength <= len)
            {
                name += nameLength % 2 == 1 ? Consonants[Rnd.Next(Consonants.Length)] : Vowels[Rnd.Next(Vowels.Length)];

                nameLength++;
            }

            Debug.WriteLineIf(debug, $"{name,-16} length : {name.Length}");

            return name;
        }

        #endregion Private Methods

        #region Private Fields

        private static readonly string[] Consonants =
        {
            "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "y", "z"
        };

        private static readonly string[] FirstNames =
        {
            "Aiden", "Jackson", "Mason", "Liam", "Jacob", "Jayden", "Ethan", "Noah", "Lucas", "Logan", "Caleb", "Caden",
            "Jack", "Ryan", "Connor", "Michael", "Elijah", "Brayden", "Benjamin", "Nicholas", "Alexander",
            "William", "Matthew", "James", "Landon", "Nathan", "Dylan", "Evan", "Luke", "Andrew", "Gabriel", "Gavin",
            "Joshua", "Owen", "Daniel", "Carter", "Tyler", "Cameron", "Christian", "Wyatt", "Henry", "Eli",
            "Joseph", "Max", "Isaac", "Samuel", "Anthony", "Grayson", "Zachary", "David", "Christopher", "John",
            "Isaiah", "Levi", "Jonathan", "Oliver", "Chase", "Cooper", "Tristan", "Colton", "Austin", "Colin",
            "Charlie", "Dominic", "Parker", "Hunter", "Thomas", "Alex", "Ian", "Jordan", "Cole", "Julian", "Aaron",
            "Carson", "Miles", "Blake", "Brody", "Adam", "Sebastian", "Adrian", "Nolan", "Sean", "Riley",
            "Bentley", "Xavier", "Hayden", "Jeremiah", "Jason", "Jake", "Asher", "Micah", "Jace", "Brandon", "Josiah",
            "Hudson", "Nathaniel", "Bryson", "Ryder", "Justin", "Bryce", "", null
        };

        private static readonly string[] LastNames =
        {
            "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller", "Wilson", "Moore", "Taylor",
            "Anderson", "Thomas", "Jackson", "White", "Harris", "Martin", "Thompson", "Garcia",
            "Martinez", "Robinson", "Clark", "Rodriguez", "Lewis", "Lee", "Walker", "Hall", "Allen", "Young",
            "Hernandez", "King", "Wright", "Lopez", "Hill", "Scott", "Green", "Adams", "Baker",
            "Gonzalez", "Nelson", "Carter", "Mitchell", "Perez", "Roberts", "Turner", "Phillips", "Campbell", "Parker",
            "Evans", "Edwards", "Collins", "Stewart", "Sanchez", "Morris", "Rogers",
            "Reed", "Cook", "Morgan", "Bell", "Murphy", "Bailey", "Rivera", "Cooper", "Richardson", "Cox", "Howard",
            "Ward", "Torres", "Peterson", "Gray", "Ramirez", "James", "Watson", "Brooks",
            "Kelly", "Sanders", "Price", "Bennett", "Wood", "Barnes", "Ross", "Henderson", "Coleman", "Jenkins",
            "Perry", "Powell", "Long", "Patterson", "Hughes", "Flores", "Washington", "Butler",
            "Simmons", "Foster", "Gonzales", "Bryant", "Alexander", "Russell", "Griffin", "Diaz", "Hayes", "", null
        };

        private static readonly Random Rnd;
        private static readonly string[] Vowels = { "a", "e", "i", "o", "u", "y" };

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        ///     Create random employee
        /// </summary>
        /// <returns></returns>
        public static Employe CreateRandomEmployee(bool distinct = false, Countries countries = null)
        {
            // distinct lastName or not
            var emp = new Employe(
                // last name
                distinct ? GenerateName() : LastNames[Rnd.Next(LastNames.Length)],

                // first name
                FirstNames[Rnd.Next(FirstNames.Length)],

                // salary
                Math.Round(Rnd.NextDouble() * (300 - 100) + 100, 1),

                // age
                Rnd.Next(18, 75) * 1,

                // do not remove cast (DateTime?/bool?) to ensure compatibility with the C# version of the 4.8 NetFramework
                // start date + time
                Rnd.Next(0, 10) != 1
                    ? new DateTime(2012 + Rnd.Next(10), Rnd.Next(12) + 1, Rnd.Next(28) + 1, Rnd.Next(23), Rnd.Next(59),
                        Rnd.Next(59))
                    // ReSharper disable once RedundantCast
                    : (DateTime?)null,

                Rnd.Next(0, 10) != 1
                    ? new DateOnly(2012 + Rnd.Next(10), Rnd.Next(12) + 1, Rnd.Next(28) + 1)
                    // ReSharper disable once RedundantCast
                    : (DateOnly?)null,
                Rnd.Next(0, 10) != 1
                    ? new TimeOnly(Rnd.Next(23), Rnd.Next(59), Rnd.Next(59))
                    // ReSharper disable once RedundantCast
                    : (TimeOnly?)null,
                Rnd.Next(0, 10) != 1
                    ? new TimeSpan(Rnd.Next(0,2), Rnd.Next(23), Rnd.Next(59), Rnd.Next(59))
                    // ReSharper disable once RedundantCast
                    : (TimeSpan?)null,

                // is manager (three states)
                // ReSharper disable once RedundantCast
                Rnd.Next() % 3 == 2 ? (bool?)null : Rnd.Next() % 2 == 1,

                // Department (enum)
                (Departments)Rnd.Next(0, Enum.GetNames(typeof(Departments)).Length),

                // Country (IdCountry)
                countries?[Rnd.Next(0, countries.Count)]?.Id ?? 0,

                // Country (nested object)
                countries?[Rnd.Next(0, countries.Count)]
            );
            return emp;
        }

        /// <summary>
        ///     Display a list of random names (for testing)
        /// </summary>
        /// <param name="num"></param>
        public static void Generate(int num = 100)
        {
            for (var i = 0; i < num; i++) GenerateName(true);
        }

        #endregion Public Methods
    }
}