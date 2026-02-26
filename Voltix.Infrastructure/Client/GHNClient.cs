using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Voltix.Domain.ValueObjects;
using Voltix.Infrastructure.IClient;
using System.Text;

namespace Voltix.Infrastructure.Client
{
    public class GHNClient : IGHNClient
    {
        private readonly IConfiguration _cfg;
        private readonly HttpClient _http;
        private static string? _baseUrl;
        public GHNClient(IConfiguration cfg, HttpClient http)
        {
            _cfg = cfg;
            _http = http;
            _baseUrl = _cfg["GHN:BaseUrl"];
        }

        private HttpRequestMessage JsonRequest(HttpMethod method, string path, object? payload = null)
        {
            var request = new HttpRequestMessage(method, _baseUrl + path);
            request.Headers.Add("Token", _cfg["GHN:Token"] ?? throw new ArgumentNullException("GHN:Token is not exist"));
            if (payload is not null)
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            }

            return request;
        }

        public async Task<GhnResult<List<GhnDistrict>>> GetDistrictsAsync(int provinceId, CancellationToken ct = default)
        {
            var request = JsonRequest(HttpMethod.Get, $"/district?province_id={provinceId}", ct);
            var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

            var json = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                return GhnResult<List<GhnDistrict>>.Fail($"GHN API error: {response.StatusCode} - {json}");
            }
            return JsonConvert.DeserializeObject<GhnResult<List<GhnDistrict>>>(json) ?? GhnResult<List<GhnDistrict>>.Fail("Deserialize error");
        }

        public async Task<GhnResult<List<GhnProvince>>> GetProvincesAsync(CancellationToken ct = default)
        {
            var request = JsonRequest(HttpMethod.Get, $"/province");
            var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

            var json = await response.Content.ReadAsStringAsync(ct); 
            if (!response.IsSuccessStatusCode)
            {
                return GhnResult<List<GhnProvince>>.Fail($"GHN API error: {response.StatusCode} - {json}");
            }
            return JsonConvert.DeserializeObject<GhnResult<List<GhnProvince>>>(json) ?? GhnResult<List<GhnProvince>>.Fail("Deserialize error");
        }


        public async Task<GhnResult<List<GhnWard>>> GetWardsAsync(int districtId, CancellationToken ct = default)
        {
            var request = JsonRequest(HttpMethod.Get, $"/ward?district_id={districtId}", ct);
            var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

            var json = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
            {
                return GhnResult<List<GhnWard>>.Fail($"GHN API error: {response.StatusCode} - {json}");
            }
            return JsonConvert.DeserializeObject<GhnResult<List<GhnWard>>>(json) ?? GhnResult<List<GhnWard>>.Fail("Deserialize error");
        }


        private HttpRequestMessage AbsoluteGet(string absoluteUrl)
        {
            return new HttpRequestMessage(HttpMethod.Get, absoluteUrl);
        }


        public Task<ProvincesOpenGetWardResponse> ProvincesOpenGetWardResponse(string provinceCode, CancellationToken ct = default)
        {
            var request = AbsoluteGet($"https://provinces.open-api.vn/api/v2/p/{provinceCode}?depth=2");
            var response = _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).Result;
            var json = response.Content.ReadAsStringAsync(ct).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Provinces Open API error: {response.StatusCode} - {json}");
            }
            return Task.FromResult(JsonConvert.DeserializeObject<ProvincesOpenGetWardResponse>(json) ?? throw new Exception("Deserialize error"));
        }


        public Task<List<ProvincesOpenGetProvinceResponse>> ProvincesOpenGetProvinceResponse(CancellationToken ct = default)
        {
            var request = AbsoluteGet($"https://provinces.open-api.vn/api/v2/p/");
            var response = _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct).Result;

            var json = response.Content.ReadAsStringAsync(ct).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Provinces Open API error: {response.StatusCode} - {json}");
            }
            return Task.FromResult(JsonConvert.DeserializeObject<List<ProvincesOpenGetProvinceResponse>>(json) ?? throw new Exception("Deserialize error"));
        }
    }
}
