# PDFGeneration
Very simple PoC for PDF generation with Playwright

There are some tools like [puppeteer](https://github.com/puppeteer/puppeteer) for browser interaction automation.
For .NET on the backend, it is convenient to use another tool - [Playwright](https://playwright.dev/docs/intro).

It has a .NET API over different browsers, so it can be done with PDF generation in a way like this:

```csharp
    private async Task<FileContentResult> Pdf<TModel>(string html)
    {
        // Initial creation, to do that we need the browser be installed
        // Browser installation is a separate topic
        using var playwright = await Playwright.CreateAsync();

        // Here we are using Chromium, but supported:
        // chromium, chrome, edge,firefox, and webkit.
        // We can use the most convenient one.
        await using var browser = await playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions
            {
                Headless = true
            });
        await page.SetContentAsync(html);
        var pdf = await page.PdfAsync();
        return File(pdf, "application/pdf");
    }
```
