using System.Security.Cryptography;
using System.Text;

namespace Dirassati_Backend.Features.Auth.Register.Services
{
    public static class PasswordGeneratorService
    {
        private const string _lowercase = "abcdefghijklmnopqrstuvwxyz";
        private const string _uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string _numbers = "0123456789";
        private const string _specialCharacters = "!@#$%^&*()-_=+[]{}|;:,.<>?";
        private const int _defaultLength = 12;

        public static string GeneratePassword(int length = _defaultLength)
        {
            if (length < 8)
                throw new ArgumentException("Password length must be at least 8 characters.", nameof(length));

            // Ensure at least one character from each category
            var password = new StringBuilder();
            password.Append(GetRandomCharacter(_lowercase)); // At least one lowercase
            password.Append(GetRandomCharacter(_uppercase)); // At least one uppercase
            password.Append(GetRandomCharacter(_numbers)); // At least one number
            password.Append(GetRandomCharacter(_specialCharacters)); // At least one special character

            // Fill the remaining length with random characters from all categories
            var allCharacters = _lowercase + _uppercase + _numbers + _specialCharacters;
            for (int i = password.Length; i < length; i++)
            {
                password.Append(GetRandomCharacter(allCharacters));
            }

            // Shuffle the password to avoid predictable patterns (e.g., always starting with lowercase)
            return ShuffleString(password.ToString());
        }

        private static char GetRandomCharacter(string characterSet)
        {
            byte[] randomBytes = new byte[4];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            // Use modulo to select a random index within the character set
            int randomIndex = BitConverter.ToInt32(randomBytes, 0) % characterSet.Length;
            return characterSet[Math.Abs(randomIndex)];
        }

        private static string ShuffleString(string input)
        {
            var characters = input.ToCharArray();
            var random = new Random();
            for (int i = characters.Length - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                (characters[i], characters[j]) = (characters[j], characters[i]);
            }
            return new string(characters);
        }
    }
}