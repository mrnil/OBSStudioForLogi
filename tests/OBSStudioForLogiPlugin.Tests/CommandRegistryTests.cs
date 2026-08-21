namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

using System.Linq;
using Moq;

public class CommandRegistryTests
{
    private readonly CommandRegistry _registry;

    public CommandRegistryTests()
    {
        this._registry = new CommandRegistry();
    }

    [Fact]
    public void Register_NullCommand_DoesNotThrow()
    {
        var exception = Record.Exception(() => this._registry.Register(null));

        Assert.Null(exception);
    }

    [Fact]
    public void Register_ValidCommand_IsReturnedByGetCommands()
    {
        var mockCommand = new Mock<IObsCommand>();

        this._registry.Register(mockCommand.Object);

        Assert.Single(this._registry.GetCommands<IObsCommand>());
        Assert.Same(mockCommand.Object, this._registry.GetCommands<IObsCommand>().Single());
    }

    [Fact]
    public void Register_DuplicateCommand_DoesNotAddTwice()
    {
        var mockCommand = new Mock<IObsCommand>();

        this._registry.Register(mockCommand.Object);
        this._registry.Register(mockCommand.Object);

        Assert.Single(this._registry.GetCommands<IObsCommand>());
    }

    [Fact]
    public void GetCommands_WithNoCommands_ReturnsEmpty()
    {
        Assert.Empty(this._registry.GetCommands<IObsCommand>());
    }

    [Fact]
    public void GetCommands_FiltersByInterfaceType()
    {
        var sceneAware = new Mock<ISceneAwareCommand>();
        var nonAware = new Mock<IObsCommand>();
        this._registry.Register(sceneAware.Object);
        this._registry.Register(nonAware.Object);

        var result = this._registry.GetCommands<ISceneAwareCommand>().ToList();

        Assert.Single(result);
        Assert.Same(sceneAware.Object, result[0]);
    }

    [Fact]
    public void GetCommands_CommandImplementingMultipleInterfaces_ReturnedForEachInterface()
    {
        var multiAware = new Mock<IMultiAwareCommand>();
        this._registry.Register(multiAware.Object);

        Assert.Single(this._registry.GetCommands<ISceneAwareCommand>());
        Assert.Single(this._registry.GetCommands<IInputMuteAwareCommand>());
        Assert.Single(this._registry.GetCommands<IObsCommand>());
    }

    // Helper interface combining multiple for testing
    public interface IMultiAwareCommand : ISceneAwareCommand, IInputMuteAwareCommand
    {
    }
}
