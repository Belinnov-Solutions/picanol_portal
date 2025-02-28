using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Picanol.Services
{
    [Serializable]
    public class ErrorData
    {
        /// <summary>
        /// Name of property that have validation messages. 
        /// </summary>
        public string Property { get; set; }
        /// <summary>
        /// Id of control at client side.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// List of validation messages.
        /// </summary>
        public List<string> ErrorMessages { get; set; }
    }
    public static class StringExtension
    {
        public static string InvariantFormat(this string value, params object[] args)
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, value, args);
        }
    }
    public class ModelStateWrapper : iValidation
    {
        public ModelStateWrapper() : this(new ModelStateDictionary()) { }
        private System.Web.Mvc.ModelStateDictionary modelState;
        public ModelStateWrapper(ModelStateDictionary modelState)
        {
            if (modelState == null)
                throw new ArgumentNullException("modelState");

            this.modelState = modelState;
        }
        public IEnumerable<ErrorData> GetErrorList()
        {
            return GetErrorList(string.Empty);
        }
        public IEnumerable<ErrorData> GetErrorList(string prefix)
        {
            string propertyFormat = string.IsNullOrWhiteSpace(prefix) ? @"{0}{1}" : @"{0}.{1}";
            string idFormat = string.IsNullOrWhiteSpace(prefix) ? @"{0}{1}" : @"{0}_{1}";
            return this.modelState.Where((state) => state.Value.Errors.Count > 0)
                                    .Select((state) =>
                                    {
                                        var errorData = new ErrorData
                                        {
                                            Property = propertyFormat.InvariantFormat(prefix,

HtmlHelper.GenerateIdFromName(state.Key)),
                                            Id = idFormat.InvariantFormat(prefix, HtmlHelper.GenerateIdFromName

(state.Key)),
                                            ErrorMessages = state.Value.Errors.Select(error =>

error.ErrorMessage).ToList<string>()
                                        };
                                        return errorData;
                                    });
        }
        public void AddError(string key, string errorMessage)
        {
            this.modelState.AddModelError(key, errorMessage);
        }
        public bool isValid
        {
            get { return this.modelState.IsValid; }
        }

    }
}