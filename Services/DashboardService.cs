using controle_ja_mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace controle_ja_mobile.Services
{
    public class DashboardService
    {
        private readonly ApiService _apiService;

        public DashboardService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<ChartData>> getExpensesByCategory(long start, long end)
        {
            try
            {
                var response = await _apiService.GetAsync<HttpResponseMessage>($"dashboard/expenses-category?start={start}&end={end}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ChartData>>() ?? new List<ChartData>();
                }
                return new List<ChartData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<ChartData>();
            }
        }

        public async Task<List<ChartData>> getIncomesByCategory(long start, long end)
        {
            try
            {
                var response = await _apiService.GetAsync<HttpResponseMessage>($"dashboard/incomes-category?start={start}&end={end}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ChartData>>() ?? new List<ChartData>();
                }
                return new List<ChartData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<ChartData>();
            }
        }

        public async Task<List<ChartData>> getFuelComparison(long start, long end)
        {
            try
            {
                var response = await _apiService.GetAsync<HttpResponseMessage>($"dashboard/fuel-comparison?start={start}&end={end}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ChartData>>() ?? new List<ChartData>();
                }
                return new List<ChartData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<ChartData>();
            }
        }

        public async Task<FinancialSummary?> getDashboardSummary(long start, long end)
        {
            try
            {
                var response = await _apiService.GetAsync<HttpResponseMessage>($"dashboard/summary?start={start}&end={end}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<FinancialSummary>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<List<ChartData>> getEvolution(long start, long end, string uuid)
        {
            try
            {
                string endpoint = $"dashboard/evolution?start={start}&end={end}";
                if (string.IsNullOrEmpty(uuid))
                {
                    endpoint += $"&uuid={uuid}";
                }
                var response = await _apiService.GetAsync<HttpResponseMessage>(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ChartData>>() ?? new List<ChartData>();
                }
                return new List<ChartData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<ChartData>();
            }
        }
    }
}
