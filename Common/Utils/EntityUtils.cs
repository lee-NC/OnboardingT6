using System.Security.Cryptography;
using System.Text;
using Demo.Common.Utils.Crypto;
using Microsoft.AspNetCore.Identity;

namespace Demo.Common.Utils
{
    public static class EntityUtils
    {
        /// <summary>
        /// Generates a random string.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string GenerateRandomString(int length)
        {
            var result = new StringBuilder();
            const string src = "1234567890";
            var seed = GetRandomSeed();
            var rnd = new Random(seed);
            length.Times(() => result.Append(src[rnd.Next(src.Length - 1)]));
            return result.ToString();
        }

        /// <summary>
        /// Merge dictionary.
        /// Like Jquery's $.extend(source, target)
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void Append(this IDictionary<string, object> target, IDictionary<string, object> source)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (source == null)
            {
                return;
            }

            foreach (var key in source.Keys)
            {
                var val = source[key];
                if (val == null)
                {
                    continue;
                }

                if (target.ContainsKey(key))
                {
                    target[key] = val;
                }
                else
                {
                    target.Add(key, val);
                }
            }
        }

        /// <summary>
        /// Repeat the action n times.
        /// </summary>
        /// <param name="n">The times.</param>
        /// <param name="action">The action.</param>
        public static void Times(this int n, Action action)
        {
            if (action == null || n <= 0)
            {
                return;
            }

            for (var i = 0; i < n; i++)
            {
                action();
            }
        }

        /// <summary>
        /// Gets the random seed.
        /// </summary>
        /// <returns></returns>
        public static int GetRandomSeed()
        {
            //var rng = new RNGCryptoServiceProvider();
            //rng.GetBytes(randomBytes);
            var randomBytes = RandomNumberGenerator.GetBytes(4);
            return (randomBytes[0] & 0x7f) << 24 |
                   randomBytes[1] << 16 |
                   randomBytes[2] << 8 |
                   randomBytes[3];
        }

        /// <summary>
        /// Generates some the random bytes.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static byte[] GenerateRandomBytes(int length)
        {
            //Random random = new Random();
            //var result = new byte[length];
            //random.NextBytes(result);
            //return result;
            return RandomNumberGenerator.GetBytes(length);
        }

        /// <summary>
        /// Hash the input password
        /// </summary>
        /// <param name="pwd">The password.</param>
        /// <param name="salt">The salt.</param>
        /// <returns></returns>
        public static byte[] GetInputPasswordHash(string pwd, byte[] salt)
        {
            var inputPwdBytes = Encoding.UTF8.GetBytes(pwd);
            var inputPwdHasher = new Rfc2898DeriveBytes(inputPwdBytes, salt, PasswordGenerator.PasswordDerivationIteration, HashAlgorithmName.SHA256);
            return inputPwdHasher.GetBytes(PasswordGenerator.PasswordBytesLength);
        }

        ///<summary>
        /// Generate notify number
        ///</summary>
        ///<param name="date">The date</param>
        ///<returns></returns>
        public static string GenerateNotifyNumber(DateTime date)
        {
            return string.Format("{0:MMddHHmmssfff/yyyy}/TB-TVAN", date);
        }

        public static bool VerifyPassword(byte[] oldPwdHash, string newPassword, byte[] passwordSalt)
        {
            var salt = new byte[PasswordGenerator.PasswordSaltLength];
            for (var i = 0; i < PasswordGenerator.PasswordSaltLength; i++)
            {
                salt[i] = passwordSalt[i];
            }

            var newPwdHash = GetInputPasswordHash(newPassword, salt);

            if (newPwdHash.SequenceEqual(oldPwdHash))
            {
                return true;
            }

            return false;
        }

        public static string GenerateRandomPassword(PasswordOptions opts = null)
        {
            opts ??= new PasswordOptions()
            {
                RequiredLength = 8,
                RequiredUniqueChars = 4,
                RequireDigit = true,
                RequireLowercase = true,
                RequireNonAlphanumeric = true,
                RequireUppercase = true
            };

            string[] randomChars = new[] {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
            "abcdefghijkmnopqrstuvwxyz",    // lowercase
            "0123456789",                   // digits
            "!@$?_-"                        // non-alphanumeric
        };

            Random rand = new(Environment.TickCount);
            List<char> chars = new();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

        public static byte[] Base64Decode(string input)
        {
            input = input.PadRight(input.Length + (4 - input.Length % 4) % 4, '=');
            var result = Convert.FromBase64String(input);
            return result;
        }
    }
}
