using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(AppDbContext context, IOptions<JwtSettings> jwtOptions)
    {
        _context = context;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Username.ToLower() == request.Username.ToLower());

        if (employee == null || employee.Password != request.Password|| !employee.IsActive)
        {
            return null; // có thể hash và so sánh ở đây
        }

        var token = GenerateToken(employee);

        return new LoginResponseDto
        {
            EmployeeId = employee.Id,
            Token = token,
            Role = employee.Role
        };
    }

    private string GenerateToken(Employee employee)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, employee.Username),
            new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
            new Claim(ClaimTypes.Role, employee.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
