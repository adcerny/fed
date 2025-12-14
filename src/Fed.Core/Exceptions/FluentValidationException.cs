using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fed.Core.Exceptions
{
    public class FluentValidationException : Exception
    {
        public FluentValidationException(ValidationResult validationResult) : base ()
        {
            ValidationResult = validationResult;
        }
        public ValidationResult ValidationResult { get; set; }
    }
}
