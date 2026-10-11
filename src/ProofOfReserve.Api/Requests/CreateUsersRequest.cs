namespace Sakura.ProofOfReserve.Api.Requests;

public sealed record CreateUsersRequest(List<UserInput> Users);

public sealed record UserInput(int Id, int Balance);