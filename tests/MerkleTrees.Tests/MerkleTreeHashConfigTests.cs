namespace Sakura.MerkleTrees.Tests;

public class MerkleTreeHashConfigTests
{
    private const string ValidTag = "Test_Tag";

    [Fact]
    public void Constructor_WithTwoDifferentValidTags_CreatesAValidConfig()
    {
        var anotherValidTag = "Test_Tag2";

        var config = new MerkleTreeHashConfig(ValidTag, anotherValidTag);

        Assert.NotNull(config);
        Assert.NotEmpty(config.LeafHash);
        Assert.NotEmpty(config.BranchHash);
        Assert.NotEqual(config.LeafHash, config.BranchHash);
    }

    [Fact]
    public void Constructor_WithTwoSameValidTags_CreatesAValidConfig()
    {
        var config = new MerkleTreeHashConfig(ValidTag, ValidTag);

        Assert.NotNull(config);
        Assert.NotEmpty(config.LeafHash);
        Assert.NotEmpty(config.BranchHash);
        Assert.Equal(config.LeafHash, config.BranchHash);
    }

    [Fact]
    public void Constructor_WithEmptyTags_RaisesAnArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var config = new MerkleTreeHashConfig(string.Empty, string.Empty);
        });
    }

    [Fact]
    public void Constructor_WithNullTags_RaisesAnArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            var config = new MerkleTreeHashConfig(null!, null!);
        });
    }

    [Fact]
    public void Constructor_WithAValidLeafTagButANullBranchTag_RaisesAnArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            var config = new MerkleTreeHashConfig(ValidTag, branchTag: null!);
        });
    }

    [Fact]
    public void Constructor_WithANullLeafTagAndAValidBranchTag_RaisesAnArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            var config = new MerkleTreeHashConfig(leafTag: null!, branchTag: ValidTag);
        });
    }

    [Fact]
    public void Constructor_WithAValidLeafTagButAEmptyBranchTag_RaisesAnArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var config = new MerkleTreeHashConfig(ValidTag, branchTag: string.Empty);
        });
    }

    [Fact]
    public void Constructor_WithAnEmptyLeafTagAndAValidBranchTag_RaisesAnArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var config = new MerkleTreeHashConfig(leafTag: string.Empty, branchTag: ValidTag);
        });
    }
}