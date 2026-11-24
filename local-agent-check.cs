#!/usr/bin/env dotnet
#:package Microsoft.Extensions.AI@11.0.1
#:package Microsoft.Extensions.AI.OpenAI@11.0.1

using System.ClientModel;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.AI;
using OpenAI;

var endpoint = Environment.GetEnvironmentVariable("LLAMA_ENDPOINT")
    ?? "http://localhost:9931/v1";

string? model = args.FirstOrDefault();

try
{
    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
    using var json = JsonDocument.Parse(
        await http.GetStringAsync($"{endpoint}/models"));

    var models = json.RootElement.GetProperty("data").EnumerateArray()
        .Select(m => m.GetProperty("id").GetString())
        .ToList();

    model ??= models.FirstOrDefault();
    if (model is null || !models.Contains(model))
        throw new InvalidOperationException(
            $"Model '{model}' not found among {models.Count} available.");
}
catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException
    or JsonException or KeyNotFoundException or InvalidOperationException)
{
    Console.Error.WriteLine($"NOT READY: {ex.Message}");
    return 1;
}

var nonce = Guid.NewGuid().ToString("N")[..8];
var calls = 0;

IChatClient client = new OpenAIClient(
        new ApiKeyCredential("no-key-required"),
        new OpenAIClientOptions { Endpoint = new Uri(endpoint) })
    .GetChatClient(model)
    .AsIChatClient()
    .AsBuilder()
    .UseFunctionInvocation()
    .Build();

var options = new ChatOptions
{
    Tools = [AIFunctionFactory.Create(GetReadinessCode)]
};

using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));

try
{
    var response = await client.GetResponseAsync(
        "Call the tool to get the readiness code. Reply with that code only.",
        options,
        timeout.Token);

    var answered = response.Text.Contains(nonce, StringComparison.Ordinal);
    Console.WriteLine($"Model: {model}");
    Console.WriteLine($"Tool calls: {calls}, code returned: {answered}");
    return calls > 0 && answered ? 0 : 2;
}
catch (Exception ex) when (ex is ClientResultException or HttpRequestException
    or OperationCanceledException)
{
    Console.Error.WriteLine($"REQUEST FAILED: {ex.Message}");
    return 3;
}

[Description("Returns the one-time readiness code.")]
string GetReadinessCode()
{
    calls++;
    return nonce;
}
