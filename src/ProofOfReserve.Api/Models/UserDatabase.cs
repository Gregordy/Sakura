namespace Sakura.ProofOfReserve.Api.Models;

public sealed class UserDatabase
{
    public List<UserAccount> UserAccounts;

    public UserDatabase()
    {
        UserAccounts = [];
    }

    public void Save(List<UserAccount> userAccounts)
    {
        UserAccounts.AddRange(userAccounts);
    }

    public List<UserAccount> GetAll()
    {
        return UserAccounts;
    }

    public bool ExistsByIds(List<UserAccount> newAccounts)
    {
        List<UserAccount> userAccounts = GetAll();

        return userAccounts.IntersectBy(newAccounts.Select(a => a.Id), a => a.Id).Any();
    }
}