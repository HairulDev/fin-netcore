using System;
using api.Interfaces;
using api.Models;

namespace api.Service
{
    public class StockService : IStockService
    {
        private readonly IStockRepository _stockRepo;
        private readonly IFMPService _fmpService;

        public StockService(IStockRepository stockRepo, IFMPService fmpService)
        {
            _stockRepo = stockRepo;
            _fmpService = fmpService;
        }

        public async Task<Stock?> EnsureStockExistsAsync(string symbol)
        {
            var stock = await _stockRepo.GetBySymbolAsync(symbol);

            if (stock == null)
            {
                stock = await _fmpService.FindStockBySymbolAsync(symbol);
                if (stock == null)
                    return null;

                await _stockRepo.CreateAsync(stock);
            }

            return stock;
        }
    }

}

