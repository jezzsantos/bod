using System;
using System.Collections.Generic;
using System.Linq;
using Application;
using Application.Resources;
using ApplicationServices;
using ClinicsApplication.Storage;
using ClinicsDomain;
using Domain.Interfaces;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny.Primitives;
using ServiceStack;

namespace ClinicsApplication
{
    public class ClinicsApplication : ApplicationBase, IClinicsApplication
    {
        private readonly IIdentifierFactory idFactory;
        private readonly ILogger logger;
        private readonly IPersonsService personsService;
        private readonly IClinicStorage storage;

        public ClinicsApplication(ILogger logger, IIdentifierFactory idFactory, IClinicStorage storage,
            IPersonsService personsService)
        {
            logger.GuardAgainstNull(nameof(logger));
            idFactory.GuardAgainstNull(nameof(idFactory));
            storage.GuardAgainstNull(nameof(storage));
            personsService.GuardAgainstNull(nameof(personsService));
            this.logger = logger;
            this.idFactory = idFactory;
            this.storage = storage;
            this.personsService = personsService;
        }

        public Clinic Create(ICurrentCaller caller, int year, string make, string model)
        {
            caller.GuardAgainstNull(nameof(caller));

            var owner = this.personsService.Get(caller.Id)
                .ToOwner();

            var car = new ClinicEntity(this.logger, this.idFactory);
            car.SetOwnership(new VehicleOwner(owner.Id));
            car.SetManufacturer(new Manufacturer(year, make, model));

            var created = this.storage.Save(car);

            this.logger.LogInformation("Clinic {Id} was created by {Caller}", created.Id, caller.Id);

            return created.ToCar();
        }

        public Clinic Offline(ICurrentCaller caller, string id, DateTime fromUtc, DateTime toUtc)
        {
            caller.GuardAgainstNull(nameof(caller));
            id.GuardAgainstNullOrEmpty(nameof(id));
            fromUtc.GuardAgainstMinValue(nameof(fromUtc));
            toUtc.GuardAgainstMinValue(nameof(toUtc));

            var car = this.storage.Load(id.ToIdentifier());
            if (id == null)
            {
                throw new ResourceNotFoundException();
            }

            car.Offline(new TimeSlot(fromUtc, toUtc));
            var updated = this.storage.Save(car);

            this.logger.LogInformation("Clinic {Id} was taken offline from {From} until {To}, by {Caller}",
                id, fromUtc, toUtc, caller.Id);

            return updated.ToCar();
        }

        public Clinic Register(ICurrentCaller caller, string id, string jurisdiction, string number)
        {
            caller.GuardAgainstNull(nameof(caller));
            id.GuardAgainstNullOrEmpty(nameof(id));

            var car = this.storage.Load(id.ToIdentifier());
            if (id == null)
            {
                throw new ResourceNotFoundException();
            }

            var plate = new LicensePlate(jurisdiction, number);
            car.Register(plate);
            var updated = this.storage.Save(car);

            this.logger.LogInformation("Clinic {Id} was registered with plate {Plate}, by {Caller}", id, plate,
                caller.Id);

            return updated.ToCar();
        }

        public SearchResults<Clinic> SearchAvailable(ICurrentCaller caller, DateTime fromUtc, DateTime toUtc,
            SearchOptions searchOptions,
            GetOptions getOptions)
        {
            caller.GuardAgainstNull(nameof(caller));

            var cars = this.storage.SearchAvailable(fromUtc, toUtc, searchOptions);

            this.logger.LogInformation("Available carsApplication were retrieved by {Caller}", caller.Id);

            return searchOptions.ApplyWithMetadata(cars
                .ConvertAll(c => WithGetOptions(c.ToCar(), getOptions)));
        }

        // ReSharper disable once UnusedParameter.Local
        private static Clinic WithGetOptions(Clinic clinic, GetOptions options)
        {
            // TODO: expand embedded resources, etc
            return clinic;
        }
    }

    public static class CarConversionExtensions
    {
        public static Clinic ToCar(this ReadModels.Clinic readModel)
        {
            var dto = readModel.ConvertTo<Clinic>();
            dto.Owner = new CarOwner {Id = readModel.VehicleOwnerId};
            dto.Managers = readModel.ManagerIds?.Select(id => new CarManager {Id = id}).ToList();
            dto.Manufacturer = new CarManufacturer
            {
                Year = readModel.ManufactureYear,
                Make = readModel.ManufactureMake,
                Model = readModel.ManufactureModel
            };
            dto.Plate = new CarLicensePlate
                {Jurisdiction = readModel.LicenseJurisdiction, Number = readModel.LicenseNumber};
            return dto;
        }

        public static Clinic ToCar(this ClinicEntity entity)
        {
            var dto = entity.ConvertTo<Clinic>();
            dto.Id = entity.Id;
            dto.Owner = entity.Owner.ToOwner();
            dto.Managers = entity.Managers.ToManagers();

            return dto;
        }

        private static List<CarManager> ToManagers(this VehicleManagers managers)
        {
            return managers.HasValue()
                ? new List<CarManager>(managers.Managers.Select(id => new CarManager {Id = id}))
                : new List<CarManager>();
        }

        private static CarOwner ToOwner(this VehicleOwner owner)
        {
            return owner.HasValue()
                ? new CarOwner {Id = owner}
                : null;
        }

        public static CarOwner ToOwner(this Person person)
        {
            var owner = person.ConvertTo<CarOwner>();

            return owner;
        }
    }
}