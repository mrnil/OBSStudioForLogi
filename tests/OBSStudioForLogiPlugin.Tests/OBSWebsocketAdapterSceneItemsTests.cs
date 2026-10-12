namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using Newtonsoft.Json.Linq;

/// <summary>
/// The adapter reads GetSceneItemList's raw response so each item's visibility comes with its ID
/// and name in one request (assessment #30); the library's SceneItemDetails drops it.
/// </summary>
public class OBSWebsocketAdapterSceneItemsTests
{
    [Fact]
    public void ParseSceneItems_MapsIdNameAndVisibilityInOBSOrder()
    {
        JObject response = JObject.Parse(@"{
            ""sceneItems"": [
                { ""sceneItemId"": 3, ""sourceName"": ""Camera"", ""sceneItemEnabled"": true, ""inputKind"": ""dshow_input"" },
                { ""sceneItemId"": 7, ""sourceName"": ""Overlay"", ""sceneItemEnabled"": false }
            ]
        }");

        (Int32 ItemId, String SourceName, Boolean Enabled)[] items = OBSWebsocketAdapter.ParseSceneItems(response);

        Assert.Equal(new[] { (3, "Camera", true), (7, "Overlay", false) }, items);
    }

    [Fact]
    public void ParseSceneItems_WhenVisibilityMissing_TreatsTheItemAsHidden()
    {
        JObject response = JObject.Parse(@"{ ""sceneItems"": [ { ""sceneItemId"": 3, ""sourceName"": ""Camera"" } ] }");

        (Int32 ItemId, String SourceName, Boolean Enabled)[] items = OBSWebsocketAdapter.ParseSceneItems(response);

        Assert.False(Assert.Single(items).Enabled);
    }

    [Fact]
    public void ParseSceneItems_WhenTheSceneIsEmpty_ReturnsNoItems()
    {
        JObject response = JObject.Parse(@"{ ""sceneItems"": [] }");

        Assert.Empty(OBSWebsocketAdapter.ParseSceneItems(response));
    }

    [Fact]
    public void ParseSceneItems_WhenTheResponseHasNoItemList_ReturnsNoItems()
    {
        Assert.Empty(OBSWebsocketAdapter.ParseSceneItems(new JObject()));
        Assert.Empty(OBSWebsocketAdapter.ParseSceneItems(null));
    }
}
