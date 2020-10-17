using Api.Common.Validators;
using Api.Interfaces.ServiceOperations;
using ClinicsApi.Properties;
using ClinicsDomain;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using QueryAny.Primitives;
using ServiceStack.FluentValidation;

namespace ClinicsApi.Services.Clinics
{
    internal class RegisterClinicRequestValidator : AbstractValidator<RegisterClinicRequest>
    {
        public RegisterClinicRequestValidator(IIdentifierFactory identifierFactory)
        {
            RuleFor(dto => dto.Id)
                .IsEntityId(identifierFactory)
                .WithMessage(Resources.AnyValidator_InvalidId);
            RuleFor(dto => dto.Jurisdiction)
                .NotEmpty();
            When(dto => dto.Jurisdiction.HasValue(), () =>
            {
                RuleFor(dto => dto.Jurisdiction)
                    .Matches(Validations.Clinic.Jurisdiction.Expression)
                    .Must(dto => LicensePlate.Jurisdictions.Contains(dto))
                    .WithMessage(Resources.RegisterCarRequestValidator_InvalidJurisdiction);
            });
            RuleFor(dto => dto.Number)
                .NotEmpty();
            When(dto => dto.Number.HasValue(), () =>
            {
                RuleFor(dto => dto.Number)
                    .Matches(Validations.Clinic.Number.Expression)
                    .WithMessage(Resources.RegisterCarRequestValidator_InvalidNumber);
            });
        }
    }
}