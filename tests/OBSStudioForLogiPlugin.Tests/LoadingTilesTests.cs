namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

public class LoadingTilesTests
{
    [Fact]
    public void Message_NamesWhatIsLoading()
    {
        LoadingTiles tiles = new LoadingTiles("sources");

        Assert.Equal("Loading sources...", tiles.Message);
    }

    [Fact]
    public void ActionParameters_OnePerTileInOrder()
    {
        LoadingTiles tiles = new LoadingTiles("sources");

        String[] parameters = tiles.ActionParameters.ToArray();

        Assert.Equal(LoadingTiles.TileCount, parameters.Length);
        Assert.Equal(Enumerable.Range(0, LoadingTiles.TileCount), parameters.Select(LoadingTiles.TileIndex));
    }

    [Fact]
    public void IsLoading_FollowsBeginAndEnd()
    {
        LoadingTiles tiles = new LoadingTiles("sources");
        Assert.False(tiles.IsLoading);

        tiles.Begin();
        Assert.True(tiles.IsLoading);

        tiles.End();
        Assert.False(tiles.IsLoading);
    }

    [Fact]
    public void End_ReportsWhetherTilesWereShowing()
    {
        LoadingTiles tiles = new LoadingTiles("sources");
        tiles.Begin();

        Assert.True(tiles.End());
        Assert.False(tiles.End());
    }

    // A source can't be mistaken for a tile, or tapping it would do nothing.
    [Theory]
    [InlineData("Camera")]
    [InlineData("loading_0")]
    [InlineData("")]
    [InlineData(null)]
    public void IsTile_ForOtherParameters_ReturnsFalse(String? parameter)
    {
        Assert.False(LoadingTiles.IsTile(parameter!));
        Assert.Equal(-1, LoadingTiles.TileIndex(parameter!));
    }

    [Theory]
    [InlineData("__loading_2")]
    [InlineData("__loading_-1")]
    [InlineData("__loading_x")]
    public void TileIndex_OutOfRangeOrMalformed_ReturnsMinusOne(String parameter)
    {
        Assert.Equal(-1, LoadingTiles.TileIndex(parameter));
    }

    // The Back button takes the top-left slot, so the tiles must fit in the rest of the top row
    // of the smallest grid, the MX Creative Console's three-wide keypad.
    [Fact]
    public void TileCount_FitsBesideBackButtonOnThreeWideKeypad()
    {
        Assert.Equal(2, LoadingTiles.TileCount);
    }

    [Theory]
    [InlineData("Loading sources...", 90)]
    [InlineData("Loading audio sources...", 90)]
    [InlineData("Loading audio sources...", 60)]
    [InlineData("Loading audio sources...", 116)]
    public void FontSize_FitsMessageAcrossTiles(String message, Int32 tileWidth)
    {
        Int32 fontSize = LoadingTiles.FontSize(message, tileWidth);

        Assert.InRange(message.Length * fontSize * 0.6, 0, tileWidth * LoadingTiles.TileCount);
        Assert.InRange(fontSize, 8, tileWidth / 5);
    }

    [Fact]
    public void FontSize_ShortMessage_CappedAtFifthOfButtonWidth()
    {
        Assert.Equal(18, LoadingTiles.FontSize("Loading...", 90));
    }
}
