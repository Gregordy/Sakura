using System.Text.RegularExpressions;

namespace Sakura.MerkleTrees;

/// <summary>
/// Is a specialized version of the base merkle tree that additionally stores original transactions for later retrieval.
/// </summary>
public sealed class UserAccountMerkleTree : MerkleTree
{    
    internal record UserAccount(int UserId, int Balance, int LeafIndex);

    private static readonly Regex TransactionRegex = new(@"\A\([1-9][0-9]*\,[0-9]+\)\z");

    /// <summary>
    /// Will store user id as a key their associated user account as a value.
    /// </summary>
    private readonly Dictionary<int, UserAccount> _userIdToBalance;

    public UserAccountMerkleTree(string[] transactions, string leafTag, string branchTag)
    : base (transactions, leafTag, branchTag)
    {
        _userIdToBalance = SeedUserAccounts(transactions);
    }

    public MerkleProof GetMerkleProof(int userId)
    {
        int userBalance = GetUserBalance(userId);
        int leafIndex = GetLeafIndex(userId);

        return new MerkleProof(userBalance, BuildProofMetadata(leafIndex));
    }

    /// <summary>
    /// Retrieves the user balance for the given user id, or an exception if the user does not exist.
    /// </summary>
    public int GetUserBalance(int userId)
    {
        if (_userIdToBalance.TryGetValue(userId, out var userAccount))
        {
            return userAccount.Balance;
        }

        throw new KeyNotFoundException($"User {userId} does not exist");
    }

    private int GetLeafIndex(int userId)
    {
        if (_userIdToBalance.TryGetValue(userId, out var userAccount))
        {
            return userAccount.LeafIndex;
        }

        throw new KeyNotFoundException($"User {userId} does not exist");
    }

    /// <summary>
    /// Seed all user accounts from the transactions information.
    /// 
    /// We use the order of the transactions to determine the index of the leaves.
    /// </summary>
    private Dictionary<int, UserAccount> SeedUserAccounts(string[] transactions)
    {
        ThrowIfAnyInvalidTransaction(transactions);

        Dictionary<int, UserAccount> userAccounts = transactions
            .Select((transaction, index) =>
            {
                var transactionData = transaction.Trim('(', ')').Split(',');

                int userId = int.Parse(transactionData[0]);
                int balance = int.Parse(transactionData[1]);

                return new UserAccount(userId, balance, index);
            })
            .ToDictionary(
                userAccount => userAccount.UserId,
                userAccount => userAccount
            );

        return userAccounts;
    }

    /// <summary>
    /// In the context of this tree, a valid transaction must follow this exact pattern (X.Y) where both X and Y are
    /// positive integers
    private static void ThrowIfAnyInvalidTransaction(string[] transactions)
    {
        if (transactions.Any(transaction => !TransactionRegex.IsMatch(transaction)))
        {
            throw new FormatException("At least one transaction has an invalid format.");
        }

        // For brevity, I do not check for duplicate user ids.
    }

    /// <summary>
    /// Build the additional data required for the merkle proof.
    /// </summary>
    private MerkleProofMetadata[] BuildProofMetadata(int leafIndex)
    {
        var proofMetadata = new List<MerkleProofMetadata>();

        int index = leafIndex;

        // Walk from the leaf level towards the root without including it.
        for (int level = 0; level < _merkleTreeLevels.Count - 1; level++)
        {
            var nodes = _merkleTreeLevels[level];

            bool isRightPair = index % 2 == 1;
            int siblingIndex = isRightPair ? index - 1 : index + 1;

            // Duplicate the final node when the level has an odd count.
            if (siblingIndex >= nodes.Count)
            {
                siblingIndex = index;
            }

            string siblingHash = Convert
                .ToHexString(nodes[siblingIndex])
                .ToLowerInvariant();

            proofMetadata.Add(new MerkleProofMetadata(
                siblingHash,
                SiblingPositionExtensions.FromBoolean(!isRightPair)
            ));

            index /= 2; // Move to the parent node's index at the next level.
        }

        return proofMetadata.ToArray();
    }
}    
