﻿using RavenDB.AspNetCore.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Linq;
using Raven.Client.UniqueConstraints;
using Omu.ValueInjecter;

namespace RavenDB.AspNetCore.Identity.Stores
{
    public class RavenUserStore<TUser, TRole> :
        IUserLoginStore<TUser>,
        IUserRoleStore<TUser>,
        IUserClaimStore<TUser>,
        IUserPasswordStore<TUser>,
        IUserSecurityStampStore<TUser>,
        IUserEmailStore<TUser>,
        IUserLockoutStore<TUser>,
        IUserPhoneNumberStore<TUser>,
        IQueryableUserStore<TUser>,
        IUserTwoFactorStore<TUser>,
        IQueryableRoleStore<TRole>, 
        IRoleClaimStore<TRole>,
        IUserAuthenticationTokenStore<TUser>
        where TRole : RavenRole
        where TUser : RavenUser
    {
        //private readonly IDocumentStore _store;
        private readonly IAsyncDocumentSession _session;

        public RavenUserStore(IAsyncDocumentSession session)
        {
            //_store = store;

            _session = session;// _store.OpenAsyncSession();
        }



        //private readonly Dictionary<string, TUser> _logins = new Dictionary<string, TUser>();

        //private readonly Dictionary<string, TUser> _users = new Dictionary<string, TUser>();

        public IQueryable<TUser> Users
        {
            get
            {
                return _session.Query<TUser>();
            }
        }

        public Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            var claims = user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
            return Task.FromResult<IList<Claim>>(claims);
        }

