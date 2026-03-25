using controle_ja_mobile.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace controle_ja_mobile.Services
{
    public class VehicleService
    {
        private readonly ApiService _apiService;

        public VehicleService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<Vehicle>> GetVehiclesAsync()
        {
            try
            {
                var response = await _apiService.GetAsync<string>("vehicles");
                if (!string.IsNullOrWhiteSpace(response))
                {
                    var vehiclesResponse = JsonSerializer.Deserialize<List<Vehicle>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (vehiclesResponse != null)
                    {
                        return vehiclesResponse;
                    }
                }
                return new List<Vehicle>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<Vehicle>();
            }
        }

        public async Task<bool> SaveVehicleAsync(Vehicle vehicle)
        {
            try
            {
                var response = await _apiService.PostAsync<string>("vehicles", vehicle);
                return !string.IsNullOrWhiteSpace(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        // --- NOVOS MÉTODOS ADICIONADOS ---

        public async Task<bool> UpdateVehicleAsync(string id, Vehicle vehicle)
        {
            try
            {
                var response = await _apiService.PutAsync<string>($"vehicles/{id}", vehicle);
                return !string.IsNullOrWhiteSpace(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteVehicleAsync(string id)
        {
            try
            {
                var response = await _apiService.DeleteAsync($"vehicles/{id}");
                return !string.IsNullOrWhiteSpace(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}