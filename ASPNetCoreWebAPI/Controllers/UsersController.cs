using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASPNetCoreWebAPI.Entities;
using ASPNetCoreWebAPI.Models.Users;
using ASPNetCoreWebAPI.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ASPNetCoreWebAPI.Controllers {
    [Authorize]
    [Route ("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase {
        private IUserService _userService;
        private IMapper _mapper;
        private IConfiguration _config;
        public UsersController (IUserService UserService, IMapper Mapper, IConfiguration config) {
            _userService = UserService;
            _mapper = Mapper;
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost ("authenticate")]
        public IActionResult Authenticate ([FromBody] AuthenticateModel authmodel) {
            var user = _userService.Authenticate (authmodel.Username, authmodel.Password);

            if (user == null)
                return BadRequest (new { message = "Username or password is incorrect" });

            var tokenString = BuildToken ();
            return Ok (new {
                Id = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Token = tokenString
            });
        }

        private string BuildToken () {
            var key = new SymmetricSecurityKey (Encoding.UTF8.GetBytes (_config["Jwt:Key"]));
            var creds = new SigningCredentials (key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken (_config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                expires : DateTime.Now.AddMinutes (30),
                signingCredentials : creds);

            return new JwtSecurityTokenHandler ().WriteToken (token);
        }

        [AllowAnonymous]
        [HttpPost ("register")]
        public IActionResult Register ([FromBody] RegisterModel model) {
            // map model to entity
            var usermodel = _mapper.Map<User> (model);

            try {
                // create user
                var user = _userService.Create (usermodel, model.Password);
                var tokenString = BuildToken ();
                return Ok (new {
                    Id = user.Id,
                        Username = user.Username,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Token = tokenString
                });
            } catch (Exception ex) {
                // return error message if there was an exception
                return BadRequest (new { message = ex.Message });
            }
        }

        [HttpPut ("{id}")]
        public IActionResult Update (int id, [FromBody] UpdateModel model) {
            // map model to entity and set id
            var user = _mapper.Map<User> (model);
            user.Id = id;

            try {
                // update user 
                _userService.Update (user, model.Password);
                return Ok ();
            } catch (Exception ex) {
                // return error message if there was an exception
                return BadRequest (new { message = ex.Message });
            }
        }

    }
}