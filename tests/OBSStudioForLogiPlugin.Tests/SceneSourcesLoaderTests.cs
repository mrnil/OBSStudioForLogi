namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

public class SceneSourcesLoaderTests
{
    private readonly Queue<Action> _background = new Queue<Action>();
    private readonly List<String> _fetchedScenes = new List<String>();
    private readonly List<(String Scene, String[] Sources, String[] AudioSources)> _delivered = new List<(String, String[], String[])>();
    private Boolean _isConnected = true;
    private Func<String, (String[], String[])> _fetch;

    public SceneSourcesLoaderTests()
    {
        this._fetch = scene => (new[] { $"{scene}-source" }, new[] { $"{scene}-audio" });
    }

    // Most tests want a folder open, so the loader starts with one viewer unless told otherwise.
    private SceneSourcesLoader CreateLoader(Boolean inline, Boolean withViewer = true)
    {
        SceneSourcesLoader loader = new SceneSourcesLoader(
            scene =>
            {
                this._fetchedScenes.Add(scene);
                return this._fetch(scene);
            },
            () => this._isConnected,
            inline ? action => action() : action => this._background.Enqueue(action));

        if (withViewer)
        {
            loader.AddViewer("Folder");
        }

        return loader;
    }

    private void Deliver(String scene, String[] sources, String[] audioSources)
    {
        this._delivered.Add((scene, sources, audioSources));
    }

    [Fact]
    public void Load_WhenConnected_DeliversSourcesAndAudioSources()
    {
        SceneSourcesLoader loader = this.CreateLoader(inline: true);

        loader.Load("Scene A", this.Deliver);

        Assert.Single(this._delivered);
        Assert.Equal("Scene A", this._delivered[0].Scene);
        Assert.Equal(new[] { "Scene A-source" }, this._delivered[0].Sources);
        Assert.Equal(new[] { "Scene A-audio" }, this._delivered[0].AudioSources);
    }

