namespace Microsoft.FeatureFlighting.Common.Config
{
    /// <summary>
    /// Configuration for parallel processing of feature flags
    /// </summary>
    public class ParallelEvaluationConfiguration
    {
        public static int DefaultBatchSize = 10;

        /// <summary>
        /// Batch size
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// Parallel Mode
        /// </summary>
        /// <remarks>
        /// None - All feature flags to be evaluated synchronously.
        /// Full - Parallel thread for all feature flags.
        /// Batch - Feature flags will be divided into batches and processed in parallel. Each batch will process flag in parallel.
        /// SynchronousParallelGroup - Feature flags will be divided into batches and the batches will be processed in order. All flags in the batch will processed in parallel.
        /// </remarks>
        public string ParallelMode { get; set; }

        /// <summary>
        /// Gets default configuration
        /// </summary>
        /// <returns></returns>
        public static ParallelEvaluationConfiguration GetDefault()
        {
            return new ParallelEvaluationConfiguration
            {
                BatchSize = DefaultBatchSize,
                ParallelMode = "Full"
            };
        }

        public int GetBatchSize()
        {
            if (BatchSize == 0)
                return 1;

            if (BatchSize < 0)
                return int.MaxValue;

            return BatchSize;
        }
    }
}
