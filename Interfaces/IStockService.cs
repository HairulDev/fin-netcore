using System;
using api.Models;

namespace api.Interfaces
{
    public interface IStockService
    {
        Task<Stock?> EnsureStockExistsAsync(string symbol);
    }

}

