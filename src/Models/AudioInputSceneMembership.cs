namespace Loupedeck.OBSStudioForLogiPlugin.Models
{
    using System;
    using System.Collections.Generic;

    // Which audio inputs exist and which of them aren't placed in any scene (global devices such
    // as Desktop Audio). Finding the second list takes one scene item request per scene, and it
    // only changes when inputs, scenes or scene items are added, removed or renamed, so
    // OBSWebSocketManager caches it and clears it on those events.
    public class AudioInputSceneMembership
    {
        public AudioInputSceneMembership(String[] audioInputs, String[] notInAnyScene)
        {
            this.AudioInputs = new HashSet<String>(audioInputs ?? new String[0]);
            this.NotInAnyScene = notInAnyScene ?? new String[0];
        }

        public HashSet<String> AudioInputs { get; }

        public String[] NotInAnyScene { get; }
    }
}
