using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AssetNode.Services.Sql;

using AssetNode.Models.Dtos;
using AssetNode.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Azure.Core;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;


namespace AssetNode.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AssetDbContext _context;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<User> _passwordHasher = new();


        public UserController(AssetDbContext context,IConfiguration config)
        {
            _context = context;
            _config = config;

        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return Ok(await _context.Users.ToListAsync());
        }

        // GET: api/users/{id}

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new { Message = $"User with ID {id} not found." });
            }

            return Ok(user);
          }

        //POST: api/users


        [HttpPost("Register")]
        public async Task<ActionResult<User>> CreateUser(UserDto userDto)
        {
            try { 
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid input data.", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }


            if (await _context.Users.AnyAsync(u => u.Username == userDto.Username))
            {
                throw new Exception($"Username '{userDto.Username}' is already taken.");
            }

            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
            {
                throw new Exception($"Email '{userDto.Email}' is already registered.");
            }


            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                PasswordHash = HashPassword(userDto.Password),
                Role = userDto.Role ?? "User",

            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        //PUT: api/users/{id}


      

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Invalid input data.", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { Message = $"User with ID {id} not found." });
            }

            
            if (await _context.Users.AnyAsync(u => u.Username == userDto.Username && u.UserId != id))
            {
                return BadRequest(new { Message = $"Username '{userDto.Username}' is already taken." });
            }

          
            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email && u.UserId != id))
            {
                return BadRequest(new { Message = $"Email '{userDto.Email}' is already registered." });
            }

          

            user.Username = userDto.Username;
            user.Email = userDto.Email;
            user.PasswordHash = HashPassword(userDto.Password);
            user.Role ="User";
            

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound(new { Message = $"User with ID {id} not found." });
                }
                throw;
            }

            return Ok(new { Message = "User updated successfully." });
        }

        // DELETE: api/users/{id}
        [AllowAnonymous]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { Message = $"User with ID {id} not found." });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"User with ID {id} deleted successfully." });
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }


        //[HttpPost("Login")]
        //public async Task<IActionResult> Login([FromBody] LoginDto dto)
        //{
        //    try
        //    {

        //        if (dto == null)
        //            return BadRequest("Login data is required");


        //        if (string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
        //            return BadRequest("Email and password are required");


        //        if (_context == null)
        //            return StatusCode(500, "Database context not initialized");


        //        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        //        if (user == null)
        //            return Unauthorized("Invalid email or password");


        //        if (string.IsNullOrEmpty(user.PasswordHash))
        //            return StatusCode(500, "User password not properly configured");


        //        bool isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        //        if (!isValid)
        //            return new Exception("Invalid email or password");


        //        if (_config == null)
        //            return StatusCode(500, "Configuration not available");


        //        var jwtKey = _config["Jwt:Key"];
        //        if (string.IsNullOrEmpty(jwtKey))
        //        {
        //            return StatusCode(500, "JWT key not configured");
        //        }


        //        var tokenHandler = new JwtSecurityTokenHandler();
        //        var key = Encoding.UTF8.GetBytes(jwtKey);

        //        var tokenDescriptor = new SecurityTokenDescriptor
        //        {
        //            Subject = new ClaimsIdentity(new[]
        //            {
        //        new Claim("Username", user.Username),
        //        new Claim("Role", user.Role),
        //        new Claim("Email", user.Email),
        //        new Claim("UserId", user.UserId.ToString())
        //    }),
        //            Expires = DateTime.UtcNow.AddHours(1),
        //            SigningCredentials = new SigningCredentials(
        //                new SymmetricSecurityKey(key),
        //                SecurityAlgorithms.HmacSha256Signature)
        //        };

        //        var token = tokenHandler.CreateToken(tokenDescriptor);
        //        var jwt = tokenHandler.WriteToken(token);

        //        return Ok(new
        //        {
        //            Email = user.Email,
        //            Username = user.Username,
        //            Role = user.Role,
        //            Token = jwt
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception details
        //        // _logger.LogError(ex, "Error during login for email: {Email}", dto.Email);
        //        return StatusCode(500, "An error occurred during login");
        //    }
        //}

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
                    return BadRequest("Email and password are required");

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                    return Unauthorized("Invalid email or password");

                var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role), 
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserId", user.UserId.ToString())
            }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    Issuer = _config["Jwt:Issuer"],
                    Audience = _config["Jwt:Audience"],
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwt = tokenHandler.WriteToken(token);

                return Ok(new
                {
                    Email = user.Email,
                    Username = user.Username,
                    Role = user.Role,
                    Token = jwt
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult TestClaims()
        {
            return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
        }


    }
}
