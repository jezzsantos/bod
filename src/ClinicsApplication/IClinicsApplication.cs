using System;
using Application.Resources;
using Domain.Interfaces;

namespace ClinicsApplication
{
    public interface IClinicsApplication
    {
        Clinic Create(ICurrentCaller caller, int year, string make, string model);

        SearchResults<Clinic> SearchAvailable(ICurrentCaller caller, DateTime fromUtc, DateTime toUtc,
            SearchOptions searchOptions, GetOptions getOptions);

        Clinic Offline(ICurrentCaller caller, string id, DateTime fromUtc, DateTime toUtc);

        Clinic Register(ICurrentCaller caller, string id, string jurisdiction, string number);
    }
}