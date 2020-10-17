using System;
using System.Collections.Generic;
using ClinicsDomain;
using Domain.Interfaces.Entities;
using Storage.Interfaces.ReadModels;

namespace ClinicsApplication
{
    public class ClinicIdentifierFactory : EntityPrefixIdentifierFactory
    {
        public ClinicIdentifierFactory() : base(new Dictionary<Type, string>
        {
            {typeof(Checkpoint), "ckp"},
            {typeof(ClinicEntity), "cln"},
            {typeof(UnavailabilityEntity), "una"}
        })
        {
        }
    }
}