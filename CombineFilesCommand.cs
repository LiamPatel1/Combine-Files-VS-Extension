using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CombineFilesVSExtension
{
    internal sealed class CombineFilesCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid(Guids.CommandSetGuidString);

        private readonly AsyncPackage _package;
        private static DTE2 _dte;
        private static IVsMonitorSelection _monitorSelection;
        private static IVsOutputWindowPane _outputPane;
        private const string OutputPaneTitle = "Combine Files Output";

        private CombineFilesCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);

            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            menuItem.BeforeQueryStatus += this.MenuItem_BeforeQueryStatus;
            commandService.AddCommand(menuItem);
            Debug.WriteLine($"[{nameof(CombineFilesCommand)}] Constructor: Command added with ID {menuCommandID}.");
        }

        public static CombineFilesCommand Instance { 
            get; 
            private set;
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            Debug.WriteLine($"[{nameof(CombineFilesCommand)}] InitializeAsync - START.");
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            Debug.WriteLine($"[{nameof(CombineFilesCommand)}] InitializeAsync - Switched to Main thread.");

            _dte = await package.GetServiceAsync(typeof(SDTE)) as DTE2;
            if (_dte == null) Debug.WriteLine($"[{nameof(CombineFilesCommand)}] InitializeAsync - DTE2 service NOT found.");
            else Debug.WriteLine($"[{nameof(CombineFilesCommand)}] InitializeAsync - DTE2 service obtained: {_dte.Name} {_dte.Version}");

            _monitorSelection = await package.GetServiceAsync(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            if (_monitorSelection == null) Debug.WriteLine($"[{nameof(CombineFilesCommand)}] InitializeAsync - IVsMonitorSelection service NOT found.");
            else Debug.WriteLine($"[{nameof(CombineFilesCommand)}] InitializeAsync - IVsMonitorSelection service obtained.");

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                Instance = new CombineFilesCommand(package, commandService);
                Debug.WriteLine($"[{nameof(CombineFilesCommand)}] InitializeAsync - Instance created and command registered.");
            }
            else Debug.WriteLine($"[{nameof(CombineFilesCommand)}] InitializeAsync - OleMenuCommandService NOT found.");
            Debug.WriteLine($"[{nameof(CombineFilesCommand)}] InitializeAsync - END.");
        }

        private string GetPathFromHierarchyAndItemId(IVsHierarchy hierarchy, uint itemId)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (hierarchy == null) return null;
            string filePath = null;
            // Try VSHPROPID_SaveName first
            if (ErrorHandler.Succeeded(hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_SaveName, out object saveNameObj)) &&
                saveNameObj is string saveNameStr && !string.IsNullOrEmpty(saveNameStr) &&
                File.Exists(saveNameStr) && !File.GetAttributes(saveNameStr).HasFlag(FileAttributes.Directory))
            {
                filePath = saveNameStr;
            }
            // Try IVsProject.GetMkDocument (especially for project items)
            else if (hierarchy is IVsProject project &&
                     ErrorHandler.Succeeded(project.GetMkDocument(itemId, out string mkDocument)) &&
                     !string.IsNullOrEmpty(mkDocument) && File.Exists(mkDocument) &&
                     !File.GetAttributes(mkDocument).HasFlag(FileAttributes.Directory))
            {
                filePath = mkDocument;
            }
            // Try DTE ProjectItem FullPath (can be more general)
            else if (ErrorHandler.Succeeded(hierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out object extObject)) &&
                     extObject is ProjectItem dteProjectItem &&
                     dteProjectItem.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFile)
            {
                try { string path = dteProjectItem.Properties.Item("FullPath").Value.ToString(); if (!string.IsNullOrEmpty(path) && File.Exists(path) && !File.GetAttributes(path).HasFlag(FileAttributes.Directory)) filePath = path; } catch { /*ignore*/ }
                if (string.IsNullOrEmpty(filePath)) { try { if (dteProjectItem.FileCount > 0) { string path = dteProjectItem.FileNames[1]; if (!string.IsNullOrEmpty(path) && File.Exists(path) && !File.GetAttributes(path).HasFlag(FileAttributes.Directory)) filePath = path; } } catch { /*ignore*/ } }
            }
            // Try GetCanonicalName (often works for Folder View items)
            else if (ErrorHandler.Succeeded(hierarchy.GetCanonicalName(itemId, out string canonicalName)) &&
                     !string.IsNullOrEmpty(canonicalName))
            {
                if (File.Exists(canonicalName) && !File.GetAttributes(canonicalName).HasFlag(FileAttributes.Directory))
                {
                    filePath = canonicalName;
                }
                // Handle case for Folder View where canonicalName might be relative to the opened folder
                else if (_dte?.Solution != null && string.IsNullOrEmpty(Path.GetExtension(_dte.Solution.FullName)) && Directory.Exists(_dte.Solution.FullName))
                {
                    string potentialPath = Path.Combine(_dte.Solution.FullName, canonicalName.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                    if (File.Exists(potentialPath) && !File.GetAttributes(potentialPath).HasFlag(FileAttributes.Directory))
                    {
                        filePath = potentialPath;
                    }
                }
            }

            if (!string.IsNullOrEmpty(filePath)) Debug.WriteLine($"[{nameof(GetPathFromHierarchyAndItemId)}] Path: '{filePath}' for itemId {itemId}");
            else Debug.WriteLine($"[{nameof(GetPathFromHierarchyAndItemId)}] Could not determine file path for itemId {itemId} via IVsHierarchy.");
            return filePath;
        }

        private string GetPathFromDteSelectedItem(SelectedItem dteSelectedItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (dteSelectedItem == null) return null;
            string itemName = "NameNotRetrieved"; try { itemName = dteSelectedItem.Name; } catch { /* ignore */ }

            // Check ProjectItem first
            if (dteSelectedItem.ProjectItem != null)
            {
                ProjectItem pi = dteSelectedItem.ProjectItem;
                if (pi.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFile)
                {
                    try
                    { // FullPath is usually reliable
                        string path = pi.Properties.Item("FullPath").Value.ToString();
                        if (File.Exists(path) && !File.GetAttributes(path).HasFlag(FileAttributes.Directory)) return path;
                    }
                    catch
                    {
                        try
                        { // Fallback to FileNames[1]
                            if (pi.FileCount > 0)
                            {
                                string path = pi.FileNames[1]; // FileNames is 1-based
                                if (File.Exists(path) && !File.GetAttributes(path).HasFlag(FileAttributes.Directory)) return path;
                            }
                        }
                        catch { /* ignore */ }
                    }
                }
            }
            // If not a ProjectItem or path not found, try dteSelectedItem.Name (for Folder View items not part of a project)
            if (!string.IsNullOrEmpty(itemName) && itemName != "UnnamedItem" && File.Exists(itemName) && !File.GetAttributes(itemName).HasFlag(FileAttributes.Directory))
            {
                return itemName;
            }
            // Additional check for Folder View: if Solution.FullName is a directory and itemName is relative
            if (_dte?.Solution != null && string.IsNullOrEmpty(Path.GetExtension(_dte.Solution.FullName)) && Directory.Exists(_dte.Solution.FullName) &&
                !string.IsNullOrEmpty(itemName) && itemName != "UnnamedItem")
            {
                string potentialPath = Path.Combine(_dte.Solution.FullName, itemName.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                if (File.Exists(potentialPath) && !File.GetAttributes(potentialPath).HasFlag(FileAttributes.Directory))
                {
                    return potentialPath;
                }
            }

            Debug.WriteLine($"[{nameof(GetPathFromDteSelectedItem)}] Could not get valid file path for DTE item: '{itemName}'");
            return null;
        }


        private void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var command = sender as OleMenuCommand;
            if (command == null) return;

            command.Visible = false;
            command.Enabled = false;
            bool anyFileSelected = GetSelectedFilePaths().Any();

            if (anyFileSelected)
            {
                command.Visible = true;
                command.Enabled = true;
                Debug.WriteLine($"[{nameof(MenuItem_BeforeQueryStatus)}] Command Visible & Enabled.");
            }
            else
            {
                Debug.WriteLine($"[{nameof(MenuItem_BeforeQueryStatus)}] No valid files selected. Command Hidden/Disabled.");
            }
        }

 
        private void Execute(object sender, EventArgs e)
        {
            _ = _package.JoinableTaskFactory.RunAsync(async () =>
            {
                await ExecuteCoreAsync(sender, e);
            });
        }

 
        private async System.Threading.Tasks.Task ExecuteCoreAsync(object sender, EventArgs e)
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(_package.DisposalToken);
                Debug.WriteLine($"[{nameof(ExecuteCoreAsync)}] Execute - START.");

                OptionsPageGrid options = null;
                if (_package is CombineFilesPackage packageInstance)
                {
                    options = packageInstance.GeneralOptions;
                }
                if (options == null)
                {
                    Debug.WriteLine($"[{nameof(ExecuteCoreAsync)}] OptionsPageGrid instance was null. Aborting.");
                    VsShellUtilities.ShowMessageBox(_package, "Could not load extension settings.", "Combine Files Error",
                        OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    return;
                }

                List<string> selectedAbsolutePaths = GetSelectedFilePaths(); 
                if (!selectedAbsolutePaths.Any())
                {
                    Debug.WriteLine($"[{nameof(ExecuteCoreAsync)}] No files selected or paths could not be determined.");
                    await WriteToOutputPaneAsync("No files selected to combine."); 
                    return;
                }

                string rootPath = GetSolutionOrFolderPath(); 
                Debug.WriteLine($"[{nameof(ExecuteCoreAsync)}] Root path for relative paths: {rootPath ?? "Not found (will use filenames)"}");

                var filesToProcess = new List<FileProcessData>();
                int originalIndexCounter = 0;
                foreach (var absPath in selectedAbsolutePaths)
                {
                    string fileName = Path.GetFileName(absPath);
                    string currentRelativePath = CalculateRelativePath(absPath, rootPath);

                    bool excluded = (options.ExcludeFiles).Any(pattern => // Directly use options.ExcludeFiles
                    {
                        bool patternContainsSeparators = pattern.IndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }) != -1;

                        if (patternContainsSeparators) // Pattern is path-like
                        {
                            if (Path.IsPathRooted(pattern)) // Pattern is an absolute path wildcard (e.g., "C:/temp/*")
                            {
                                return WildcardMatcher.Matches(pattern, absPath);
                            }
                            else // Pattern is a relative path wildcard (e.g., "obj/*", "src/models/*")
                            {
                                return WildcardMatcher.Matches(pattern, currentRelativePath);
                            }
                        }
                        else // Pattern is a filename wildcard (e.g., "*.txt", "LICENSE")
                        {
                            return WildcardMatcher.Matches(pattern, fileName);
                        }
                    });

                    if (excluded)
                    {
                        Debug.WriteLine($"[{nameof(ExecuteCoreAsync)}] Excluding file: {absPath} (Relative: {currentRelativePath}) due to pattern match.");
                        continue;
                    }

                    string content;
                    try
                    {
                        content = await Task.Run(() => File.ReadAllText(absPath));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[{nameof(ExecuteCoreAsync)}] Error reading file {absPath}: {ex.Message}");
                        content = $"Error reading file '{fileName}': {ex.Message}";
                    }

                    string fileType = "text";
                    foreach (var typeMatch in options.TypeMatching ?? new Dictionary<string, string>())
                    {
                        if (WildcardMatcher.Matches(typeMatch.Key, fileName)) 
                        {
                            fileType = typeMatch.Value;
                            break;
                        }
                    }
                    filesToProcess.Add(new FileProcessData { AbsolutePath = absPath, RelativePath = currentRelativePath, Content = content, Type = fileType, OriginalIndex = originalIndexCounter++ });
                }

                if (!filesToProcess.Any())
                {
                    Debug.WriteLine($"[{nameof(ExecuteCoreAsync)}] No files remaining after exclusion filter.");
                    await WriteToOutputPaneAsync("No files to combine after applying exclusion filters.");
                    return;
                }

                List<FileProcessData> sortedFiles = SortFiles(filesToProcess, options.PriorityFiles);

                var outputBuilder = new StringBuilder();
                if (!string.IsNullOrEmpty(options.OutputHeader)) outputBuilder.AppendLine(options.OutputHeader);

                string template = options.OutputTemplate;
                const string macroPattern = @"\{\{(?<name>\w+)\}\}";

                foreach (var fileData in sortedFiles)
                {
                    var macroValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "{{absolute_filepath}}", fileData.AbsolutePath },
                        { "{{relative_filepath}}", fileData.RelativePath },
                        { "{{filename}}", Path.GetFileName(fileData.AbsolutePath) },
                        { "{{type}}", fileData.Type },
                        { "{{text}}", fileData.Content }
                    };

                    string templatedOutput = Regex.Replace(template, macroPattern, match =>
                    {
                        string fullMacroTag = match.Value;
                        if (macroValues.TryGetValue(fullMacroTag, out string replacementValue))
                        {
                            return replacementValue;
                        }
                        return fullMacroTag; // Return original macro if not found
                    });

                    outputBuilder.AppendLine(templatedOutput);
                }

                if (!string.IsNullOrEmpty(options.OutputFooter)) outputBuilder.AppendLine(options.OutputFooter);

                await WriteToOutputPaneAsync(outputBuilder.ToString());
                Debug.WriteLine($"[{nameof(ExecuteCoreAsync)}] Execute - END. Files combined: {sortedFiles.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{nameof(ExecuteCoreAsync)}] Unhandled exception in ExecuteCoreAsync: {ex}");

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(_package.DisposalToken);
                VsShellUtilities.ShowMessageBox(
                    _package,
                    $"An unexpected error occurred while combining files: {ex.Message}",
                    "Combine Files Error",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }


        private List<string> GetSelectedFilePaths()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var orderedUniquePaths = new List<string>();

            // Helper to add paths uniquely while preserving order
            Action<string> addPathIfUnique = (path) => {
                if (!string.IsNullOrEmpty(path) && !orderedUniquePaths.Contains(path, StringComparer.OrdinalIgnoreCase))
                {
                    orderedUniquePaths.Add(path);
                    Debug.WriteLine($"[{nameof(GetSelectedFilePaths)}] Added path: {path}");
                }
            };

            if (_monitorSelection != null)
            {
                IntPtr hierarchyPtr = IntPtr.Zero;
                uint itemId = VSConstants.VSITEMID_NIL;
                IVsMultiItemSelect multiItemSelect = null;
                IntPtr selectionContainerPtr = IntPtr.Zero;
                try
                {
                    int hr = _monitorSelection.GetCurrentSelection(out hierarchyPtr, out itemId, out multiItemSelect, out selectionContainerPtr);
                    if (ErrorHandler.Succeeded(hr))
                    {
                        if (multiItemSelect != null && ErrorHandler.Succeeded(multiItemSelect.GetSelectionInfo(out uint itemCount, out int pfSingleHierarchy)) && itemCount > 0)
                        {
                            var vsItemSelections = new VSITEMSELECTION[itemCount];
                            if (ErrorHandler.Succeeded(multiItemSelect.GetSelectedItems(0, itemCount, vsItemSelections)))
                            {
                                foreach (var vsItemSel in vsItemSelections) // Order here tends to be selection order
                                {
                                    if (vsItemSel.pHier != null)
                                    {
                                        string path = GetPathFromHierarchyAndItemId(vsItemSel.pHier, vsItemSel.itemid);
                                        addPathIfUnique(path);
                                    }
                                }
                            }
                        }
                        else if (hierarchyPtr != IntPtr.Zero) // Single item
                        {
                            IVsHierarchy selectedHierarchy = Marshal.GetObjectForIUnknown(hierarchyPtr) as IVsHierarchy;
                            if (selectedHierarchy != null)
                            {
                                string path = GetPathFromHierarchyAndItemId(selectedHierarchy, itemId);
                                addPathIfUnique(path);
                            }
                        }
                    }
                }
                catch (Exception ex) { Debug.WriteLine($"[{nameof(GetSelectedFilePaths)}] EXCEPTION during IVsMonitorSelection: {ex.Message}"); }
                finally
                {
                    if (hierarchyPtr != IntPtr.Zero) Marshal.Release(hierarchyPtr);
                    if (selectionContainerPtr != IntPtr.Zero) Marshal.Release(selectionContainerPtr);
                }
            }

     
            if (_dte?.SelectedItems != null && _dte.SelectedItems.Count > 0)
            {
                if (!orderedUniquePaths.Any() || _dte.SelectedItems.Count > orderedUniquePaths.Count)
                {
                    Debug.WriteLine($"[{nameof(GetSelectedFilePaths)}] Considering DTE.SelectedItems. IVsMS count: {orderedUniquePaths.Count}, DTE count: {_dte.SelectedItems.Count}");
                    var dtePathsTemp = new List<string>();
                    foreach (SelectedItem dteItem in _dte.SelectedItems) // Order here tends to be selection order
                    {
                        string path = GetPathFromDteSelectedItem(dteItem);
                        if (!string.IsNullOrEmpty(path) && !dtePathsTemp.Contains(path, StringComparer.OrdinalIgnoreCase))
                        {
                            dtePathsTemp.Add(path);
                        }
                    }

                    if (!orderedUniquePaths.Any() || dtePathsTemp.Count > orderedUniquePaths.Count)
                    {

                        orderedUniquePaths.Clear();
                        orderedUniquePaths.AddRange(dtePathsTemp);
                        Debug.WriteLine($"[{nameof(GetSelectedFilePaths)}] Used DTE.SelectedItems primarily. New count: {orderedUniquePaths.Count}");
                    }
                    else

                    {
                        foreach (var dtePath in dtePathsTemp)
                        {
                            addPathIfUnique(dtePath); 
                        }
                        Debug.WriteLine($"[{nameof(GetSelectedFilePaths)}] Merged DTE.SelectedItems. New count: {orderedUniquePaths.Count}");
                    }
                }
            }
            Debug.WriteLine($"[{nameof(GetSelectedFilePaths)}] Total unique file paths found (ordered): {orderedUniquePaths.Count}");
            return orderedUniquePaths;
        }

        private string GetSolutionOrFolderPath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (_dte == null || _dte.Solution == null) return null;

            string solutionFullName = _dte.Solution.FullName;
            if (string.IsNullOrEmpty(solutionFullName)) return null;

            // If Solution.FullName points to a .sln file, root is its directory
            if (File.Exists(solutionFullName) && (solutionFullName.EndsWith(".sln", StringComparison.OrdinalIgnoreCase)))
            {
                return Path.GetDirectoryName(solutionFullName);
            }
            // If Solution.FullName points to a directory (Open Folder scenario)
            if (Directory.Exists(solutionFullName) && string.IsNullOrEmpty(Path.GetExtension(solutionFullName)))
            {
                return solutionFullName;
            }
            return null; // Fallback
        }

        private string CalculateRelativePath(string absolutePath, string rootPath)
        {
            if (string.IsNullOrEmpty(rootPath)) return Path.GetFileName(absolutePath);
            try
            {
                // Ensure rootPath ends with a directory separator for Uri processing
                string normalizedRoot = rootPath;
                if (!normalizedRoot.EndsWith(Path.DirectorySeparatorChar.ToString()) && !normalizedRoot.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                {
                    normalizedRoot += Path.DirectorySeparatorChar;
                }
                Uri rootUri = new Uri(normalizedRoot);
                Uri fileUri = new Uri(absolutePath);

                if (!rootUri.IsBaseOf(fileUri)) return Path.GetFileName(absolutePath); // Not under root

                Uri relativeUri = rootUri.MakeRelativeUri(fileUri);
                return Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{nameof(CalculateRelativePath)}] Error for '{absolutePath}' relative to '{rootPath}': {ex.Message}. Falling back to filename.");
                return Path.GetFileName(absolutePath);
            }
        }

        private List<FileProcessData> SortFiles(List<FileProcessData> files, List<String> priorityPatterns)
        {
            var priorityFiles = new List<FileProcessData>();
            var otherFiles = new List<FileProcessData>();

            foreach (var file in files)
            {
                bool isPriority = false;
                foreach (var pattern in priorityPatterns)
                {
                    if (WildcardMatcher.Matches(pattern, Path.GetFileName(file.AbsolutePath)) || WildcardMatcher.Matches(pattern, file.RelativePath))
                    {
                        isPriority = true;
                        break;
                    }
                }
                if (isPriority) priorityFiles.Add(file); else otherFiles.Add(file);
            }

            priorityFiles.Sort((a, b) =>
            {
                int GetPriorityIndex(FileProcessData fpd)
                {
                    for (int i = 0; i < priorityPatterns.Count; i++)
                    {
                        if (WildcardMatcher.Matches(priorityPatterns[i], Path.GetFileName(fpd.AbsolutePath)) || WildcardMatcher.Matches(priorityPatterns[i], fpd.RelativePath)) return i;
                    }
                    return int.MaxValue;
                }
                return GetPriorityIndex(a).CompareTo(GetPriorityIndex(b));
            });

            otherFiles.Sort((a, b) => a.OriginalIndex.CompareTo(b.OriginalIndex)); // Maintain original order for non-priority
            priorityFiles.AddRange(otherFiles);
            return priorityFiles;
        }

        private async System.Threading.Tasks.Task<IVsOutputWindowPane> GetOrCreateOutputPaneAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(_package.DisposalToken);
            if (_outputPane == null)
            {
                IVsOutputWindow outputWindow = await _package.GetServiceAsync(typeof(SVsOutputWindow)) as IVsOutputWindow;
                if (outputWindow == null)
                {
                    Debug.WriteLine($"[{nameof(CombineFilesCommand)}] GetOrCreateOutputPaneAsync - SVsOutputWindow service NOT found.");
                    return null;
                }
                Guid paneGuid = Guids.CustomOutputPaneGuid;
                // Try to get existing pane
                if (ErrorHandler.Failed(outputWindow.GetPane(ref paneGuid, out _outputPane)) || _outputPane == null)
                {
                    // Create the pane if it doesn't exist or GetPane failed
                    if (ErrorHandler.Failed(outputWindow.CreatePane(ref paneGuid, OutputPaneTitle, Convert.ToInt32(true), Convert.ToInt32(false))))
                    {
                        Debug.WriteLine($"[{nameof(CombineFilesCommand)}] GetOrCreateOutputPaneAsync - Failed to create output pane.");
                        return null;
                    }
                    if (ErrorHandler.Failed(outputWindow.GetPane(ref paneGuid, out _outputPane)))
                    {
                        Debug.WriteLine($"[{nameof(CombineFilesCommand)}] GetOrCreateOutputPaneAsync - Failed to get newly created output pane.");
                        return null;
                    }
                    Debug.WriteLine($"[{nameof(CombineFilesCommand)}] GetOrCreateOutputPaneAsync - Created new output pane: {OutputPaneTitle}");
                }
                else
                {
                    Debug.WriteLine($"[{nameof(CombineFilesCommand)}] GetOrCreateOutputPaneAsync - Found existing output pane: {OutputPaneTitle}");
                }
            }
            return _outputPane;
        }

        private async System.Threading.Tasks.Task WriteToOutputPaneAsync(string message)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(_package.DisposalToken);
            IVsOutputWindowPane pane = await GetOrCreateOutputPaneAsync();
            if (pane != null)
            {
                pane.Activate();
                pane.Clear(); // Clear previous content for each new combine operation
                pane.OutputString(message + Environment.NewLine); // Ensure message ends with a newline for proper output
            }
            else
            {
                Debug.WriteLine($"[{nameof(ExecuteCoreAsync)}] Could not get output pane. Message: {message.Substring(0, Math.Min(message.Length, 200))}...");
                VsShellUtilities.ShowMessageBox(
                    _package, "Output pane is unavailable. The combined content could not be displayed in the output window.", "Combine Files - Output Error",
                    OLEMSGICON.OLEMSGICON_WARNING, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        private class FileProcessData
        {
            public string AbsolutePath { get; set; }
            public string RelativePath { get; set; }
            public string Content { get; set; }
            public string Type { get; set; }
            public int OriginalIndex { get; set; }
        }
    }
}