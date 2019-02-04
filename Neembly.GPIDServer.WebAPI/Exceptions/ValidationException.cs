using System;
using System.Collections.Generic;
using FluentValidation.Results;
using System.Linq;

namespace Neembly.GPIDServer.WebAPI.Exceptions
{
    public class ValidationException : Exception
    {
        #region Constructor
        public ValidationException()
            : base("One or more rules validation not followed.")
        {
            Failures = new Dictionary<string, string[]>();
        }
        #endregion

        #region Properties
        public IDictionary<string, string[]> Failures { get; }
        #endregion

        #region Methods
        public ValidationException(List<ValidationFailure> failures)
            : this()
        {
            var propertyNames = failures
                .Select(e => e.PropertyName)
                .Distinct();

            foreach (var propertyName in propertyNames)
            {
                var propertyFailures = failures
                    .Where(e => e.PropertyName == propertyName)
                    .Select(e => e.ErrorMessage)
                    .ToArray();

                Failures.Add(propertyName, propertyFailures);
            }
        }
        #endregion 
    }
}
