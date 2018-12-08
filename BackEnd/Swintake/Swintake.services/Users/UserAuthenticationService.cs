﻿using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SecuredWebApi.Services.Security;
using Swintake.domain.Users;
using Swintake.services.Users;
using Swintake.services.Users.Security;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecuredWebApi.Services
{
    public class UserAuthenticationService : IUserAuthenticationService
    { 
        private readonly IUserRepository _userRepository;
        private readonly Hasher _hasher;
        private readonly Salter _salter;
        private Secrets _secrets { get; }

        public UserAuthenticationService()
        {
        }

        public UserAuthenticationService(IUserRepository userRepository, Hasher hasher, Salter salter, IOptions<Secrets> secrets)
        {
            _userRepository = userRepository;
            _hasher = hasher;
            _salter = salter;
            _secrets = secrets.Value;
            
        }

        public JwtSecurityToken Authenticate(string providedEmail, string providedPassword)
        {
            User foundUser = _userRepository.FindByEmail(providedEmail);

            if (IsSuccessfullyAuthenticated(providedPassword, foundUser.UserSecurity))
            {
                return new JwtSecurityTokenHandler().CreateToken(CreateTokenDescription(foundUser)) as JwtSecurityToken;
            }
            return null;
        }

        public User GetCurrentLoggedInUser(ClaimsPrincipal principleUser)
        {
            var emailOfAuthenticatedUser = principleUser.FindFirst(ClaimTypes.Email)?.Value;
            return _userRepository.FindByEmail(emailOfAuthenticatedUser);
        }

        public UserSecurity CreateUserSecurity(string userPassword)
        {
            var saltToBeUsed = _salter.CreateRandomSalt();
            return new UserSecurity(
                _hasher.CreateHashOfPasswordAndSalt(userPassword, saltToBeUsed),
                saltToBeUsed);
        }

        private SecurityTokenDescriptor CreateTokenDescription(User foundUser)
        {
            var key = Encoding.ASCII.GetBytes(_secrets.SuperStrongPassword);
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Email, foundUser.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            return tokenDescriptor;
        }

        private bool IsSuccessfullyAuthenticated(string providedPassword, UserSecurity persistedUserSecurity)
        {
            return _hasher.DoesProvidedPasswordMatchPersistedPassword(providedPassword, persistedUserSecurity);
        }
    }
}
