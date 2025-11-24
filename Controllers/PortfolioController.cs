using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Extensions;
using api.Interfaces;
using api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PortfolioSignalRApp.Hubs;

namespace api.Controllers
{
    [Route("api/portfolio")]
    [ApiController]
    public class PortfolioController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IStockRepository _stockRepo;
        private readonly IPortfolioRepository _portfolioRepo;
        private readonly IFMPService _fmpService;
        private readonly IHubContext<PortfolioHub> _hubContext;
        private readonly IStockService _stockService;

        public PortfolioController(UserManager<AppUser> userManager,
        IStockRepository stockRepo, IPortfolioRepository portfolioRepo,
        IFMPService fmpService,
        IHubContext<PortfolioHub> hubContext, IStockService stockService)
        {
            _userManager = userManager;
            _stockRepo = stockRepo;
            _portfolioRepo = portfolioRepo;
            _fmpService = fmpService;
            _hubContext = hubContext;
            _stockService = stockService;
        }

        [HttpGet]
        [Authorize]
        public async IAsyncEnumerable<Stock> GetUserPortfolio()
        {
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);

            if (appUser != null)
            {
                await foreach (var stock in _portfolioRepo.GetUserPortfolio(appUser))
                {
                    yield return stock;
                }
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddPortfolio(string symbol)
        {
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);

            if (appUser == null)
            {
                return NotFound("User not found.");
            }

            var stock = await _stockService.EnsureStockExistsAsync(symbol);
            if (stock == null) return BadRequest("Stock does not exist"); ;

            var userPortfolio = new List<Stock>();
            await foreach(var portfolioStock in _portfolioRepo.GetUserPortfolio(appUser))
            {
                userPortfolio.Add(portfolioStock);
            }

            if (userPortfolio.Any(e => e.Symbol.ToLower() == symbol.ToLower())) return BadRequest("Cannot add same stock to portfolio");

            var portfolioModel = new Portfolio
            {
                StockId = stock.Id,
                AppUserId = appUser.Id
            };

            await _portfolioRepo.CreateAsync(portfolioModel);

            if (portfolioModel == null)
            {
                return StatusCode(500, "Could not create");
            }
            else
            {
                // 🔥 Kirim update ke semua client via SignalR
                var message = $"{username} baru saja menambahkan {symbol.ToUpper()} ke portofolio nya";
                await _hubContext.Clients.All.SendAsync("ReceivePortfolioUpdate", message);

                return Created();
            }
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeletePortfolio(string symbol)
        {
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);

            if (appUser == null)
            {
                return NotFound("User not found.");
            }

            var userPortfolio = new List<Stock>();
            await foreach(var portfolioStock in _portfolioRepo.GetUserPortfolio(appUser))
            {
                userPortfolio.Add(portfolioStock);
            }

            var filteredStock = userPortfolio.Where(s => s.Symbol.ToLower() == symbol.ToLower()).ToList();

            if (filteredStock.Count() == 1)
            {
                // 🔥 Kirim update ke semua client via SignalR
                var message = $"{username} baru saja menghapus {symbol.ToUpper()} dari portofolio nya";
                await _hubContext.Clients.All.SendAsync("ReceivePortfolioUpdate", message);

                await _portfolioRepo.DeletePortfolio(appUser, symbol);
            }
            else
            {
                return BadRequest("Stock not in your portfolio");
            }

            return Ok();
        }

    }
}