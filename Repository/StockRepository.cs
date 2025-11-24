using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Stock;
using api.Helpers;
using api.Interfaces;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace api.Repository
{
    public class StockRepository : IStockRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly IDistributedCache _cache;
        public StockRepository(ApplicationDBContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<Stock> CreateAsync(Stock stockModel)
        {
            await _context.Stocks.AddAsync(stockModel);
            await _context.SaveChangesAsync();
            return stockModel;
        }

        public async Task<Stock?> DeleteAsync(int id)
        {
            var stockModel = await _context.Stocks.FirstOrDefaultAsync(x => x.Id == id);

            if (stockModel == null)
            {
                return null;
            }

            _context.Stocks.Remove(stockModel);
            await _context.SaveChangesAsync();
            await _cache.RemoveAsync($"stock-{id}");
            return stockModel;
        }

        public IAsyncEnumerable<Stock> GetAllAsync(QueryObject query)
        {
            var stocks = _context.Stocks
                .Include(c => c.Comments)
                .ThenInclude(a => a.AppUser)
                .AsNoTracking();

            // Apply filtering
            if (!string.IsNullOrWhiteSpace(query.CompanyName))
            {
                stocks = stocks.Where(s => s.CompanyName.Contains(query.CompanyName));
            }

            if (!string.IsNullOrWhiteSpace(query.Symbol))
            {
                stocks = stocks.Where(s => s.Symbol.Contains(query.Symbol));
            }

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                if (query.SortBy.Equals("Symbol", StringComparison.OrdinalIgnoreCase))
                {
                    stocks = query.IsDecsending
                        ? stocks.OrderByDescending(s => s.Symbol)
                        : stocks.OrderBy(s => s.Symbol);
                }
            }

            // Apply pagination
            var skipNumber = (query.PageNumber - 1) * query.PageSize;
            var pagedStocks = stocks.Skip(skipNumber).Take(query.PageSize);

            return pagedStocks.AsAsyncEnumerable();
        }

        public async Task<Stock?> GetByIdAsync(int id)
        {
            var cacheKey = $"stock-{id}";
            string? cachedStock = await _cache.GetStringAsync(cacheKey);
            Stock? stock;

            if (string.IsNullOrEmpty(cachedStock))
            {
                stock = await _context.Stocks.Include(c => c.Comments).ThenInclude(a => a.AppUser).FirstOrDefaultAsync(i => i.Id == id);
                if (stock != null)
                {
                    var options = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
                    };
                    await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(stock, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), options);
                }
            }
            else
            {
                stock = JsonConvert.DeserializeObject<Stock>(cachedStock);
            }

            return stock;
        }

        public async Task<Stock?> GetBySymbolAsync(string symbol)
        {
            return await _context.Stocks.AsNoTracking().FirstOrDefaultAsync(s => s.Symbol == symbol);
        }

        public Task<bool> StockExists(int id)
        {
            return _context.Stocks.AsNoTracking().AnyAsync(s => s.Id == id);
        }

        public async Task<Stock?> UpdateAsync(int id, UpdateStockRequestDto stockDto)
        {
            var existingStock = await _context.Stocks.FirstOrDefaultAsync(x => x.Id == id);

            if (existingStock == null)
            {
                return null;
            }

            existingStock.Symbol = stockDto.Symbol;
            existingStock.CompanyName = stockDto.CompanyName;
            existingStock.Purchase = stockDto.Purchase;
            existingStock.LastDiv = stockDto.LastDiv;
            existingStock.Industry = stockDto.Industry;
            existingStock.MarketCap = stockDto.MarketCap;

            await _context.SaveChangesAsync();
            await _cache.RemoveAsync($"stock-{id}");

            return existingStock;
        }
    }
}