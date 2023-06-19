using System.Text.RegularExpressions;

namespace Demo.Common.Utils.Crypto;

public class PasswordGenerator
{
    /// <summary>
    ///     Various password settings
    ///     NOTE: DONOT change these values, ever
    /// </summary>
    public const int PasswordDerivationIteration = 1000,
        PasswordBytesLength = 64,
        MinPasswordLength = 8,
        PasswordSaltLength = 16,
        ActivationCodeLength = 32;

    /// <summary>
    ///     Generates a random password based on the rules passed in the parameters
    /// </summary>
    /// <param name="includeLowercase">Bool to say if lowercase are required</param>
    /// <param name="includeUppercase">Bool to say if uppercase are required</param>
    /// <param name="includeNumeric">Bool to say if numerics are required</param>
    /// <param name="includeSpecial">Bool to say if special characters are required</param>
    /// <param name="includeSpaces">Bool to say if spaces are required</param>
    /// <param name="lengthOfPassword">Length of password required. Should be between 8 and 128</param>
    /// <returns></returns>
    public static string GeneratePassword(bool includeLowercase, bool includeUppercase, bool includeNumeric,
        bool includeSpecial, bool includeSpaces, int lengthOfPassword)
    {
        const int MAXIMUM_IDENTICAL_CONSECUTIVE_CHARS = 2;
        const string LOWERCASE_CHARACTERS = "abcdefghijklmnopqrstuvwxyz";
        const string UPPERCASE_CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string NUMERIC_CHARACTERS = "0123456789";
        const string SPECIAL_CHARACTERS = @"$_";
        const string SPACE_CHARACTER = " ";
        const int PASSWORD_LENGTH_MIN = 8;
        const int PASSWORD_LENGTH_MAX = 128;

        if (lengthOfPassword < PASSWORD_LENGTH_MIN || lengthOfPassword > PASSWORD_LENGTH_MAX) return "";

        var characterSet = "";

        if (includeLowercase) characterSet += LOWERCASE_CHARACTERS;

        if (includeUppercase) characterSet += UPPERCASE_CHARACTERS;

        if (includeNumeric) characterSet += NUMERIC_CHARACTERS;

        if (includeSpecial) characterSet += SPECIAL_CHARACTERS;

        if (includeSpaces) characterSet += SPACE_CHARACTER;

        var password = new char[lengthOfPassword];
        var characterSetLength = characterSet.Length;

        var random = new Random();
        for (var characterPosition = 0; characterPosition < lengthOfPassword; characterPosition++)
        {
            password[characterPosition] = characterSet[random.Next(characterSetLength - 1)];

            var moreThanTwoIdenticalInARow =
                characterPosition > MAXIMUM_IDENTICAL_CONSECUTIVE_CHARS
                && password[characterPosition] == password[characterPosition - 1]
                && password[characterPosition - 1] == password[characterPosition - 2];

            if (moreThanTwoIdenticalInARow) characterPosition--;
        }

        return string.Join(null, password);
    }

    /// <summary>
    ///     Checks if the password created is valid
    /// </summary>
    /// <param name="includeLowercase">Bool to say if lowercase are required</param>
    /// <param name="includeUppercase">Bool to say if uppercase are required</param>
    /// <param name="includeNumeric">Bool to say if numerics are required</param>
    /// <param name="includeSpecial">Bool to say if special characters are required</param>
    /// <param name="includeSpaces">Bool to say if spaces are required</param>
    /// <param name="password">Generated password</param>
    /// <returns>True or False to say if the password is valid or not</returns>
    public static bool PasswordIsValid(bool includeLowercase, bool includeUppercase, bool includeNumeric,
        bool includeSpecial, bool includeSpaces, string password)
    {
        const string REGEX_LOWERCASE = @"[a-z]";
        const string REGEX_UPPERCASE = @"[A-Z]";
        const string REGEX_NUMERIC = @"[\d]";
        const string REGEX_SPECIAL = @"([!#$%&*@\\])+";
        const string REGEX_SPACE = @"([ ])+";

        var lowerCaseIsValid = !includeLowercase || (includeLowercase && Regex.IsMatch(password, REGEX_LOWERCASE));
        var upperCaseIsValid = !includeUppercase || (includeUppercase && Regex.IsMatch(password, REGEX_UPPERCASE));
        var numericIsValid = !includeNumeric || (includeNumeric && Regex.IsMatch(password, REGEX_NUMERIC));
        var symbolsAreValid = !includeSpecial || (includeSpecial && Regex.IsMatch(password, REGEX_SPECIAL));
        var spacesAreValid = !includeSpaces || (includeSpaces && Regex.IsMatch(password, REGEX_SPACE));

        return lowerCaseIsValid && upperCaseIsValid && numericIsValid && symbolsAreValid && spacesAreValid;
    }
}