    [Fact]
    public void Load_DoesNotFetchOnCallingThread()
    {
        SceneSourcesLoader loader = this.CreateLoader(inline: false);

        loader.Load("Scene A", this.Deliver);

        Assert.Empty(this._fetchedScenes);
        Assert.Empty(this._delivered);
        Assert.Single(this._background);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Load_WhenSceneNameEmpty_DoesNothing(String? sceneName)
    {
        SceneSourcesLoader loader = this.CreateLoader(inline: false);

        loader.Load(sceneName!, this.Deliver);

        Assert.Empty(this._background);
        Assert.Empty(this._delivered);
    }

    [Fact]
    public void Load_WhenSupersededBeforeFinishing_DropsOlderResult()
    {
        SceneSourcesLoader loader = this.CreateLoader(inline: false);
        loader.Load("Scene A", this.Deliver);
        loader.Load("Scene B", this.Deliver);

        // Finish out of order: the newer load completes first.
        Action olderLoad = this._background.Dequeue();
        Action newerLoad = this._background.Dequeue();
        newerLoad();
        olderLoad();

        Assert.Single(this._delivered);
        Assert.Equal("Scene B", this._delivered[0].Scene);
    }

    [Fact]
    public void Load_WhenSupersededByLoadOfSameScene_DeliversOnlyLatest()
    {
        SceneSourcesLoader loader = this.CreateLoader(inline: false);
        loader.Load("Scene A", this.Deliver);
        loader.Load("Scene A", this.Deliver);

        while (this._background.Count > 0)
        {
            this._background.Dequeue()();
        }

        Assert.Single(this._delivered);
    }

    [Fact]
    public void Load_WhenDisconnectedBeforeFinishing_DropsResult()
    {
        SceneSourcesLoader loader = this.CreateLoader(inline: false);
        loader.Load("Scene A", this.Deliver);

        this._isConnected = false;
        this._background.Dequeue()();

        Assert.Empty(this._delivered);
    }

    [Fact]
    public void Load_WhenFetchThrows_DoesNotThrowOrDeliver()
    {
        this._fetch = _ => throw new InvalidOperationException("OBS error");
        SceneSourcesLoader loader = this.CreateLoader(inline: true);

        Exception exception = Record.Exception(() => loader.Load("Scene A", this.Deliver));

        Assert.Null(exception);
        Assert.Empty(this._delivered);
    }

    [Fact]
    public void Load_WhenFetchReturnsNullLists_DeliversEmptyArrays()
    {
        this._fetch = _ => (null!, null!);
        SceneSourcesLoader loader = this.CreateLoader(inline: true);

        loader.Load("Scene A", this.Deliver);

        Assert.Single(this._delivered);
        Assert.Empty(this._delivered[0].Sources);
        Assert.Empty(this._delivered[0].AudioSources);
    }

    [Fact]
    public void Load_WhenCallbackThrows_DoesNotThrow()
    {
        SceneSourcesLoader loader = this.CreateLoader(inline: true);

        Exception exception = Record.Exception(() => loader.Load("Scene A", (s, sources, audio) => throw new InvalidOperationException("boom")));

        Assert.Null(exception);
    }

    // --- Viewers: nothing is fetched unless a scene source folder is open ---

    [Fact]
    public void Load_WhenNoViewers_SkipsFetchAndReturnsFalse()
    {
        SceneSourcesLoader loader = this.CreateLoader(inline: true, withViewer: false);

        Boolean started = loader.Load("Scene A", this.Deliver);

        Assert.False(started);
        Assert.Empty(this._fetchedScenes);
        Assert.Empty(this._delivered);
    }

    [Fact]
    public void Load_WithViewer_ReturnsTrue()
    {
        SceneSourcesLoader loader = this.CreateLoader(inline: false);

        Assert.True(loader.Load("Scene A", this.Deliver));
    }

    [Fact]
    public void Load_WhenSceneNameEmpty_ReturnsFalse()
    {
        SceneSourcesLoader loader = this.CreateLoader(inline: false);

        Assert.False(loader.Load(String.Empty, this.Deliver));
    }

    [Fact]
    public void Load_AfterLastViewerRemoved_SkipsFetch()
    {
        SceneSourcesLoader loader = this.CreateLoader(inline: true);
        loader.RemoveViewer("Folder");

        loader.Load("Scene A", this.Deliver);

        Assert.Empty(this._fetchedScenes);
        Assert.False(loader.HasViewers);
    }

    [Fact]
    public void Load_WhileAnotherViewerRemains_StillFetches()
    {
        SceneSourcesLoader loader = this.CreateLoader(inline: true);
        loader.AddViewer("Other Folder");
        loader.RemoveViewer("Folder");

        loader.Load("Scene A", this.Deliver);

        Assert.Single(this._delivered);
    }

    [Fact]
    public void AddViewer_SameOwnerTwice_OneRemoveIsEnough()
    {
        SceneSourcesLoader loader = this.CreateLoader(inline: true, withViewer: false);
        loader.AddViewer("Folder");
        loader.AddViewer("Folder");

        loader.RemoveViewer("Folder");

        Assert.False(loader.HasViewers);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void AddViewer_WhenOwnerEmpty_IsIgnored(String? owner)
    {
        SceneSourcesLoader loader = this.CreateLoader(inline: true, withViewer: false);

        loader.AddViewer(owner!);

        Assert.False(loader.HasViewers);
    }

    [Fact]
    public void RemoveViewer_OwnerNeverAdded_KeepsOtherViewers()
    {
        SceneSourcesLoader loader = this.CreateLoader(inline: true);

        loader.RemoveViewer("Someone Else");

        Assert.True(loader.HasViewers);
    }

    [Fact]
    public void Constructor_WhenFetchNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new SceneSourcesLoader(null, () => true));
    }
}
