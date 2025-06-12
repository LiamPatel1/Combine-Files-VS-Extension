using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace CombineFilesVSExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("Combine Files", "Allows you to select multiple files, right click and press Combine Files to print the combined contents to the output. You can also use templates to determine how the files should be combined.", "1.0.3")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(Guids.PackageGuidString)]
    [ProvideOptionPage(typeof(OptionsPageGrid), "Combine Files", "General Settings", 0, 0, true, SupportsProfiles = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.FolderOpened_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class CombineFilesPackage : AsyncPackage
    {
        public CombineFilesPackage()
        {
            Debug.WriteLine($"[{nameof(CombineFilesPackage)}] Constructor called.");
        }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            Debug.WriteLine($"[{nameof(CombineFilesPackage)}] InitializeAsync - START.");
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            Debug.WriteLine($"[{nameof(CombineFilesPackage)}] InitializeAsync - Switched to Main thread.");

            Debug.WriteLine($"[{nameof(CombineFilesPackage)}] InitializeAsync - Initializing CombineFilesCommand.");
            await CombineFilesCommand.InitializeAsync(this); 
            Debug.WriteLine($"[{nameof(CombineFilesPackage)}] InitializeAsync - CombineFilesCommand initialization called.");

            Debug.WriteLine($"[{nameof(CombineFilesPackage)}] InitializeAsync - END.");
        }

        public OptionsPageGrid GeneralOptions
        {
            get
            {
                return (OptionsPageGrid)GetDialogPage(typeof(OptionsPageGrid));
            }
        }
    }
}
