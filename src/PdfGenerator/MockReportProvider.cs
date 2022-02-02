using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;

namespace PdfGenerator;

[UsedImplicitly]
internal sealed class MockReportProvider : IReportProvider
{
    private readonly IMemoryCache _cache;
    private readonly Random _random = new(DateTime.UtcNow.Second);

    public MockReportProvider(IMemoryCache cache)
    {
        _cache = cache;
    }

    private async Task<Report> GetData(Guid id, CancellationToken cancellationToken)
    {
        var db = await _cache.GetOrCreateAsync("names", cacheEntry =>
        {
            cacheEntry.SlidingExpiration = TimeSpan.FromSeconds(3);
            cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20);
            return ReadNames("names.json", cancellationToken);
        });

        return new Report(id, GetName(db), _random.Next(1, 1000), _random.Next(1, 1000));
    }

    public async IAsyncEnumerable<Report> GetAllRoles(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (var i = 0; i < 20; i++)
        {
            var guid = Guid.NewGuid();
            yield return await GetData(guid, cancellationToken);
        }
    }

    private string GetName(NamesDatabase db)
    {
        var (maleNames, femaleNames, lastNames) = db;
        if (maleNames == null
            || lastNames == null
            || femaleNames == null
            || !maleNames.Any()
            || !lastNames.Any()
            || !femaleNames.Any())
            throw new InvalidOperationException("DB is invalid");

        return _random.Next(1, 2) == 1
            ? $"{maleNames[_random.Next(0, maleNames.Length - 1)]} {lastNames[_random.Next(0, lastNames.Length - 1)]}"
            : $"{femaleNames[_random.Next(0, femaleNames.Length - 1)]} {lastNames[_random.Next(0, lastNames.Length - 1)]}";
    }

    private static async Task<NamesDatabase> ReadNames(string fileName, CancellationToken cancellationToken)
    {
        await using var fileStream = File.OpenRead(fileName);
        if (fileStream == Stream.Null
            || !fileStream.CanRead)
            throw new InvalidOperationException("Could not open DB file");

        return await JsonSerializer.DeserializeAsync<NamesDatabase>(fileStream, new JsonSerializerOptions
               {
                   AllowTrailingCommas = true,
                   PropertyNameCaseInsensitive = true,
                   PropertyNamingPolicy = JsonNamingPolicy.CamelCase
               }, cancellationToken)
               ?? throw new InvalidOperationException("DB file is invalid");
    }
}