using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ProxyPay.DTO.AbacatePay;
using ProxyPay.DTO.Settings;
using ProxyPay.Infra.Interfaces.AppServices;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ProxyPay.Infra.AppServices
{
    public class AbacatePayAppService : IAbacatePayAppService
    {
        private readonly AbacatePaySetting _settings;

        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        public AbacatePayAppService(IOptions<AbacatePaySetting> settings)
        {
            _settings = settings.Value;
        }

        private HttpClient CreateClient()
        {
            var client = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
            return client;
        }

        public async Task<AbacatePayResponse<BillingInfo>> CreateBillingAsync(BillingCreateRequest request)
        {
            using var client = CreateClient();
            var content = new StringContent(
                JsonConvert.SerializeObject(request, _jsonSettings),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync($"{_settings.ApiUrl}/v1/billing/create", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AbacatePayResponse<BillingInfo>>(json);
        }

        public async Task<AbacatePayResponse<PixQrCodeInfo>> CreatePixQrCodeAsync(PixQrCodeCreateRequest request)
        {
            using var client = CreateClient();
            var content = new StringContent(
                JsonConvert.SerializeObject(request, _jsonSettings),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync($"{_settings.ApiUrl}/v1/pixQrCode/create", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AbacatePayResponse<PixQrCodeInfo>>(json);
        }

        public async Task<AbacatePayResponse<PixQrCodeStatusInfo>> CheckStatusAsync(string id)
        {
            using var client = CreateClient();
            var response = await client.GetAsync($"{_settings.ApiUrl}/v1/pixQrCode/check?id={id}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AbacatePayResponse<PixQrCodeStatusInfo>>(json);
        }

        public async Task<AbacatePayResponse<PixQrCodeInfo>> SimulatePaymentAsync(string id)
        {
            using var client = CreateClient();
            var content = new StringContent("{}", Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{_settings.ApiUrl}/v1/pixQrCode/simulate-payment?id={id}", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AbacatePayResponse<PixQrCodeInfo>>(json);
        }
    }
}
