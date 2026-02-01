using controle_ja_mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace controle_ja_mobile.Services
{
    public class CategoryService
    {
        private readonly ApiService _apiService;

        public CategoryService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<HttpResponseMessage>("categories");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Category>>() ?? new List<Category>();
                }
                return new List<Category>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Category>();
            }
        }

        public async Task<Category> SaveCategoryAsync(Category category)
        {
            try
            {
                var response = await _apiService.PostAsync<HttpResponseMessage>("categories", category);
                //if (response.IsSuccessStatusCode)
                //{
                //    return await response.Content.ReadFromJsonAsync<Category>() ?? new Category();
                //}
                return new Category();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new Category();
            }
        }
    }
}
