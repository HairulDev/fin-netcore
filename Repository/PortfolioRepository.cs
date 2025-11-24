using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace api.Repository
{
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly IDistributedCache _cache;
        public PortfolioRepository(ApplicationDBContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<Portfolio> CreateAsync(Portfolio portfolio)
        {
            await _context.Portfolios.AddAsync(portfolio);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync($"portfolio-{portfolio.AppUserId}");
            return portfolio;
        }

        public async Task<Portfolio?> DeletePortfolio(AppUser appUser, string symbol)
        {
            var portfolioModel = await _context.Portfolios.FirstOrDefaultAsync(x => x.AppUserId == appUser.Id && x.Stock.Symbol.ToLower() == symbol.ToLower());

            if (portfolioModel == null)
            {
                return null;
            }

            _context.Portfolios.Remove(portfolioModel);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync($"portfolio-{appUser.Id}");
            return portfolioModel;
        }

        public async IAsyncEnumerable<Stock> GetUserPortfolio(AppUser user)
        {
            var cacheKey = $"portfolio-{user.Id}";
            string? cachedPortfolio = await _cache.GetStringAsync(cacheKey);
            List<Stock> portfolio;

            if (string.IsNullOrEmpty(cachedPortfolio))
            {
                var portfolioFromDb = _context.Portfolios.Where(u => u.AppUserId == user.Id)
                    .AsNoTracking()
                    .Select(stock => new Stock
                    {
                        Id = stock.StockId,
                        Symbol = stock.Stock.Symbol,
                        CompanyName = stock.Stock.CompanyName,
                        Purchase = stock.Stock.Purchase,
                        LastDiv = stock.Stock.LastDiv,
                        Industry = stock.Stock.Industry,
                        MarketCap = stock.Stock.MarketCap
                    });

                var portfolioList = new List<Stock>();
                await foreach (var stock in portfolioFromDb.AsAsyncEnumerable())
                {
                    portfolioList.Add(stock);
                    yield return stock;
                }

                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                };
                await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(portfolioList), options);
            }
            else
            {
                portfolio = JsonConvert.DeserializeObject<List<Stock>>(cachedPortfolio);
                foreach (var stock in portfolio)
                {
                    yield return stock;
                }
            }
        }
    }
}