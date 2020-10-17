using System;
using ServiceStack;

namespace Api.Interfaces.ServiceOperations
{
    [Route("/clinics/available", "GET")]
    public class SearchAvailableClinicsRequest : SearchOperation<SearchAvailableClinicsResponse>
    {
        public DateTime? FromUtc { get; set; }

        public DateTime? ToUtc { get; set; }
    }
}