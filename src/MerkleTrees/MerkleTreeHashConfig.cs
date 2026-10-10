using System.Text;
using System.Security.Cryptography;

namespace Sakura.MerkleTrees;

public sealed class MerkleTreeHashConfig
{
    public readonly byte[] LeafHash;

    public readonly byte[] BranchHash;

    public MerkleTreeHashConfig(string leafTag, string branchTag)
    {
        ArgumentException.ThrowIfNullOrEmpty(leafTag, "A leaf tag must be provided");
        ArgumentException.ThrowIfNullOrEmpty(branchTag, "A branch tag must be provided");
        
        LeafHash = ComputeHash(leafTag);
        BranchHash = ComputeHash(branchTag);
    }

    private byte[] ComputeHash(string tag)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(tag));
    }
}