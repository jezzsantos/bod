using Application.Resources;
using ApplicationServices;

namespace ClinicsApi.IntegrationTests
{
    public class StubPersonsService : IPersonsService
    {
        public Person Get(string id)
        {
            return new Person
            {
                Id = id,
                Name = new PersonName
                {
                    FirstName = "afirstname",
                    LastName = "alastname"
                }
            };
        }
    }
}