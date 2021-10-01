using System;

namespace Microsoft.PS.FlightingService.Domain.Evaluators
{
    public class EvaluationResult
    {
        public bool Result { get; set; }
        public string Message { get; set; }
        public bool IsFaulted { get; set; }
        public double TimeTaken { get; set; }

        public EvaluationResult(bool result)
        {
            Result = result;
            Message = result ? "Evaluation is sucesfull" : "Evaluation was unsuccesfull";
            IsFaulted = false;
        }

        public EvaluationResult(bool result, string message)
        {
            Result = result;
            Message = message;
            IsFaulted = true;
        }
    }
}
