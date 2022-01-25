using CQRS.Mediatr.Lite;
using System.Collections.Generic;

namespace Microsoft.FeatureFlighting.Core.Commands
{
    /// <summary>
    /// Result of rebuilding flights
    /// </summary>
    public class RebuildCommandResult: CommandResult
    {
        public List<string> RebuiltFlights { get; set; }
        public RebuildCommandResult(List<string> rebuiltFlights): base(true, "Rebuild complete")
        {
            RebuiltFlights = rebuiltFlights;
        }
    }
}
