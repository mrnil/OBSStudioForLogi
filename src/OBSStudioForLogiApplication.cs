namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public class OBSStudioForLogiApplication : ClientApplication
    {
        public OBSStudioForLogiApplication()
        {
        }

        protected override String GetProcessName() => "obs64.exe";

        protected override String GetBundleName() => "com.obsproject.obs-studio";

        public override ClientApplicationStatus GetApplicationStatus()
        {
            return ClientApplicationStatus.Unknown;
        }
    }
}
