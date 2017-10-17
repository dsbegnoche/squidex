// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Schemas.Validators;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class MultiField : Field<MultiFieldProperties>
    {
        public MultiField(long id, string name, Partitioning partitioning, MultiFieldProperties properties)
            : base(id, name, partitioning, properties)
        {
        }

        public override T Accept<T>(IFieldVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override object ConvertValue(JToken value)
        {
            return value;
        }

        protected override IEnumerable<IValidator> CreateValidators()
        {
            if (Properties.AllowedValues != null)
            {
                yield return new AllowedValuesValidatorMultiple<string>(Properties.AllowedValues.ToArray());
            }
        }
    }
}
