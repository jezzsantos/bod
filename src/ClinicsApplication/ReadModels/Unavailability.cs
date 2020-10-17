using System;
using ClinicsDomain;
using QueryAny;
using Storage.Interfaces.ReadModels;

namespace ClinicsApplication.ReadModels
{
    [EntityName("Unavailability")]
    public class Unavailability : IReadModelEntity
    {
        public string ClinicId { get; set; }

        public DateTime From { get; set; }

        public DateTime To { get; set; }

        public UnavailabilityCausedBy CausedBy { get; set; }

        public string CausedByReference { get; set; }

        public string Id { get; set; }
    }
}