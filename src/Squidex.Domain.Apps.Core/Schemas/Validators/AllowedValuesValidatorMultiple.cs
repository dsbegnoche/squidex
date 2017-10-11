// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Core.Schemas.Validators
{
    public sealed class AllowedValuesValidatorMultiple<T> : IValidator
    {
        private readonly T[] allowedValues;

        public AllowedValuesValidatorMultiple(params T[] allowedValues)
        {
            Guard.NotNull(allowedValues, nameof(allowedValues));

            this.allowedValues = allowedValues;
        }

        public Task ValidateAsync(object value, ValidationContext context, Action<string> addError)
        {
            if (value == null)
            {
                return TaskHelper.Done;
            }

            var typedValue = ((JArray)value).ToObject<T[]>();

            if (typedValue.Any(val => !allowedValues.Contains(val)))
            {
                addError("<FIELD> is not an allowed value");
            }

            return TaskHelper.Done;
        }
    }
}