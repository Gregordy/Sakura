namespace Sakura.MerkleTrees;

public enum SiblingPosition
{
    Left = 0,
    Right = 1
}

public static class SiblingPositionExtensions
{
    public static SiblingPosition FromBoolean(bool siblingPosition)
    {
        return siblingPosition ? SiblingPosition.Right : SiblingPosition.Left;
    }
}
