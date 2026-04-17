using controle_ja_mobile.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
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
                var response = await _apiService.GetAsync<string>($"dashboard/expenses-category?start={start}&end={end}");
                if (!string.IsNullOrWhiteSpace(response))
                {
                    var chartDataResponse = JsonSerializer.Deserialize<List<ChartData>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (chartDataResponse != null)
                    {
                        return chartDataResponse;
                    }
                }
                return new List<ChartData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<ChartData>();
            }
        }

        public async Task<List<ChartData>> getCreditExpensesByCategory(long start, long end)
        {
            try
            {
                var response = await _apiService.GetAsync<string>($"dashboard/credit-expenses-category?start={start}&end={end}");
                if (!string.IsNullOrWhiteSpace(response))
                {
                    var chartDataResponse = JsonSerializer.Deserialize<List<ChartData>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (chartDataResponse != null)
                    {
                        return chartDataResponse;
                    }
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
                var response = await _apiService.GetAsync<string>($"dashboard/incomes-category?start={start}&end={end}");
                if (!string.IsNullOrWhiteSpace(response))
                {
                    var chartDataResponse = JsonSerializer.Deserialize<List<ChartData>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (chartDataResponse != null)
                    {
                        return chartDataResponse;
                    }
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
                var response = await _apiService.GetAsync<string>($"dashboard/fuel-comparison?start={start}&end={end}");
                if (!string.IsNullOrWhiteSpace(response))
                {
                    var chartDataResponse = JsonSerializer.Deserialize<List<ChartData>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (chartDataResponse != null)
                    {
                        return chartDataResponse;
                    }
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
                var response = await _apiService.GetAsync<string>($"dashboard/summary?start={start}&end={end}");
                if (!string.IsNullOrWhiteSpace(response))
                {
                    var financialSummary = JsonSerializer.Deserialize<FinancialSummary>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (financialSummary != null)
                    {
                        return financialSummary;
                    }
                }
                return new FinancialSummary();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new FinancialSummary();
            }
        }

        public async Task<List<ChartData>> getEvolution(long start, long end, string uuid)
        {
            try
            {
                string endpoint = $"dashboard/evolution?start={start}&end={end}";
                if (!string.IsNullOrEmpty(uuid))
                {
                    endpoint += $"&categoryId={uuid}";
                }
                var response = await _apiService.GetAsync<string>(endpoint);

                if (!string.IsNullOrWhiteSpace(response))
                {
                    var chartDataResponse = JsonSerializer.Deserialize<List<ChartData>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (chartDataResponse != null)
                    {
                        return chartDataResponse;
                    }
                }
                return new List<ChartData>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<ChartData>();
            }
        }

        public async Task<DashboardFullSummary?> GetFullSummaryAsync(long start, long end)
        {
            try
            {
                string endpoint = $"dashboard/full-summary?start={start}&end={end}";
                var response = await _apiService.GetAsync<string>(endpoint);

                if (!string.IsNullOrWhiteSpace(response))
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                    };
                    var summary = JsonSerializer.Deserialize<DashboardFullSummary>(response, options);
                    if (summary != null) return summary;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERRO JSON DASHBOARD: " + ex.Message);
                return null;
            }
        }
    }
}