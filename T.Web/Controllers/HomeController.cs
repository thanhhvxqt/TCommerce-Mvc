﻿using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using T.Library.Model.Enum;
using T.Web.Attribute;
using T.Web.Models;
using T.Web.Services.HomePageServices;

namespace T.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomePageService _homePageService;
        //private readonly ISliderItemService _sliderItemService;

        public HomeController(ILogger<HomeController> logger, IHomePageService homePageService/*, ISliderItemService sliderItemService*/)
        {
            _logger = logger;
            _homePageService = homePageService;
            //_sliderItemService = sliderItemService;
        }

        public IActionResult Index()
        {
            return View();
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAllSliderItemAsync()
        //{
        //    var sliderItemList = await _sliderItemService.GetAllSliderItemAsync();

        //    return View(sliderItemList);
        //}

        [HttpGet]
        public async Task<IActionResult> ShowCategoriesOnHomePage()
        {
            var listModel = await _homePageService.ShowCategoriesOnHomePage();
            return Json(new { data = listModel });
        }

        [CustomAuthorizationFilter(RoleName.Customer)]
        public IActionResult Privacy()
        {
            return View();
        }

        [Route("Error/{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}