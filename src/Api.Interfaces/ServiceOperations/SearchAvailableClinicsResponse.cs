using System.Collections.Generic;
using Application.Resources;

namespace Api.Interfaces.ServiceOperations
{
    public class SearchAvailableClinicsResponse : SearchOperationResponse
    {
        public List<Clinic> Cars { get; set; }
    }
}