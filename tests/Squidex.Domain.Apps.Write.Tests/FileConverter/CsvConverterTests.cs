﻿// ==========================================================================
//  CsvConverterTests.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using DataAccess;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Write.Apps;
using Squidex.Domain.Apps.Write.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Write.FileConverter
{
    public class CsvConverterTests : HandlerTestBase<AppDomainObject>
    {
        private const string AssetGuid = "22661bbb-ea42-40f2-8c7f-85fd94154b3f";
        private const string MasterLanguage = "en";
        private const string CorrectHeader = "name,tags,asset\r\n";
        private const string IncorrectHeader = "wrong,tags,asset\r\n";
        private const string CorrectFormat = "testname,testtags,\r\n";
        private const string ExtraDataHeader = "name,tags,asset,\"\"";
        private const string ExtraData = "name,tags,asset,extra\r\n";
        private const string EmptyNameField = ",empty tags,\r\n";
        private const string EmptyRow = ",,\r\n";
        private const string MultipleTags = "\"tags,with,commas\"";
        private readonly string correctFormat2 = $"name2,{MultipleTags},{AssetGuid}\r\n";
        private readonly CsvConverter sut;
        private readonly IFormFile fileMock = A.Fake<IFormFile>();
        private readonly ISchemaEntity schema = A.Fake<ISchemaEntity>();
        private readonly IAppEntity app = A.Fake<IAppEntity>();
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.Create(Language.EN);

        public CsvConverterTests()
        {
            sut = new CsvConverter();
            var schemaDef =
                Schema.Create("my-schema", new SchemaProperties())
                    .AddOrUpdateField(new StringField(1, "name", Partitioning.Invariant,
                        new StringFieldProperties { IsRequired = true }))
                    .AddOrUpdateField(new TagField(2, "tags", Partitioning.Language,
                        new TagFieldProperties()))
                    .AddOrUpdateField(new AssetsField(3, "asset", Partitioning.Language,
                        new AssetsFieldProperties()));

            A.CallTo(() => app.LanguagesConfig).Returns(languagesConfig);
            A.CallTo(() => app.PartitionResolver).Returns(languagesConfig.ToResolver());
            A.CallTo(() => schema.SchemaDef).Returns(schemaDef);
        }

        [Fact]
        public void Should_return_null_if_file_empty()
        {
            var retVal = sut.ReadWithSchema(schema, fileMock, MasterLanguage);

            Assert.Null(retVal);
        }

        [Fact]
        public void Should_return_null_if_a_row_has_too_many_entries()
        {
            CreateFile(false, false, true);
            var retVal = sut.ReadWithSchema(schema, fileMock, MasterLanguage);

            Assert.Null(retVal);
        }

        [Fact]
        public void Should_return_null_if_header_row_is_does_not_match_schema_field_names()
        {
            CreateFile(true, false, false);
            var retVal = sut.ReadWithSchema(schema, fileMock, MasterLanguage);

            Assert.Null(retVal);
        }

        [Fact]
        public void Should_return_json_with_content_data_with_null_required_field_and_continues()
        {
            CreateFile(false, true, false);
            var retVal = sut.ReadWithSchema(schema, fileMock, MasterLanguage);

            var containsNullField = retVal.Contains("\"iv\":null");
            var containsMultipleTags = retVal.Contains("\"en\":[\"tags\",\"with\",\"commas\"]");
            var containsReference = retVal.Contains($"\"en\":[\"{AssetGuid}\"]");
            Assert.True(containsNullField);
            Assert.True(containsMultipleTags);
            Assert.True(containsReference);
        }

        [Fact]
        public void Should_return_json_with_all_correct_data()
        {
            CreateFile(false, false, false);
            var retVal = sut.ReadWithSchema(schema, fileMock, MasterLanguage);

            var containsNoNullFields = !retVal.Contains("\"iv\":null");
            Assert.True(containsNoNullFields);
        }

        private void CreateFile(bool wrongFormat, bool emptyRequiredField, bool extraData)
        {
            var ms = GetStream(wrongFormat, emptyRequiredField, extraData);
            A.CallTo(() => fileMock.Length).Returns(1);
            A.CallTo(() => fileMock.OpenReadStream()).Returns(ms);
        }

        private MemoryStream GetStream(bool wrongFormat, bool emptyRequiredField, bool extraData)
        {
            var ms = new MemoryStream();
            var content = wrongFormat
                ? $"{IncorrectHeader}{CorrectFormat}"
                : emptyRequiredField
                    ? $"{CorrectHeader}{CorrectFormat}{EmptyNameField}{correctFormat2}"
                    : extraData
                        ? $"{ExtraDataHeader}{EmptyRow}{ExtraData}"
                        : $"{CorrectHeader}{EmptyRow}{CorrectFormat}";
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            return ms;
        }
    }
}
