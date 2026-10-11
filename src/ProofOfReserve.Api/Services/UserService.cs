using Sakura.MerkleTrees;
using Sakura.ProofOfReserve.Api.Models;
using Sakura.ProofOfReserve.Api.Requests;

namespace Sakura.ProofOfReserve.Api.Services;

public class UserService(UserDatabase UserDatabase)
{
    private const string LeafTag = "ProofOfReserve_Leaf";
    private const string BranchTag = "ProofOfReserve_Branch";
    
    /// <summary>
    /// Turns all the user information into valid user account records of the form "(10,500)".
    /// </summary>
    public List<UserAccount> CreateUserAccounts(CreateUsersRequest request)
    {
        List<UserAccount> userAccounts = request.Users
            .Select(user => new UserAccount(user.Id, user.Balance))
            .ToList();

        if (UserDatabase.ExistsByIds(userAccounts))
        {
            return null!;
        }

        UserDatabase.Save(userAccounts);

        return userAccounts;
    }

    /// <summary>
    /// Computes the merkle root for all user accounts.
    /// </summary>
    public UserAccountMerkleTree? ComputeMerkleTreeForAllAccounts()
    {
        UserAccount[] userAccounts = [.. UserDatabase.GetAll()];

        if (userAccounts is null || userAccounts.Length == 0)
        {
            return null;
        }

        string[] serializedUserAccounts = userAccounts
            .Select(user => $"({user.Id},{user.Balance})")
            .ToArray();

        return new UserAccountMerkleTree(serializedUserAccounts, LeafTag, BranchTag);
    }

    public MerkleProof? ComputeMerkleProofForAccount(int userId)
    {
        var merkleTree = ComputeMerkleTreeForAllAccounts();

        if (merkleTree is null)
        {
            return null;
        }

        return merkleTree.GetMerkleProof(userId);
    }
    
}