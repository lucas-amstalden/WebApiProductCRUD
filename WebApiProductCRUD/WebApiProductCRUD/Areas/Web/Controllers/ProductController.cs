﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiProductCRUD.Areas.Web.ViewModels;
using WebApiProductCRUD.Models;
using WebApiProductCRUD.Repositories;
using WebApiProductCRUD.Services.WebData;
using WebApiProductCRUD.Utils;

namespace WebApiProductCRUD.Areas.Web.Controllers
{
    [Area("Web")]
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductRepository _repository;
        private readonly IWebDataService _apiService;
        private readonly IMapper _mapper;

        public ProductController(IProductRepository repository, IMapper mapper, IWebDataService apiService)
        {
            _repository = repository;
            _mapper = mapper;
            _apiService = apiService;
        }

        public async Task<IActionResult> Index(string? productName = null)
        {
            var result = await _apiService.Get<Product>();
            var items = result.Items.ToArray();
            if (!string.IsNullOrWhiteSpace(productName))
            {
                items = items
                    .Where(x => x.Name
                    .ToLower()
                    .Contains(productName.ToLower()))
                    .ToArray();
            }

            return View(items);

        }

        [HttpGet]
        public async Task<IActionResult> Edit(string? id = null)
        {
            if (string.IsNullOrEmpty(id))
                return View(new ProductVm());

            var result = await _apiService.Get<Product>(id);
            var vm = _mapper.Map<ProductVm>(result.Items.FirstOrDefault());

            vm ??= new ProductVm();
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProductVm vm)
        {
            var model = _mapper.Map<Product>(vm);
            var apiUri = _apiService.ApiEndpoint(typeof(Product)) + $"/{nameof(Edit)}";
            var result = await _apiService.Post<Product>(apiUri, model);

            if (result.Success)
                return RedirectToAction(nameof(Index));

            foreach (var item in result.KeyAndErrorMessages)
                ModelState.AddModelError(item.Key, item.Message);

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return View(nameof(Index));

            var model = (await _apiService.Get<Product>(id)).Items.First();
            var apiUri = _apiService.ApiEndpoint(typeof(Product)) + $"/{nameof(Delete)}";
            var result = await _apiService.Post<Product>(apiUri, model);

            return RedirectToAction(nameof(Index));
        }

    }
}
