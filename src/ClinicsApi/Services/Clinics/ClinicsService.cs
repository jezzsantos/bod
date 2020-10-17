using System;
using Api.Common;
using Api.Interfaces;
using Api.Interfaces.ServiceOperations;
using Application.Resources;
using ClinicsApplication;
using QueryAny.Primitives;
using ServiceStack;

namespace ClinicsApi.Services.Clinics
{
    internal class ClinicsService : Service
    {
        private readonly IClinicsApplication clinicsApplication;

        public ClinicsService(IClinicsApplication clinicsApplication)
        {
            clinicsApplication.GuardAgainstNull(nameof(clinicsApplication));

            this.clinicsApplication = clinicsApplication;
        }

        public SearchAvailableClinicsResponse Get(SearchAvailableClinicsRequest request)
        {
            var fromUtc = request.FromUtc.GetValueOrDefault(DateTime.MinValue);
            var toUtc = request.ToUtc.GetValueOrDefault(DateTime.MaxValue);
            var available = this.clinicsApplication.SearchAvailable(Request.ToCaller(), fromUtc, toUtc,
                request.ToSearchOptions(defaultSort: Reflector<Clinic>.GetPropertyName(c => c.Id)),
                request.ToGetOptions());
            return new SearchAvailableClinicsResponse
            {
                Cars = available.Results,
                Metadata = available.Metadata
            };
        }

        public CreateClinicResponse Post(CreateClinicRequest request)
        {
            return new CreateClinicResponse
            {
                Clinic = this.clinicsApplication.Create(Request.ToCaller(), request.Year, request.Make, request.Model)
            };
        }

        public RegisterClinicResponse Put(RegisterClinicRequest request)
        {
            return new RegisterClinicResponse
            {
                Clinic = this.clinicsApplication.Register(Request.ToCaller(), request.Id, request.Jurisdiction,
                    request.Number)
            };
        }

        public OfflineClinicResponse Put(OfflineClinicRequest request)
        {
            return new OfflineClinicResponse
            {
                Clinic = this.clinicsApplication.Offline(Request.ToCaller(), request.Id, request.FromUtc, request.ToUtc)
            };
        }
    }
}