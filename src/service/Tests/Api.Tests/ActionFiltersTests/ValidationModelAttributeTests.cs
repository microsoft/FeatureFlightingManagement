using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.PS.Services.FlightingService.Api.ActionFilters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.PS.FlightingService.Api.Tests.ActionFiltersTests
{
    [ExcludeFromCodeCoverage]
    [TestCategory("ValidationModelAttribute")]
    [TestClass]
    public class ValidationModelAttributeTests
    {
        [TestMethod]
        public void Validation_Result_Must_Be_Set_To_False_On_Invalid_Model_State()
        {
            ValidateModelAttribute validationAttr = new ValidateModelAttribute();
            ActionContext context = new ActionContext();
            context.HttpContext = new DefaultHttpContext();
            context.RouteData = new RouteData();
            context.ActionDescriptor = new ActionDescriptor();
            ActionExecutingContext executingContext = new ActionExecutingContext(context, new List<IFilterMetadata>(), new Dictionary<string, object>(), new object());
            executingContext.ModelState.AddModelError("TKEY", "TMessage");
            validationAttr.OnActionExecuting(executingContext);
            Assert.AreEqual(typeof(ValidationFailedResult), executingContext.Result.GetType());
        }

        [TestMethod]
        public void Validation_Error_Must_Be_Set_With_Values()
        {
            ValidationError error = new ValidationError("TField", "TMessage");
            Assert.AreEqual("TField", error.Field);
            Assert.AreEqual("TMessage", error.Message);

            error = new ValidationError("", "TMessage");
            Assert.IsNull(error.Field);
            Assert.AreEqual("TMessage", error.Message);
        }

        [TestMethod]
        public void Validation_Result_Model_Must_Be_Set_With_Values()
        {
            ModelStateDictionary stateDictionary = new ModelStateDictionary();
            stateDictionary.AddModelError("TKEY", "TErrorMessage");
            ValidationResultModel model = new ValidationResultModel(stateDictionary);

            Assert.AreEqual("Validation Failed", model.Message);
            Assert.AreEqual(1, model.Errors.Count);
        }

        [TestMethod]
        public void Validation_Failed_Result_Must_Be_Set_With_Values()
        {
            ModelStateDictionary stateDictionary = new ModelStateDictionary();
            stateDictionary.AddModelError("TKEY", "TErrorMessage");

            ValidationFailedResult failedResultWithDefaultStatus = new ValidationFailedResult(stateDictionary);
            ValidationFailedResult failedResultWithCustomStatus = new ValidationFailedResult(stateDictionary, 403);
            Assert.AreEqual(400, failedResultWithDefaultStatus.StatusCode);
            Assert.AreEqual(403, failedResultWithCustomStatus.StatusCode);
        }
    }
}
