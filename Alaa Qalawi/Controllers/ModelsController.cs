using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CarModelsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModelsController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IMemoryCache _memoryCache;
        private readonly string _carMakeCsvFilePath;
        public ModelsController(
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment hostingEnvironment,
            IMemoryCache memoryCache,
             string carMakeCsvFilePath)
        {
            _httpClientFactory = httpClientFactory;
            _hostingEnvironment = hostingEnvironment;
            _memoryCache = memoryCache;
            _carMakeCsvFilePath = carMakeCsvFilePath;
            LoadMakeIdDictionary(_carMakeCsvFilePath);  
        }

        private Dictionary<string, int> LoadMakeIdDictionary(string carMakeCsvFilePath)
        {
            try
            {
                if (string.IsNullOrEmpty(carMakeCsvFilePath))
                {
                    throw new ArgumentNullException(nameof(carMakeCsvFilePath), "File path is null or empty.");
                }

                Dictionary<string, int> makeIdDictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                using (var reader = new StreamReader(carMakeCsvFilePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var values = line.Split(',');
                        if (values.Length == 2 && int.TryParse(values[0], out int makeId))
                        {
                            var carMake = values[1].Trim();
                            makeIdDictionary[carMake] = makeId;
                        }
                    }
                }

                return makeIdDictionary;
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading car make IDs from the CSV file.", ex);
            }
        }

        private int GetMakeIdFromCsv(string make)
        {
            if (_memoryCache.TryGetValue("MakeIdDictionary",out Dictionary<string, int> makeIdDictionary))
            {
                if (makeIdDictionary.TryGetValue(make, out int makeId))
                {
                    return makeId;
                }
            }

            makeIdDictionary = LoadMakeIdDictionary(_carMakeCsvFilePath);
            if (makeIdDictionary.TryGetValue(make, out int updatedMakeId))
            {
                return updatedMakeId;
            }

            throw new Exception("Car make not found or unsupported.");
        }

        [HttpGet]
        public IActionResult GetModels(int modelyear, string make)
        {
            try
            {
                int makeId = GetMakeIdFromCsv(make);

                string apiUrl = $"https://vpic.nhtsa.dot.gov/api/vehicles/GetModelsForMakeIdYear/makeId/{makeId}/modelyear/{modelyear}?format=json";
                var httpClient = _httpClientFactory.CreateClient();
                var response = httpClient.GetStringAsync(apiUrl).Result;

                var models = ParseModels(response);

                return Ok(new { Models = models });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        private List<string> ParseModels(string response)
        {
            var modelsList = new List<string>();

            try
            {
                dynamic jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(response);

                if (jsonObject?.Results != null)
                {
                    var results = jsonObject.Results as JArray;

                    modelsList.AddRange(results?.Select(result => result["Model_Name"].ToString()) ?? Enumerable.Empty<string>());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error parsing models from the API response.", ex);
            }

            return modelsList;
        }
    }
}