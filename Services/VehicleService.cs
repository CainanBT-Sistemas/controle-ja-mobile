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
                    var vehiclesResponse = JsonSerializer.Deserialize<List<Vehicle>>(response);
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
                if (!string.IsNullOrWhiteSpace(response))
                {
                    var vehicleResponse = JsonSerializer.Deserialize<Vehicle>(response);
                    if (vehicleResponse != null)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
