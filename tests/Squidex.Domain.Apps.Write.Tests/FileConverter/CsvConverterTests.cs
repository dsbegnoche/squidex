// ==========================================================================
//  CsvConverterTests.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Squidex.Domain.Apps.Read.Schemas;
using Xunit;

namespace Squidex.Domain.Apps.Write.FileConverter
{
    public class CsvConverterTests
    {
        private readonly CsvConverter sut;
        private readonly string masterLanguage = "en";
        private readonly IFormFile emptyFile = A.Fake<IFormFile>();
        private readonly ISchemaEntity schema = A.Fake<ISchemaEntity>();

        public CsvConverterTests()
        {
            sut = new CsvConverter();
        }

        [Fact]
        public async Task Should_return_null_if_file_empty()
        {
            var retVal = await sut.ReadWithSchemaAsync(schema, emptyFile, masterLanguage);

            Assert.Null(retVal);
        }
    }
}
