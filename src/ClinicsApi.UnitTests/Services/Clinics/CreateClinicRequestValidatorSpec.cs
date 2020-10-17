using Api.Interfaces.ServiceOperations;
using ClinicsApi.Properties;
using ClinicsApi.Services.Clinics;
using ClinicsDomain;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceStack.FluentValidation;

namespace ClinicsApi.UnitTests.Services.Clinics
{
    [TestClass, TestCategory("Unit")]
    public class CreateClinicRequestValidatorSpec
    {
        private CreateClinicRequest dto;
        private CreateClinicRequestValidator validator;

        [TestInitialize]
        public void Initialize()
        {
            this.validator = new CreateClinicRequestValidator();
            this.dto = new CreateClinicRequest
            {
                Year = Manufacturer.MinYear,
                Make = Manufacturer.Makes[0],
                Model = Manufacturer.Models[0]
            };
        }

        [TestMethod]
        public void WhenAllProperties_ThenSucceeds()
        {
            this.validator.ValidateAndThrow(this.dto);
        }

        [TestMethod]
        public void WhenYearIsZero_ThenThrows()
        {
            this.dto.Year = 0;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.CreateCarRequestValidator_InvalidYear);
        }

        [TestMethod]
        public void WhenYearIsTooOld_ThenThrows()
        {
            this.dto.Year = Manufacturer.MinYear - 1;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.CreateCarRequestValidator_InvalidYear);
        }

        [TestMethod]
        public void WhenYearIsTooNew_ThenThrows()
        {
            this.dto.Year = Manufacturer.MaxYear + 1;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.CreateCarRequestValidator_InvalidYear);
        }

        [TestMethod]
        public void WhenMakeIsNull_ThenThrows()
        {
            this.dto.Make = null;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageForNotEmpty();
        }

        [TestMethod]
        public void WhenMakeIsUnknown_ThenThrows()
        {
            this.dto.Make = "unknownmake";

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.CreateCarRequestValidator_InvalidMake);
        }

        [TestMethod]
        public void WhenModelIsNull_ThenThrows()
        {
            this.dto.Model = null;

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageForNotEmpty();
        }

        [TestMethod]
        public void WhenModelIsUnknown_ThenThrows()
        {
            this.dto.Model = "unknownmake";

            this.validator
                .Invoking(x => x.ValidateAndThrow(this.dto))
                .Should().Throw<ValidationException>()
                .WithValidationMessageLike(Resources.CreateCarRequestValidator_InvalidModel);
        }
    }
}