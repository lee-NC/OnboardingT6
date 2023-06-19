using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Demo.Common.Utils.Crypto;

public class JwtValidationUtils
{
    /// <summary>
    /// </summary>
    /// <param name="conf"></param>
    /// <returns></returns>
    public static TokenValidationParameters GetTokenValidationParametersRS(IConfiguration conf)
    {
        var key = conf["Identityserver:Key"];
        var rs256Token = key.Replace("-----BEGIN PUBLIC KEY-----", "");
        rs256Token = rs256Token.Replace("-----END PUBLIC KEY-----", "");
        rs256Token = rs256Token.Replace("\n", "");

        var keyBytes = Convert.FromBase64String(rs256Token);

        var asymmetricKeyParameter = PublicKeyFactory.CreateKey(keyBytes);
        var rsaKeyParameters = (RsaKeyParameters)asymmetricKeyParameter;
        var rsaParameters = new RSAParameters
        {
            Modulus = rsaKeyParameters.Modulus.ToByteArrayUnsigned(),
            Exponent = rsaKeyParameters.Exponent.ToByteArrayUnsigned()
        };
        var rsa = new RSACryptoServiceProvider();

        rsa.ImportParameters(rsaParameters);
        var validateAud =
            ("" + conf["Identityserver:ValidateAudience"]).Equals("true", StringComparison.OrdinalIgnoreCase);
        var validateIss =
            ("" + conf["Identityserver:ValidateIssuer"]).Equals("true", StringComparison.OrdinalIgnoreCase);

        var validationParameters = new TokenValidationParameters
        {
            ValidateAudience = validateAud,
            ValidAudience = conf["Identityserver:Audience"],
            ValidateIssuer = validateIss,
            ValidIssuer = conf["Identityserver:Issuer"],
            RequireSignedTokens = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(rsa),
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };

        return validationParameters;
    }
}