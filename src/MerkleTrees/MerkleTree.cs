using System.Text;
using System.Security.Cryptography;

namespace Sakura.MerkleTrees;

/// <summary>
/// The MerkleTree class represents a merkle tree.
/// </summary>
public class MerkleTree
{    
    /// Represents all the levels of a merkle tree. 
    /// Elements at the first index are the leaf nodes, subsequent ones until the penultimate are the branch nodes, and
    /// the root is a the last index of the list.
    protected readonly List<List<byte[]>> _merkleTreeLevels = [];

    private readonly MerkleTreeHashConfig _merkleTreeHashConfig;

    private const int Sha256HashSize = 32;
    private const int TaggedHashPrefixSize = 2 * Sha256HashSize;
    
    /// The maximum number of transactions we accept in the tree to constrain its memory usage.
    private const int MaxTransactionCount = 50_000;

    public MerkleTree(string[] transactions, string leafTag, string branchTag)
    {
        ThrowIfInvalidTransactions(transactions);

        _merkleTreeHashConfig = new MerkleTreeHashConfig(leafTag, branchTag);
        
        BuildMerkleTree(transactions);
    }

    private void BuildMerkleTree(string[] transactions)
    {
        _merkleTreeLevels.Add(ComputeLeafNodes(transactions));
        _merkleTreeLevels.Add(ComputeBranchNodes(_merkleTreeLevels[0]));

        while (_merkleTreeLevels[^1].Count > 1)
        {
            var currentLevel = _merkleTreeLevels[^1];
            _merkleTreeLevels.Add(ComputeBranchNodes(currentLevel));
        }
    }

    /// <summary>
    /// Returns the root of the merkle tree as a 64 lowercase hexadecimal characters.
    /// 
    /// We use the lower case representation for consistency of results and because it is idiomatic with common Bitcoin
    /// hash representation.
    /// </summary>
    public string GetMerkleRoot()
    {
        var merkleRoot = _merkleTreeLevels[^1][0];

        return Convert.ToHexString(merkleRoot).ToLowerInvariant();
    }

    /// <summary>
    /// Returns the height of the merkle tree.
    /// </summary>
    public int GetHeight()
    {
        return _merkleTreeLevels.Count - 1;
    }

    /// <summary>
    /// Enforces which invarient we consider for the transactions.
    /// </summary>
    private static void ThrowIfInvalidTransactions(string[] transactions)
    {
        ArgumentNullException.ThrowIfNull(transactions);

        if (transactions.Length == 0)
        {
            throw new ArgumentException("At least one transaction is required.", nameof(transactions));
        }

        if (transactions.Any(transaction => transaction is null))
        {
            throw new ArgumentException("Transactions cannot contain null elements.", nameof(transactions));
        }

        if (transactions.Length > MaxTransactionCount)
        {
            throw new ArgumentException($"The max transaction count is {MaxTransactionCount}.", nameof(transactions));
        }
    }

    /// <summary>
    /// Creates and returns the bottom of the merkle tree (leaf nodes) from the provided transactions.
    /// </summary>
    private List<byte[]> ComputeLeafNodes(string[] transactions)
    {
        var leafNodes = new List<byte[]>(transactions.Length);

        foreach (string transaction in transactions)
        {
            byte[] transactionBytes = Encoding.UTF8.GetBytes(transaction);
            var taggedLeaf = CreateTaggedHash(transactionBytes, _merkleTreeHashConfig.LeafHash);

            leafNodes.Add(taggedLeaf);
        }

        return leafNodes;
    }

    /// <summary>
    /// Computes the rest of the merkle tree (all branches until the root) from the provided leafNodes.
    /// </summary>
    private List<byte[]> ComputeBranchNodes(List<byte[]> leafNodes)
    {
        var nextLevel = new List<byte[]>((leafNodes.Count + 1) / 2);

        for (int i = 0; i < leafNodes.Count; i += 2)
        {
            byte[] left = leafNodes[i];

            // Duplicate the last node if it has no right sibling.
            byte[] right = i + 1 < leafNodes.Count ? leafNodes[i + 1] : left;
            byte[] branchNode = new byte[left.Length + right.Length];

            left.CopyTo(branchNode, 0);
            right.CopyTo(branchNode, left.Length);

            var taggedBranch = CreateTaggedHash(branchNode, _merkleTreeHashConfig.BranchHash);

            nextLevel.Add(taggedBranch);
        }

        return nextLevel;
    }

    /// <summary>
    /// Creates a BIP-340 tagged hash using the following pattern: SHA256(SHA256(tag) || SHA256(tag) || message)
    /// </summary>
    private static byte[] CreateTaggedHash(byte[] messageBytes, byte[] tagHash)
    {
        byte[] taggedHashInput = new byte[TaggedHashPrefixSize + messageBytes.Length];

        tagHash.CopyTo(taggedHashInput, 0);
        tagHash.CopyTo(taggedHashInput, Sha256HashSize);
        messageBytes.CopyTo(taggedHashInput, TaggedHashPrefixSize);

        return SHA256.HashData(taggedHashInput);
    }
}    
