using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController: ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        public AuthController(IAuthRepository repo, IConfiguration config)
        { 
            _repo = repo;
            _config = config;
        }
  
       [HttpPost("register")]
       public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto )
       {
           // validate request 

        //    if(!ModelState.IsValid) // use this to validate if not an apicontroller
        //         return BadRequest(ModelState);

           userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

           if(await _repo.UserExists(userForRegisterDto.Username))
                return BadRequest("User already exists..");

           var userToCreate = new User
           {
               Username = userForRegisterDto.Username
           };

           var createdUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

           return StatusCode(201);
            

       }
[HttpPost("login")]
public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
{
// try
// {
  //  throw new Exception("Computer sys no");
    
    var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password.ToLower());

    if(userFromRepo == null)
    return Unauthorized();

var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
    new Claim(ClaimTypes.Name, userFromRepo.Username) 

};
 
    var key= new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
    
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);



            var tokenHandler = new JwtSecurityTokenHandler();

    var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity((IEnumerable<Claim>)claims),
        Expires = DateTime.Now.AddDays((double)1),
        SigningCredentials = creds

    });

    return Ok(new {
        token= new JwtSecurityTokenHandler().WriteToken(token)
    });
//}
// catch
// {
    
//     return StatusCode(500,"Computer really says no!");
// }

  //  throw new Exception("Computer sys no");

  
}
    }
}