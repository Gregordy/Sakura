namespace Sakura.MerkleTrees.Tests;

public class UserAccountMerkleTreeTests
{
    private static readonly string ValidTransaction = "(10,500)";
    private static readonly string TransactionWithMissingComma = "(10500)";
    private static readonly string TransactionWithNegativeBalance = "(10,-500)";
    private static readonly string TransactionWithNegativeUserId = "(-10,500)";
    private static readonly int ValidTransactionUserId = 10;
    private static readonly int ValidTransactionBalance = 500;

    private const string LeafTag = "ProofOfReserve_Leaf";
    private const string BranchTag = "ProofOfReserve_Branch";

    [Fact]
    public void Constructor_WithValidTransactions_ShouldBeAValidTree()
    {
        string[] transactions = [ValidTransaction, "(20,800)", "(30,200)"];

        var merkleTree = new UserAccountMerkleTree(transactions, LeafTag, BranchTag);

        Assert.NotNull(merkleTree);
    }

    [Fact]
    public void Constructor_WithManyValidTransactionsAndASingleInvalidDueToMissingComma_RaisesAFormatException()
    {
        Assert.Throws<FormatException>(() =>
        {
            string[] transactions = ["(20,800)", "(30,200)", TransactionWithMissingComma];
            var merkleTree = new UserAccountMerkleTree(transactions, LeafTag, BranchTag);
        });
    }

    [Fact]
    public void Constructor_WithManyValidTransactionsAndASingleInvalidDueToNegativeBalance_RaisesAFormatException()
    {
        Assert.Throws<FormatException>(() =>
        {
            string[] transactions = ["(20,800)", "(30,200)", TransactionWithNegativeBalance];
            var merkleTree = new UserAccountMerkleTree(transactions, LeafTag, BranchTag);
        });
    }

    [Fact]
    public void Constructor_WithManyValidTransactionsAndASingleInvalidDueToNegativeUserId_RaisesAFormatException()
    {
        Assert.Throws<FormatException>(() =>
        {
            string[] transactions = ["(20,800)", "(30,200)", TransactionWithNegativeUserId];
            var merkleTree = new UserAccountMerkleTree(transactions, LeafTag, BranchTag);
        });
    }

    [Fact]
    public void Constructor_WithNoTransaction_RaisesAnArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var merkleTree = new UserAccountMerkleTree(Array.Empty<string>(), LeafTag, BranchTag);
        });
    }

    [Fact]
    public void GetUserBalance_WithThreeValidTransactions_ShouldReturnTheCorrectBalance()
    {
        string[] transactions = [ValidTransaction, "(20,800)", "(30,200)"];

        var merkleTree = new UserAccountMerkleTree(transactions, LeafTag, BranchTag);
        var userBalance = merkleTree.GetUserBalance(ValidTransactionUserId);

        Assert.Equal(ValidTransactionBalance, userBalance);
        Assert.Equal(800, merkleTree.GetUserBalance(20));
        Assert.Equal(200, merkleTree.GetUserBalance(30));
    }

    [Fact]
    public void GetMerkleProof_ForANonExistingUser_RaisesAKeyNotFoundException()
    {
        const int unknownUserId = 40;
        string[] transactions = [ValidTransaction, "(20,800)", "(30,200)"];

        var merkleTree = new UserAccountMerkleTree(transactions, LeafTag, BranchTag);

        Assert.Throws<KeyNotFoundException>(() =>
        {
            var merkleProof = merkleTree.GetMerkleProof(unknownUserId);
        });
    }


    [Fact]
    public void GetMerkleProof_ForAnExistingUser_ReturnsTheCorrectData()
    {
        const int firstUserId = 1;
        const int firstUserBalance = 1111;
        const string expectedMerkleRoot = "b1231de33da17c23cebd80c104b88198e0914b0463d0e14db163605b904a7ba3";

        string[] transactions = [
            "(1,1111)", "(2,2222)", "(3,3333)", "(4,4444)", "(5,5555)", "(6,6666)", "(7,7777)", "(8,8888)"
        ];

        var merkleTree = new UserAccountMerkleTree(transactions, LeafTag, BranchTag);

        var merkleProof = merkleTree.GetMerkleProof(firstUserId);

        Assert.Equal(firstUserBalance, merkleProof.UserBalance);
        Assert.Equal(3, merkleProof.ProofMetadata.Length);

        Assert.Equal("04bd4a356d675cc13ea5b0fc83e0736a3fbf3067980de9e8e0553c934f5906b8", merkleProof.ProofMetadata[0].SiblingHash);
        Assert.Equal("d185af244042b0fecba7ee16c9933d73b10c5482104538274dd777b6b120eae1", merkleProof.ProofMetadata[1].SiblingHash);
        Assert.Equal("9d7f79fa8e788d4a32c9c674b67dcfaf0885f539ac2699129e3c4d88c11c76e7", merkleProof.ProofMetadata[2].SiblingHash);

        Assert.Equal(SiblingPosition.Right, merkleProof.ProofMetadata[0].SiblingPosition);
        Assert.Equal(SiblingPosition.Right, merkleProof.ProofMetadata[1].SiblingPosition);
        Assert.Equal(SiblingPosition.Right, merkleProof.ProofMetadata[2].SiblingPosition);

        Assert.Equal(expectedMerkleRoot, merkleTree.GetMerkleRoot());
    }

}