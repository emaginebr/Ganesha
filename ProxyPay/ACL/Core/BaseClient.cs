using Microsoft.Extensions.Options;
using ProxyPay.DTO.Settings;
using System;

namespace ProxyPay.ACL.Core
{
    public abstract class BaseClient
    {
        protected readonly HttpClient _httpClient;
        protected readonly IOptions<ProxyPaySetting> _nsalesSetting;

        public BaseClient(IOptions<ProxyPaySetting> nsalesSetting)
        {
            _httpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });
            _nsalesSetting = nsalesSetting;
        }
    }
}
