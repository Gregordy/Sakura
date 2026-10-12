**（日本語版は[こちら](README-jp.md)）**

# Sakura Project structure

Sakura is a .NET solution composed of two projects:
- `MerkleTrees.csproj`: a library providing a base Merkle tree implementation and a specialized implementation.
- `ProofOfReserve.Api.csproj`: a Web API for retrieving user balances and their corresponding Merkle proofs.

> Note: Additionally, they each have a corresponding test project.

## MerkleTrees
The MerkleTrees library provides two implementations of a Merkle tree.

It composed of two main classes:
- `MerkleTree.cs`, the base implementation
- `UserAccountMerkleTree.cs`, a specialized version for user accounts (see the class documentation).

**Meaningful test data** for this project are located in the method named  
`Constructor_WithSpecificData_ShouldReturnAGivenMerkleRoot()` of `MerkleTreeTests.cs`.

To execute all tests of the MerkleTrees project, run this command from the root directory:

    dotnet test tests/MerkleTrees.Tests/MerkleTrees.Tests.csproj

### Dependencies

This project **uses no third-party library** and uses xUnit as its test framework.

## Proof of Reserve API

This Web API exposes two features:
1) Populating a list of users and their balances
2) Retrieving a user balance and its corresponding Merkle proof.

This project uses a specialized implementation of `MerkleTree.cs`.  
Its purpose is to return the Merkle proof that contains a user's balance and additional data.  

The major added method for the new version of the library is `GetMerkleProof()`.

### Dependencies

For this Web API project, I used [Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api)
instead of Controller-based APIs.  
It is a way to have the least amount of dependencies in an API-based project.

This means ASP.NET Core shared framework is the only runtime dependency this project has.

Besides this, and the use of xUnit for its test framework, **no third-party library** is used.

## Additional configuration

The configuration file `xunit.runner.json` included in both `MerkleTrees.Tests.csproj` and
`ProofOfReserve.Api.Tests.csproj` prevents strings from being truncated in the xUnit test output. It is mainly used to
ease debugging while developing.

The rules in `.editorconfig` have been chosen to follow idiomatic .NET conventions.

## 1. Calculation of Merkle Root

The MerkleTrees library performs CPU-bound operations and does not perform I/O.

The Git tag created for the completion of the first version of this library (the one containing only the base
implementation of a merkle tree) is
[v1.0](https://github.com/Gregordy/Sakura/releases/tag/v1.0). 

`MerkleTreeTests.cs` contains tests for it.

### Scalability considerations

Here is a breakdown of the time complexity of different operations.

1) Building the tree requires `O(n)` time as we hash a linear number of nodes.
2) Generating a Merkle proof is a `O(log n)` operation as we need to traverse the tree's height.  
3) Finally, generating proofs for all leaves is a `O(n log n)` because each proof traverses the tree independently.

Once the tree is built, getting the Merkle root is a constant time operation.

## 2. Proof of Reserve Web API

The Proof of Reserve Web API is located in the folder `ProofOfReserve.Api`.

The new Git tag for the updated MerkleTree implementation (**UserAccountMerkleTree.cs**) is 
[v1.1](https://github.com/Gregordy/Sakura/releases/tag/v1.1).  

### Starting the API

To start the Proof of Reserve API, run this command:

    dotnet run --project src/ProofOfReserve.Api/ProofOfReserve.Api.csproj

It will launch on `http://localhost:5066`.

### Testing the API

> Reminder: There are only two features. One using GET, and the other using POST. Both use the "/users" endpoint.

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

To retrieve the Merkle proof and its additional data for a given user, use this next request:

```http
GET http://localhost:5066/users/1
```

> Note: Here, 1 is the user id for which we want the Merkle proof.

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

All of these data can already be observed in the test data of the updated library and those of the API.

Refer to `GetMerkleProof_ForAnExistingUser_ReturnsTheCorrectData()` in `UserAccountMerkleTreeTests.cs`.  
Or to `Get_ProofOfFundsForAnExistingUser_ReturnsOk()` in `ProofOfReserveEndpointTests.cs`.
 
## 3. Suggestions for Improvement

If the Proof of Reserve Web API was offered as an actual service, many improvements should be considered.

### Security

a) **Authentication**

To prevent unauthorized access to the API, requiring either an API token or a user account to access it is preferred.

Also, authorization rules to determine which data a given user can access should be enforced.

b) **Input data validation**

Currently, input data validation is limited.

For a real production service, appropriate validation and request-size limits would help prevent invalid input,
excessive resource consumption, and potential security vulnerabilities.

For this API, limiting the number of users a single request can create will help prevent excessively large operations.

c) **Rate-limiting**

As the web service will be exposed, it will be vulnerable to abuse or excessive traffic.

Restricting the number of requests a client can make within a given time range will be necessary to prevent such abuse.

### Data integrity

a) **Long-term persistence**

Right now, the database is in-memory.

This means data will be lost each time the service restarts or crashes.  
To prevent this, we can replace the in-memory database with persistent storage.

b) **Concurrency and consistency**

Concurrent requests must not produce Merkle roots and proofs that represent different versions of the user data.

To prevent this, tree updates should be applied atomically. Also, the generation of proofs should be done on a
consistent tree snapshot.

For the current in-memory implementation, this may require synchronization.  
If persistent storage is introduced, this will mean considering the appropriate transaction and isolation strategy 
depending on the consistency requirements.

### Scalability

a) **Caching**

Preventing redundant computations of proofs on an unchanged tree could benefit from caching.  
The same applies to rebuilding the tree when no new user was added between requests.

However, caching should be introduced only when performance measurements indicate that it will provide meaningful
benefit.

Cache invalidation must be handled carefully to ensure that cached proofs and Merkle roots remain consistent with the
underlying user data.

b) **Incremental tree update**

Currently, the Merkle tree is rebuilt entirely each time it is requested.

If the tree grows above a certain size, it could be useful to consider implementing incremental tree updates.

### Maintainability

a) **Stronger error handling**

Currently, the error handling is minimal.

For easier debugging and better visibility into the system, states such as duplicate user IDs, missing users, invalid
request data, or other expected failure conditions must be handled precisely to provide end users with meaningful
information.

b) **Persistence abstraction**

Adding an abstraction layer to better separate persistence operations from business logic could improve maintainability
and evolvability if the application needs to support different storage providers in the future.

### API design

Currently, the POST endpoint performs two actions:
- creating users from the provided data
- returning the Merkle root for all the newly created users (in addition to possible existing ones).

It would be better to **split these functionalities between different** endpoints such as:

- `POST /users`, that only handles the user creation and returns an HTTP 201 on success and 400 on failure
- `GET /users/{userId}/balance`, that returns the Merkle proof for the given user
- `GET /users/summary`, which will return the Merkle root for all the currently saved users.

Additionally, integrating OpenAPI will make it easier to use, maintain, and document the API.