using Sakura.ProofOfReserve.Api.Requests;
using Sakura.ProofOfReserve.Api.Responses;
using Sakura.ProofOfReserve.Api.Services;

namespace Sakura.ProofOfReserve.Api.Endpoints;

public static class UserEndpoints
{
    public const string BasePath = "/users";

    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        /// The GET endpoint for retrieving the proof of reserve of a given user.
        endpoints.MapGet($"{BasePath}/{{userId:int}}", (int userId, UserService userService) =>
        {
            var merkleProof = userService.ComputeMerkleProofForAccount(userId);

            if (merkleProof is null)
            {
                return Results.NotFound(new GetUserProofOfReserveFailure("No proof of reserve for this user."));
            }

            return Results.Ok(new GetUserProofOfReserveSuccess(merkleProof));
        });

        /// The POST endpoint for creating users and returning the associated merkle root.
        endpoints.MapPost(BasePath, (CreateUsersRequest request, UserService userService) =>
        {
            var userAccounts = userService.CreateUserAccounts(request);

            if (userAccounts is null)
            {
                return Results.NotFound(new CreateUsersFailure("A user with the same id already exists."));
            }

            var merkleTree = userService.ComputeMerkleTreeForAllAccounts();

            // Here, we just created users so we're sure the tree isn't empty
            return Results.Ok(new CreateUsersSuccess(merkleTree!.GetMerkleRoot()));
        });

        return endpoints;
    }
}
