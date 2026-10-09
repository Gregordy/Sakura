namespace Sakura.ProofOfReserve.Api.Endpoints;

public static class UserEndpoints
{
    private const string Base_Path = "/users/";

    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(Base_Path, () => "test");
        endpoints.MapPost(Base_Path, () => "test");

        return endpoints;
    }
}
