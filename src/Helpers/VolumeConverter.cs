namespace Loupedeck.OBSStudioForLogiPlugin
{
    using System;

    public static class VolumeConverter
    {
        public static Single MulToDb(Single volumeMul)
        {
            if (volumeMul <= 0f)
                return Single.NegativeInfinity;

            return 20f * MathF.Log10(volumeMul);
        }

        public static String FormatDb(Single volumeMul)
        {
            if (volumeMul <= 0f)
                return "-\u221E dB";

            Single db = MulToDb(volumeMul);
            String sign = db > 0f ? "+" : "";
            return $"{sign}{db:F1} dB";
        }
    }
}