        public Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var claim in claims)
            {
                user.Claims.Add(new RavenUserClaim<string> { ClaimType = claim.Type, ClaimValue = claim.Value });
            }
            return Task.FromResult(0);
        }

        public Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default(CancellationToken))
        {
            var matchedClaims = user.Claims.Where(uc => uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type).ToList();
            foreach (var matchedClaim in matchedClaims)
            {
                matchedClaim.ClaimValue = newClaim.Value;
                matchedClaim.ClaimType = newClaim.Type;
            }
            return Task.FromResult(0);
        }

        public Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var claim in claims)
            {
                var entity = user.Claims.FirstOrDefault(uc => uc.ClaimType == claim.Type && uc.ClaimValue == claim.Value);
                if (entity != null)
                {
                    user.Claims.Remove(entity);
                }
            }
            return Task.FromResult(0);
        }

        public Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.Email = email;
            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(user.Email);
        }

        public Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(user.NormalizedEmail);
        }

        public Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.FromResult(0);
        }


        public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public async Task<TUser> FindByEmailAsync(string email, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                return await _session.LoadByUniqueConstraintAsync<TUser>(t => t.NormalizedEmail, email);
            }
            catch
            {
                return null;
            }
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(user.LockoutEnd);
        }

        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.LockoutEnd = lockoutEnd;
            return Task.FromResult(0);
        }

        public Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.AccessFailedCount++;
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.AccessFailedCount = 0;
            return Task.FromResult(0);
        }

        public Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(user.LockoutEnabled);
        }

        public Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.LockoutEnabled = enabled;
            return Task.FromResult(0);
        }

        private string GetLoginKey(string loginProvider, string providerKey)
        {
            return loginProvider + "|" + providerKey;
        }

        public virtual Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.Logins.Add(new RavenUserLogin<string>
            {
                UserId = user.Id,
                ProviderKey = login.ProviderKey,
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.ProviderDisplayName
            });
            //_logins[GetLoginKey(login.LoginProvider, login.ProviderKey)] = user;
            return Task.FromResult(0);
        }

        public Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var loginEntity =
                user.Logins.SingleOrDefault(
                    l =>
                        l.ProviderKey == providerKey && l.LoginProvider == loginProvider &&
                        l.UserId == user.Id);
            if (loginEntity != null)
            {
                user.Logins.Remove(loginEntity);
            }
            //_logins[GetLoginKey(loginProvider, providerKey)] = null;
            return Task.FromResult(0);
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            IList<UserLoginInfo> result = user.Logins
                .Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.ProviderDisplayName)).ToList();
            return Task.FromResult(result);
        }

        public Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            string key = GetLoginKey(loginProvider, providerKey);
            //if (_logins.ContainsKey(key))
            //{
            //    return Task.FromResult(_logins[key]);
            //}
            return Task.FromResult<TUser>(null);
        }

        public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(user.Id);
        }

        public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.UserName = userName;
            return Task.FromResult(0);
        }

        public async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _session.StoreAsync(user);
            await _session.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dbUser = await _session.LoadAsync<TUser>(user.Id);

            dbUser.InjectFrom(user);

            await _session.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _session.LoadAsync<TUser>(userId, cancellationToken);
        }

        public void Dispose()
        {
        }

        public async Task<TUser> FindByNameAsync(string userName, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                return await _session.LoadByUniqueConstraintAsync<TUser>(t => t.NormalizedUserName, userName);
            }
            catch
            {
                return null;
            }
        }

        public async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dbUser = await _session.LoadAsync<TUser>(user.Id, cancellationToken);
            if (user == null || dbUser == null)
            {
                throw new InvalidOperationException("Unknown user");
            }
            _session.Delete(dbUser);
            await _session.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.PhoneNumber = phoneNumber;
            return Task.FromResult(0);
        }

        public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.PhoneNumberConfirmed = confirmed;
            return Task.FromResult(0);
        }

        // RoleId == roleName for InMemory
        public async Task AddToRoleAsync(TUser user, string role, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dbRole = await _session.LoadAsync<TRole>($"Roles/{role}");

            if (dbRole != null)
            {
                user.Roles.Add(dbRole.Id);
            }
        }

        // RoleId == roleName for InMemory
        public async Task RemoveFromRoleAsync(TUser user, string role, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dbRole = await _session.LoadAsync<TRole>($"Roles/{role}");

            user.Roles.Remove(dbRole.Id);
        }

        public async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            await Task.FromResult(0);
            return user.Roles.Select(t => t.Substring(6)).ToList();
        }

        public Task<bool> IsInRoleAsync(TUser user, string role, CancellationToken cancellationToken = default(CancellationToken))
        {
            bool result = user.Roles.Contains($"Roles/{role}");
            return Task.FromResult(result);
        }

        public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(user.SecurityStamp);
        }

        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.TwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }

        public Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task SetNormalizedUserNameAsync(TUser user, string userName, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.NormalizedUserName = userName;
            return Task.FromResult(0);
        }

        // RoleId == rolename for inmemory store tests
        public async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (String.IsNullOrEmpty(roleName))
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            return await Users.Where(t => t.Roles.Contains($"Roles/{roleName}")).ToListAsync();
        }

        public async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            var query = from user in Users
                        where user.Claims.Where(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value).FirstOrDefault() != null
                        select user;

            return await query.ToListAsync();
        }

        public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _session.StoreAsync(role, cancellationToken);
            await _session.SaveChangesAsync(cancellationToken);
            
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dbRole = _session.LoadAsync<TRole>(role.Id);
            if (role == null || dbRole == null)
            {
                throw new InvalidOperationException("Unknown role");
            }
            _session.Delete(dbRole);
            await _session.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(role.Id);
        }

        public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(role.Name);
        }

        public Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            role.Name = roleName;
            return Task.FromResult(0);
        }

        public async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            await _session.StoreAsync(role, cancellationToken);
            await _session.SaveChangesAsync(cancellationToken);

            return IdentityResult.Success;
        }

        async Task<TRole> IRoleStore<TRole>.FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return await _session.LoadAsync<TRole>(roleId);
        }

        async Task<TRole> IRoleStore<TRole>.FindByNameAsync(string roleName, CancellationToken cancellationToken)
        {
            return await _session.LoadAsync<TRole>($"Roles/{roleName}");
        }

        public Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            var claims = role.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
            return Task.FromResult<IList<Claim>>(claims);
        }

        public Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            role.Claims.Add(new RavenRoleClaim<string> { ClaimType = claim.Type, ClaimValue = claim.Value, RoleId = role.Id });
            return Task.FromResult(0);
        }

        public Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            var entity =
                role.Claims.FirstOrDefault(
                    ur => ur.RoleId == role.Id && ur.ClaimType == claim.Type && ur.ClaimValue == claim.Value);
            if (entity != null)
            {
                role.Claims.Remove(entity);
            }
            return Task.FromResult(0);
        }

        public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(role.NormalizedName);
        }

        public Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken = default(CancellationToken))
        {
            role.NormalizedName = normalizedName;
            return Task.FromResult(0);
        }

        public Task SetTokenAsync(TUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            var tokenEntity =
                user.Logins.SingleOrDefault(
                    l =>
                        l.ProviderDisplayName == name && l.LoginProvider == loginProvider &&
                        l.UserId == user.Id);
            if (tokenEntity != null)
            {
                tokenEntity.ProviderKey = value;
            }
            else
            {
                user.Logins.Add(new RavenUserLogin<string>
                {
                    UserId = user.Id,
                    LoginProvider = loginProvider,
                    ProviderDisplayName = name,
                    ProviderKey = value
                });
            }
            return Task.FromResult(0);
        }

        public Task RemoveTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            var tokenEntity =
                user.Logins.SingleOrDefault(
                    l =>
                        l.ProviderDisplayName == name && l.LoginProvider == loginProvider &&
                        l.UserId == user.Id);
            if (tokenEntity != null)
            {
                user.Logins.Remove(tokenEntity);
            }
            return Task.FromResult(0);
        }

        public Task<string> GetTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            var tokenEntity =
                user.Logins.SingleOrDefault(
                    l =>
                        l.ProviderDisplayName == name && l.LoginProvider == loginProvider &&
                        l.UserId == user.Id);
            return Task.FromResult(tokenEntity?.ProviderKey);
        }

        public IQueryable<TRole> Roles
        {
            get { return _session.Query<TRole>(); }
        }
    }
}