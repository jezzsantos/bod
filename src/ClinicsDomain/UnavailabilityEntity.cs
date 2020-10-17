using System;
using Domain.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using QueryAny;

namespace ClinicsDomain
{
    [EntityName("Unavailability")]
    public class UnavailabilityEntity : EntityBase
    {
        public UnavailabilityEntity(ILogger logger, IIdentifierFactory idFactory) : base(logger,
            idFactory)
        {
        }

        public Identifier ClinicId { get; private set; }

        public TimeSlot Slot { get; private set; }

        public UnavailabilityCausedBy CausedBy { get; private set; }

        public string CausedByReference { get; private set; }

        public DateTime SlotFrom => Slot.From;

        public DateTime SlotTo => Slot.To;

        protected override void OnEventRaised(IChangeEvent @event)
        {
            switch (@event)
            {
                case Events.Clinic.UnavailabilitySlotAdded added:
                    ClinicId = added.ClinicId.ToIdentifier();
                    Slot = new TimeSlot(added.From, added.To);
                    CausedBy = added.CausedBy;
                    CausedByReference = added.CausedByReference;
                    break;

                default:
                    throw new InvalidOperationException($"Unknown event {@event.GetType()}");
            }
        }
    }
}