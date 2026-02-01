using controle_ja_mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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
                var response = await _apiService.GetAsync<string>("categories");
                if (!string.IsNullOrWhiteSpace(response))
                {
                    var categorysResponse = JsonSerializer.Deserialize<List<Category>>(response);
                    if (categorysResponse != null)
                    {
                        return categorysResponse;
                    }
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
                var response = await _apiService.PostAsync<string>("categories", category);
                if (!string.IsNullOrWhiteSpace(response))
                {
                    var categoryResponse = JsonSerializer.Deserialize<Category>(response);
                    if (categoryResponse != null)
                    {
                        return categoryResponse;
                    }
                }
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
