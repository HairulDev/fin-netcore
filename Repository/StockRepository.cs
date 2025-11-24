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
using Microsoft.Extensions.Caching.Memory;

//    1. Caching: Metode GetAllAsync dan GetByIdAsync menggunakan cache dalam memori untuk
//       mengurangi panggilan ke database.
//    2. Invalidasi Cache: Cache akan otomatis dihapus saat ada operasi
//        Create, Update, atau Delete untuk menjaga konsistensi data.
//    3. Optimasi EF Core: Query yang bersifat hanya-baca (read-only) menggunakan .AsNoTracking() untuk mengurangi
//       overhead.

namespace api.Repository
{
    public class StockRepository : IStockRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly IMemoryCache _cache;
        public StockRepository(ApplicationDBContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<Stock> CreateAsync(Stock stockModel)
        {
            await _context.Stocks.AddAsync(stockModel);
            await _context.SaveChangesAsync();
            _cache.Remove("stocks");
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
            _cache.Remove("stocks");
            return stockModel;
        }

        public async Task<List<Stock>> GetAllAsync(QueryObject query)
        {
            var cacheKey = "stocks";
            if (!_cache.TryGetValue(cacheKey, out List<Stock>? stocks))
            {
                stocks = await _context.Stocks.Include(c => c.Comments).ThenInclude(a => a.AppUser).ToListAsync();
                _cache.Set(cacheKey, stocks, TimeSpan.FromMinutes(15));
            }

            var stocksQueryable = (stocks ?? new List<Stock>()).AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.CompanyName))
            {
                stocksQueryable = stocksQueryable.Where(s => s.CompanyName.Contains(query.CompanyName));
            }

            if (!string.IsNullOrWhiteSpace(query.Symbol))
            {
                stocksQueryable = stocksQueryable.Where(s => s.Symbol.Contains(query.Symbol));
            }

            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                if (query.SortBy.Equals("Symbol", StringComparison.OrdinalIgnoreCase))
                {
                    stocksQueryable = query.IsDecsending ? stocksQueryable.OrderByDescending(s => s.Symbol) : stocksQueryable.OrderBy(s => s.Symbol);
                }
            }

            var skipNumber = (query.PageNumber - 1) * query.PageSize;


            return stocksQueryable.Skip(skipNumber).Take(query.PageSize).ToList();
        }

        public async Task<Stock?> GetByIdAsync(int id)
        {
            var cacheKey = $"stock-{id}";
            if (!_cache.TryGetValue(cacheKey, out Stock? stock))
            {
                stock = await _context.Stocks.Include(c => c.Comments).ThenInclude(a => a.AppUser).FirstOrDefaultAsync(i => i.Id == id);
                if (stock != null)
                {
                    _cache.Set(cacheKey, stock, TimeSpan.FromMinutes(15));
                }
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
            _cache.Remove("stocks");
            _cache.Remove($"stock-{id}");

            return existingStock;
        }
    }
}