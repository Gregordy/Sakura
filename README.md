**（日本語は後述します）**

# Sakura Project structure

This .NET solution is composed of two projects:
- `MerkleTrees.csproj` and
- `ProofOfReserve.Api.csproj`.

`MerkleTrees.csproj` is a library whose goal is to compute a merkle tree from a list of strings.

It contains two main classes:
- MerkleTree.cs
- UserAccountMerkleTree.cs.

**Meaningful test data** for this project are located in the method named  
`Constructor_WithSpecificData_ShouldReturnAGivenMerkleRoot()` of `MerkleTreeTests.cs`.

To execute all tests of the MerkleTrees project, run this command from the root directory:

    dotnet test tests/MerkleTrees.Tests/MerkleTrees.Tests.csproj

`ProofOfReserve.Api.csproj` is a Web API with two features:
- populating a list of users and their balances
- retrieving the user balance and the merkle proof.


### Third-party libraries used

The MerkleTrees project **uses no third-party library**.

For the Proof of Reserve Web API project, I used
[Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api) instead of Controller-based APIs.  
It is a way to have the least amount of dependencies in an API-based project.

ASP.NET Core framework (.NET 10) is the only dependency this project has (because of the inclusion of
`Microsoft.NET.Sdk.Web`).  
Besides this, **no third-party libraries** are used.

### Additional configuration

The file  `xunit.runner.json` included in both `MerkleTrees.Tests.csproj.cs` and `ProofOfReserve.Api.Tests.csproj` is to
prevent strings from being truncated in the xUnit test output. It is mainly to ease debugging while developing.

The rules from the `.editorconfig` have been chosen to follow idiomatic .NET conventions.

## 1. Calculation of Merkle Root

The Git tag created for the completion of the library is [v1.0](https://github.com/Gregordy/Sakura/releases/tag/v1.0). 

`MerkleTreeTests.cs` contains tests for it.

### Scalability considerations

The MerkleTrees library is **CPU-bound**.

There is no operation around files nor are API requests performed.  
In short, the library does not wait for any I/O.

Computing the merkle tree and merkle proofs relies entirely on CPU-bound operations.

Building the tree requires `(O(n))` time as we hash a linear number of nodes.  
Generating one proof is a `(O(log n))` operation as we need to traverse the tree's height.  

Verifying one proof is also a `(O(log n))` operation because we recompute hashes along the path (still traversing the
tree's height).  
Finally, generating proofs for all leaves is a `(O(n log n))` because each proof traverses the tree independently.

## 2. Proof of Reserve Web API

The Proof of Reserve Web API is located in the folder `ProofOfReserve.Api`.

The new Git tag for the updated MerkleTree implementation (called **UserAccountMerkleTree.cs**) is 
[v1.1](https://github.com/Gregordy/Sakura/releases/tag/v1.1).  
This implementation extends the base `MerkleTree.cs` and adds features such as:
- `GetMerkleProof()` to return the merkle proof containing the user balance

### Running the API

To run the Proof of Reserve API

    dotnet run --project src/ProofOfReserve.Api/ProofOfReserve.Api.csproj

It will launch on `http://localhost:5066`

### Testing the API

> Reminder: There are only two features. One using GET, and the other using POST. Both use the /users endpoint.

Once the API is started, the in-memory database registered as a singleton in `ProofOfReserve.Api/Program.cs`
will be up.

Seed it with the following POST request:

```http
POST http://localhost:5066/users
Content-Type: application/json

{
  "users": [
    { "id": 1, "balance": 1111 },
    { "id": 2, "balance": 2222 },
    { "id": 3, "balance": 3333 },
    { "id": 4, "balance": 4444 },
    { "id": 5, "balance": 5555 },
    { "id": 6, "balance": 6666 },
    { "id": 7, "balance": 7777 },
    { "id": 8, "balance": 8888 }
  ]
}
```

It will return the following response:

```json
{
  "merkleRoot": "b1231de33da17c23cebd80c104b88198e0914b0463d0e14db163605b904a7ba3"
}
```

To retrieve the merkle proof and its additional data for a given user, use this next query:

```http
GET "http://localhost:5066/users/1"
```

> Note: Here, 1 is the user id for which we want the merkle proof.

The response for this query is as follows.

```json
{
  "merkleProof": {
    "userBalance": 1111,
    "proofMetadata": [
      {
        "siblingHash": "04bd4a356d675cc13ea5b0fc83e0736a3fbf3067980de9e8e0553c934f5906b8",
        "siblingPosition": 1
      },
      {
        "siblingHash": "d185af244042b0fecba7ee16c9933d73b10c5482104538274dd777b6b120eae1",
        "siblingPosition": 1
      },
      {
        "siblingHash": "9d7f79fa8e788d4a32c9c674b67dcfaf0885f539ac2699129e3c4d88c11c76e7",
        "siblingPosition": 1
      }
    ]
  }
}
```

## 3. Suggestions for Improvement

If this web service was to be offered as an actual service, many improvements should be considered.

### Security

a) **Authentication**

To prevent unauthorized access to the API, requiring either an API token or a user account to access it will be needed.

b) **Input data validation**

Right now, input data are not thoroughly checked.

For a real production service, this is dangerous as it could allow attackers to either flood our service with invalid
data or even open security breaches.  
Implementing strong sanitization of input data is paramount to prevent such issues.

Also, limiting the number of users that can be created on a per request basis.

c) **Rate-limiting**

As the web service will be exposed, it will be vulnerable to abuse.

Protecting how many times it can be used in a given time range will be necessary to prevent such abuse.

### API design

Currently, the POST endpoint performs two actions:
- create users from the provided data
- return the merkle root for all the newly created users (in addition to possible existing ones).

It would be better to **split these functionalities between different** endpoints such as:

- `POST /users`, that only handles the user creation and returns an HTTP 201 on success and 400 on failure
- `GET /users/{userId}/balance`, that returns the merkle proof for the given user
- `GET /users/summary`, which will return the merkle root for all the currently saved users.

Additionally, integrating OpenApi will make it easier to use, maintain, and document the API.

### Data integrity

a) **Long-term storage**

Right now, the database is in-memory.

This means that each time the service restarts or crashes, data will be lost.  
To prevent this, we can replace the in-memory database with a persistent storage.

b) **Concurrency**

To prevent lost updates the API will need to make sure that updates happen atomically and in isolation.

### Scalability

a) **Incremental tree update**

Right now, the merkle tree is rebuilt entirely each time it is requested.

b) **Caching**

To prevent recomputing often accessed data such as the merkle tree

Invalidation will be important. When new users are added, the whole merkle tree will need to be recomputed so it is
crucial the clear the previously built tree stored in cache to prevent inconsistencies.

Same thing for the proof of a given user.  
If one of these users becomes a "hot key" (requested many times in a short interval), then serving a cached entry may be
faster than recomputing it each time. 

### Maintainability

a) **Stronger error handling**

Currently, it the error handling is lightweight.

For easier debugging and better visibility into the system, states such as user already created, malformed transactions,
or other input related errors must be handled more precisely to given to the end user more meaningful information about
what went wrong while processing a request.
