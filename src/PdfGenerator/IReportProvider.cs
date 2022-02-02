using System.Collections.Generic;
using System.Threading;

namespace PdfGenerator;

public interface IReportProvider
{
    IAsyncEnumerable<Report> GetAllRoles(CancellationToken cancellationToken);
}