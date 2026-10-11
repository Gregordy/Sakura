using Sakura.MerkleTrees;

namespace Sakura.ProofOfReserve.Api.Responses;

public abstract record GetUserProofOfReserveResponse;
public sealed record GetUserProofOfReserveSuccess(MerkleProof MerkleProof) : GetUserProofOfReserveResponse;
public sealed record GetUserProofOfReserveFailure(string Error) : GetUserProofOfReserveResponse;