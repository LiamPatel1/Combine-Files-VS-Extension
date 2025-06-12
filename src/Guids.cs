using System;

namespace CombineFilesVSExtension
{
    internal static class Guids
    {
        public const string PackageGuidString = "3E55CBED-F57C-465A-BC58-0B0AF6427AC4";
        public const string CommandSetGuidString = "11772DCF-9E2C-4621-9211-5149A3AD700E";
        public const string CustomOutputPaneGuidString = "B97961E2-C3FC-4D9F-BC64-669A03161BEF"; 

        public static readonly Guid PackageGuid = new Guid(PackageGuidString);
        public static readonly Guid CommandSetGuid = new Guid(CommandSetGuidString);
        public static readonly Guid CustomOutputPaneGuid = new Guid(CustomOutputPaneGuidString);
    }
}