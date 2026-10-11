namespace Sakura.ProofOfReserve.Api.Responses;

public abstract record CreateUsersResponse;
public sealed record CreateUsersSuccess(string MerkleRoot) : CreateUsersResponse;
public sealed record CreateUsersFailure(string Error) : CreateUsersResponse;