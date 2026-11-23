
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class JwtProvider : IJwtProvider
{
    private readonly JwtOptions options;
    public JwtProvider(IOptions<JwtOptions> _options)
    {
        options = _options.Value;
    }
    public string GenerateToken(UserEntity user)
    {
        Claim[] claims = [
            new("UserId",user.Id.ToString())
            ];

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey));

        var SigningCredentials = new SigningCredentials(
            symmetricSecurityKey,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(options.ExpiresHours),
            signingCredentials: SigningCredentials
            );
          
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}