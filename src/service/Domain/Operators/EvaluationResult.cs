using System.Text;
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

        public EvaluationResult(bool isSuccess, string message)
        {
            Result = isSuccess;
            Message = message;
        }

        public EvaluationResult(bool isSuccess, Operator @operator, string filter)
        {
            Result = isSuccess;
            Message = CreateMessage(isSuccess, @operator, filter);
            IsFaulted = false;
        }

        public EvaluationResult(bool result, Operator @operator, string filter, string additionalMessage)
        {
            Result = result;
            Message = CreateMessage(false, @operator, filter, additionalMessage);
        }

        public static EvaluationResult CreateFaultedResult(bool isEvaluationSuccesfull, string faultMessage, Operator @operator, string filter)
        {
            return new EvaluationResult(isEvaluationSuccesfull, @operator, filter, faultMessage)
            {
                IsFaulted = true
            };
        }

        private string CreateMessage(bool isSuccess, Operator @operator, string filter, string additionalMessage = null)
        {
            StringBuilder messageBuilder = new();
            messageBuilder.Append(@operator.ToString());
            messageBuilder.Append(isSuccess ? " operator passed for " : " operator failed for ");
            messageBuilder.Append(filter);
            messageBuilder.Append(".");
            if (!string.IsNullOrWhiteSpace(additionalMessage))
            {
                messageBuilder.Append(" Message - ");
                messageBuilder.Append(additionalMessage);
            }
            return messageBuilder.ToString();
        }
    }
}
