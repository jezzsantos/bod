using System;
using System.Collections.Generic;
using System.Linq;
using ClinicsApplication.ReadModels;
using ClinicsApplication.Storage;
using ClinicsDomain;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny;
using QueryAny.Primitives;
using Storage;
using Storage.Interfaces;

namespace ClinicsStorage
{
    public class ClinicsStorage : IClinicStorage
    {
        private readonly IEventStreamStorage<ClinicEntity> clinicEventStreamStorage;
        private readonly IQueryStorage<Clinic> clinicQueryStorage;
        private readonly IQueryStorage<Unavailability> unavailabilitiesQueryStorage;

        public ClinicsStorage(ILogger logger, IDomainFactory domainFactory,
            IEventStreamStorage<ClinicEntity> eventStreamStorage,
            IRepository repository)
        {
            logger.GuardAgainstNull(nameof(logger));
            domainFactory.GuardAgainstNull(nameof(domainFactory));
            eventStreamStorage.GuardAgainstNull(nameof(eventStreamStorage));
            repository.GuardAgainstNull(nameof(repository));

            this.clinicQueryStorage = new GeneralQueryStorage<Clinic>(logger, domainFactory, repository);
            this.clinicEventStreamStorage = eventStreamStorage;
            this.unavailabilitiesQueryStorage =
                new GeneralQueryStorage<Unavailability>(logger, domainFactory, repository);
        }

        public ClinicsStorage(IQueryStorage<Clinic> clinicQueryStorage,
            IEventStreamStorage<ClinicEntity> clinicEventStreamStorage,
            IQueryStorage<Unavailability> unavailabilitiesQueryStorage)
        {
            clinicQueryStorage.GuardAgainstNull(nameof(clinicQueryStorage));
            clinicEventStreamStorage.GuardAgainstNull(nameof(clinicEventStreamStorage));
            unavailabilitiesQueryStorage.GuardAgainstNull(nameof(unavailabilitiesQueryStorage));
            this.clinicQueryStorage = clinicQueryStorage;
            this.clinicEventStreamStorage = clinicEventStreamStorage;
            this.unavailabilitiesQueryStorage = unavailabilitiesQueryStorage;
        }

        public ClinicEntity Load(Identifier id)
        {
            return this.clinicEventStreamStorage.Load(id);
        }

        public ClinicEntity Save(ClinicEntity clinic)
        {
            this.clinicEventStreamStorage.Save(clinic);
            return clinic;
        }

        public List<Clinic> SearchAvailable(DateTime fromUtc, DateTime toUtc, SearchOptions options)
        {
            var unavailabilities = this.unavailabilitiesQueryStorage.Query(Query.From<Unavailability>()
                    .Where(e => e.From, ConditionOperator.LessThanEqualTo, fromUtc)
                    .AndWhere(e => e.To, ConditionOperator.GreaterThanEqualTo, toUtc))
                .Results;

            var limit = options.Limit;
            var offset = options.Offset;
            options.ClearLimitAndOffset();

            var cars = this.clinicQueryStorage.Query(Query.From<Clinic>()
                    .WhereAll()
                    .WithSearchOptions(options))
                .Results;

            return cars
                .Where(car => unavailabilities.All(unavailability => unavailability.ClinicId != car.Id))
                .Skip(offset)
                .Take(limit)
                .ToList();
        }
    }
}