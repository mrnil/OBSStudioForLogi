namespace Loupedeck.OBSStudioForLogiPlugin.Tests;

// AudioSelectionState is static, so test classes that change the selection must not run in
// parallel with each other.
[CollectionDefinition(Name)]
public class AudioSelectionStateCollection
{
    public const String Name = "AudioSelectionState";
}
