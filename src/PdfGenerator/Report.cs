using System;

namespace PdfGenerator;

public sealed record Report(Guid Id, string Name, int Sales, int PlannedSales);