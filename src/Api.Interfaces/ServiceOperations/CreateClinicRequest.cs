using ServiceStack;

namespace Api.Interfaces.ServiceOperations
{
    [Route("/clinics", "POST")]
    public class CreateClinicRequest : IReturn<CreateClinicResponse>, IPost
    {
        public int Year { get; set; }

        public string Make { get; set; }

        public string Model { get; set; }
    }
}