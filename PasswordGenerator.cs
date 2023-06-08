using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace GodsBlessing
{
    class PasswordGenerator
    {
        public Random Random { get; private set; }
        public HashAlgorithm HashAlgorithm { get; private set; }

        public PasswordGenerator()
        {
            Random = new Random();
            HashAlgorithm = SHA1.Create();
        }

        /// <summary>
        /// Method creates simple password in length from 1 to 128 characters.
        /// </summary>
        /// <param name="length">Not more then 128 characters.</param>
        /// <returns>Simple password.</returns>
        public string CreateSimplePassword(int length)
        {
            int temporraryLength = Random.Next(1, 9);
            int hopNumber = Random.Next(1, length);

            string simplePassword = new String(Random.Next((int)Math.Pow(10, temporraryLength - 1), (int)Math.Pow(10, temporraryLength) - 1).ToString().Select((s, index) => index % hopNumber == 0 ? (char)(int.Parse(s.ToString()) + 33) : s).ToArray());

            return new String(HashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(simplePassword)).Select(s => s.ToString("X2")[0]).ToArray()).Substring(0, length);
        }
    }
}
