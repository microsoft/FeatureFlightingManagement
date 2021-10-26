using Microsoft.FeatureFlighting.Core.FeatureFilters;

namespace Microsoft.FeatureFlighting.Core.Operators
{
    /// <summary>
    /// Result of evaluating a feature flag
    /// </summary>
    public class EvaluationResult
    {
        /// <summary>
        /// True if feature flag is true
        /// </summary>
        public bool Result { get; set; }
        
        /// <summary>
        /// Message about the evaluation
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Indicates that evaluation faulted
        /// </summary>
        public bool IsFaulted { get; set; }
        
        /// <summary>
        /// Time taken to evaluate the feature flag
        /// </summary>
        public double TimeTaken { get; set; }

        public EvaluationResult(bool result)
        {
            Result = result;
            Message = result ? "Evaluation is sucesfull" : "Evaluation was unsuccesfull";
            IsFaulted = false;
        }

        public EvaluationResult(bool result, Operator @operator, string filter)
        {
            Result = result;
            Message = result ? $"{@operator} operator passed for {filter}" : $"{@operator} operator failed for {filter}";
            IsFaulted = false;
        }

        public EvaluationResult(bool result, string message)
        {
            Result = result;
            Message = message;
            IsFaulted = true;
        }

        public EvaluationResult(bool result, string message, Operator @operator, string filter)
        {
            Result = result;
            Message = $"{@operator} operator failed for {filter}. Message - {message}";
            IsFaulted = true;
        }
    }
}
