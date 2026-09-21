using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cortiqa.Sdk.Exceptions;
using Cortiqa.Sdk.Models;

namespace Cortiqa.Sdk.Services
{
    public class ModelsService
    {
        private readonly CortiqaClient _client;

        public ModelsService(CortiqaClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<ModelListResponse> ListAsync(CancellationToken cancellationToken = default)
        {
            var response = await _client.SendWithRetryAsync(
                HttpMethod.Get,
                "/api/v1/ai/models",
                null,
                cancellationToken).ConfigureAwait(false);

            string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = JsonSerializer.Deserialize<ModelListResponse>(responseBody);
            return result ?? throw new CortiqaException("Failed to deserialize models list response.");
        }
    }
}
