using CQRS.Mediatr.Lite;
using Microsoft.FeatureFlighting.Common.Model;

namespace Microsoft.FeatureFlighting.Core.Commands
{
    /// <summary>
    /// Result for <see cref="GenerateReportCommand"/>
    /// </summary>
    public class ReportCommandResult : CommandResult
    {
        /// <summary>
        /// Generated report for the tenant
        /// </summary>
        public UsageReportDto Report { get; private set; }

        public ReportCommandResult(UsageReportDto report) : base(true, "Report generated")
        {
            Report = report;
        }

        public ReportCommandResult(string message): base(false, message)
        {
            Report = null;
        }
    }
}
