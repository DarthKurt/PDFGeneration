using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Playwright;

namespace PdfGenerator.Controllers;

[Route("[controller]")]
public sealed class ReportController : Controller
{
    private readonly IReportProvider _reportProvider;
    private readonly IRazorViewEngine _razorViewEngine;
    private readonly ITempDataProvider _tempDataProvider;

    public ReportController(IReportProvider reportProvider, IRazorViewEngine razorViewEngine,
        ITempDataProvider tempDataProvider)
    {
        _reportProvider = reportProvider;
        _razorViewEngine = razorViewEngine;
        _tempDataProvider = tempDataProvider;
    }

    [HttpGet]
    [Route("json")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IAsyncEnumerable<Report>), StatusCodes.Status200OK)]
    public IActionResult GetJson(CancellationToken cancellationToken)
        => Ok(_reportProvider.GetAllRoles(cancellationToken));

    [HttpGet]
    [Route("html")]
    [Produces(MediaTypeNames.Text.Html)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHtml(CancellationToken cancellationToken)
    {
        var reports = await _reportProvider.GetAllRoles(cancellationToken).ToArrayAsync(cancellationToken);
        return View("~/Pages/Report/Index.cshtml", reports);
    }

    [HttpGet]
    [Route("pdf")]
    [Produces(MediaTypeNames.Text.Html)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPdf(CancellationToken cancellationToken)
    {
        var reports = await _reportProvider.GetAllRoles(cancellationToken).ToArrayAsync(cancellationToken);
        return await Pdf("~/Pages/Report/Index.cshtml", reports);
    }

    private async Task<FileContentResult> Pdf<TModel>(string view, TModel model)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions
            {
                Headless = true
            });
        var page = await browser.NewPageAsync();
        var html = await RenderViewToStringAsync(ControllerContext, view, model);
        await page.SetContentAsync(html);
        var pdf = await page.PdfAsync();
        return File(pdf, "application/pdf");
    }

    private async Task<string> RenderViewToStringAsync<TModel>(ActionContext actionContext, string viewPath, TModel model)
    {
        var viewEngineResult = _razorViewEngine.GetView(viewPath, viewPath, false);

        if (viewEngineResult.View == null || !viewEngineResult.Success)
            throw new ArgumentNullException($"Unable to find view '{viewPath}'");

        var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), actionContext.ModelState)
        {
            Model = model
        };

        var view = viewEngineResult.View;
        var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);

        await using var sw = new StringWriter();
        var viewContext = new ViewContext(actionContext, view, viewDictionary, tempData, sw, new HtmlHelperOptions());
        await view.RenderAsync(viewContext);
        return sw.ToString();
    }
}