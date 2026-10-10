namespace Sakura.MerkleTrees;

/// <summary>
/// Represents a merkle proof that serves as a proof of reserve for a given user.
/// </summary>
public sealed record MerkleProof(int UserBalance, MerkleProofMetadata[] ProofMetadata);

public sealed record class MerkleProofMetadata(string SiblingHash, SiblingPosition SiblingPosition);
