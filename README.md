# Bod
[![Build status](https://ci.appveyor.com/api/projects/status/gb2pso0gypfuk8pf?svg=true)](https://ci.appveyor.com/project/JezzSantos/bod)

This project was inspired by the patterns in the Reference Implementation of [QueryAny](https://github.com/jezzsantos/queryany/samples/ri)

# Automated Testing

1. First: [Getting Started](https://github.com/jezzsantos/queryany/wiki/Getting-Started)  for details on what you need installed on your machine.
1. Build the solution.
1. Run all tests in the solution.

> Note: if the integration tests for one of the repositories fail, its likely due to the fact that you don't have that technology installed on your local machine, or that you are not running your IDE as Administrator, and therefore cant start/stop those local services. Refer to step 1 above.

# Local Development, Debugging and Manual Testing

1. First: [Getting Started](https://github.com/jezzsantos/queryany/wiki/Getting-Started)  for details on what you need installed on your machine.
1. You will need to start the Azure CosmosDB emulator on your machine  (from the Start Menu).
1. Ensure that you have manually created a new CosmosDB database called:  `Production`.
1. Start the `CarsApi` and `PersonsApi` projects locally with F5. (A  browser should open to API documentation site for both sites).

To manually test everything is working, or debug the code:

1. Navigate to: [GET https://localhost:5001/clinics/available](https://localhost:5001/clinics/available)  (you should get an empty array of cars in response)
1. Using a tool like PostMan or other REST client, create a new car by  calling: `POST /cars` with a JSON body like this: `{"Year":  2020,"Make": "Honda","Model": "Civic"}`