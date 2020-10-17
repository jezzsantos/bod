using ServiceStack;

namespace Api.Interfaces.ServiceOperations
{
    [Route("/clinics/{Id}/register", "PUT")]
    public class RegisterClinicRequest : IReturn<RegisterClinicResponse>, IPut
    {
        public string Id { get; set; }

        public string Jurisdiction { get; set; }

        public string Number { get; set; }
    }
}