using Homework.Repository.Interfaces;
using Homework.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Homework.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly TestDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IDistributedCache _redisCache;

        public UserRepository(TestDbContext context, IMemoryCache cache, IDistributedCache redisCache)
        {
            this._context = context;
            _cache = cache;
            _redisCache = redisCache;
        }
        public async Task<IEnumerable<UserDetail>> GetAllUsersAsync()
        {
            var cacheKey = "GetAllUsersAsync";
            var cachedData = await _redisCache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                var data = JsonSerializer.Deserialize<IEnumerable<UserDetail>>(cachedData);
                return data;
            }

            var users = await _context.UserDetails.ToListAsync();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10));

            _cache.Set(cacheKey, users, cacheOptions);

            return users;
        }

        public async Task<UserDetail> GetByIdAsync(int id)
        {
            var user = await _context.UserDetails.SingleOrDefaultAsync( x => x.UserDetailId == id);
            return  user;
        }
        public async Task<UserDetail> GetByUserNameAsync(string userName)
        {
            var user = await _context.UserDetails.SingleOrDefaultAsync(x => x.Username == userName);
            return user;
        }

        public async Task<bool> AddRefreshToken(RefreshToken refreshToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
            return true;
        }
    }
}
