﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using T.Library.Model.Common;
using T.Library.Model.Enum;
using T.Library.Model.Response;
using T.WebApi.Attribute;
using T.WebApi.Services.CategoryServices;

namespace T.WebApi.Controllers
{
    [Route("api/category")]
    [ApiController]
    [CustomAuthorizationFilter(RoleName.Admin)]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet(APIRoutes.GetAll)]
        public async Task<ActionResult> GetAll() 
        {
            return Ok(await _categoryService.GetAllCategoryAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<Category>>> Get(int id)
        {
            return await _categoryService.GetCategoryByIdAsync(id);
        }

        [HttpPost(APIRoutes.AddOrEdit)]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<ActionResult> CreateOrEdit(Category category)
        {
            var result = await _categoryService.CreateOrEditAsync(category);
            if(!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("delete/{id}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<ActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteCategoryByIdAsync(id);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
