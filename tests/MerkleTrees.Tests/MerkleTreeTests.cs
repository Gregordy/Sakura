namespace Sakura.MerkleTrees.Tests;

public class MerkleTreeTests
{
    private const string ValidLeafTag = "Leaf_Tag";
    private const string ValidBranchTag = "Branch_Tag";

    private static readonly string[] ValidSingleTransaction = ["aaa"];

    [Fact]
    public void Constructor_WithSpecificData_ShouldReturnAGivenMerkleRoot()
    {
        const string expectedMerkleRoot = "4aa906745f72053498ecc74f79813370a4fe04f85e09421df2d5ef760dfa94b5";
        const string leafAndBranchTag = "Bitcoin_Transaction";

        string[] transactions = ["aaa", "bbb", "ccc", "ddd", "eee"];

        var merkleTree = new MerkleTree(transactions, leafAndBranchTag, leafAndBranchTag);
        var merkleRoot = merkleTree.GetMerkleRoot();

        Assert.Equal(expectedMerkleRoot, merkleRoot);
    }

    [Fact]
    public void Constructor_WithOneValidTransaction_CreatesAValidTree()
    {
        var merkleTree = new MerkleTree(ValidSingleTransaction, ValidLeafTag, ValidBranchTag);

        Assert.NotNull(merkleTree);
        Assert.NotNull(merkleTree.GetMerkleRoot());
        Assert.Equal(1, merkleTree.GetHeight());
    }

    [Fact]
    public void GetHeight_WithSixValidTransactions_ReturnsTheCorrectValue()
    {
        const int expectedHeight = 3;

        var transactions = Enumerable.Range(0, 6)
                .Select(i => $"transaction-{i}")
                .ToArray();

        var merkleTree = new MerkleTree(transactions, ValidLeafTag, ValidBranchTag);

        Assert.Equal(expectedHeight, merkleTree.GetHeight());
    }

    [Fact]
    public void Constructor_WithTwoValidTransactios_CreatesAValidTree()
    {
        string[] transactions = ["aaa", "bbb"];

        var merkleTree = new MerkleTree(transactions, ValidLeafTag, ValidBranchTag);

        Assert.NotNull(merkleTree);
        Assert.NotNull(merkleTree.GetMerkleRoot());
    }

    [Fact]
    public void Constructor_WithMaxValidTransactionsCount_CreatesAValidTree()
    {
        var transactions = Enumerable.Range(0, 50_000)
                .Select(i => $"transaction-{i}")
                .ToArray();

        var merkleTree = new MerkleTree(transactions, ValidLeafTag, ValidBranchTag);

        Assert.NotNull(merkleTree);
        Assert.NotNull(merkleTree.GetMerkleRoot());
    }

    [Fact]
    public void Constructor_WithNoTransaction_RaisesAnArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            var merkleTree = new MerkleTree(null!, ValidLeafTag, ValidBranchTag);
        });
    }

    [Fact]
    public void Constructor_WithAnEmptyTransactionArray_RaisesAnArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
        {    
            var singleNullTransaction = Array.Empty<string>();
            var merkleTree = new MerkleTree(singleNullTransaction, ValidLeafTag, ValidBranchTag);
        });
    }

    [Fact]
    public void Constructor_WithOneNullTransaction_RaisesAnArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
        {    
            var singleNullTransaction = new string[1];
            var merkleTree = new MerkleTree(singleNullTransaction, ValidLeafTag, ValidBranchTag);
        });
    }

    [Fact]
    public void Constructor_WithOnlyNullTransactions_RaisesAnArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var onlyNullTransactions = new string[10];
            var merkleTree = new MerkleTree(onlyNullTransactions, ValidLeafTag, ValidBranchTag);
        });
    }

    [Fact]
    public void Constructor_WithTransactionsExceedingMaxTransactionCount_RaisesAnArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var transactions = Enumerable.Range(0, 50_001)
                .Select(i => $"transaction-{i}")
                .ToArray();

            var merkleTree = new MerkleTree(transactions, ValidLeafTag, ValidBranchTag);
        });
    }

    [Fact]
    public void Constructor_WithAValidTransactionsButEmptyLeafAndBranchTags_RaisesAnArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var merkleTree = new MerkleTree(ValidSingleTransaction, leafTag: string.Empty, string.Empty);
        });
    }

    [Fact]
    public void Constructor_WithAValidTransactionsButAnEmptyLeafTag_RaisesAnArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var merkleTree = new MerkleTree(ValidSingleTransaction, leafTag: string.Empty, ValidBranchTag);
        });
    }

    [Fact]
    public void Constructor_WithAValidTransactionsButAnEmptyBranchTag_RaisesAnArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var merkleTree = new MerkleTree(ValidSingleTransaction, ValidLeafTag, branchTag: string.Empty);
        });
    }

}