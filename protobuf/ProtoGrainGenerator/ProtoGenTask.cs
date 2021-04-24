using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Proto.GrainGenerator;


namespace MSBuildTasks
{
    public class ProtoGenTask : Task
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        [Required]
        public string IntermediateOutputPath { get; set; } = null!;

        [Required]
        public string MSBuildProjectFullPath { get; set; } = null!;

        public ITaskItem[] ProtoFile { get; set; } = Array.Empty<ITaskItem>();

        public override bool Execute()
        {
            var projectFile = MSBuildProjectFullPath;
            Log.LogMessage(MessageImportance.High, $"Processing Project file: {projectFile}");
            Log.LogMessage(MessageImportance.High, $"Intermediate OutputPath: {IntermediateOutputPath}");
            var projectDirectory = Path.GetDirectoryName(projectFile)!;
            
            var potatoDirectory = Path.Combine(IntermediateOutputPath!, "protopotato");
            Directory.CreateDirectory(potatoDirectory);
            (new DirectoryInfo(potatoDirectory)).Delete(true);
            
            if (ProtoFile.Any())
            {
                foreach (var item in ProtoFile)
                {
                    var templateFiles = item.GetMetadata("TemplateFiles");
                    var additionalImportDirs = item.GetMetadata("AdditionalImportDirs");
                    var protoFile = item.ItemSpec;
                    Log.LogMessage(MessageImportance.High,
                        $"ProtoFile Item File:{item.ItemSpec}, Imports:{additionalImportDirs}, Templates:{templateFiles}"
                    );
                    ProcessFile(projectDirectory, potatoDirectory,protoFile, additionalImportDirs, templateFiles);
                }
            }
            else
            {
                Log.LogMessage(MessageImportance.High, "No files marked as 'ProtoFile' in project....");
            }
            
            Log.LogMessage(MessageImportance.High, "ProtoGen completed successfully");

            return true;
        }

        private void ProcessFile(string projectDirectory, string objDirectory, string protoFile, string additionalImportDirs, string templateFiles)
        {
            Log.LogMessage(MessageImportance.High, $"Processing Proto file: {protoFile}");
            var protoSourceFile = Path.GetRelativePath(projectDirectory, protoFile);
            var inputFileInfo = new FileInfo(protoFile);
            var importPaths = 
                additionalImportDirs
                    .Split(";", StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .Select(p => Path.GetRelativePath(projectDirectory, p))
                    .Select(p => new DirectoryInfo(p)).ToArray();

            foreach (var importPath in importPaths)
            {
                Log.LogMessage(MessageImportance.High, $"Import path {importPath.FullName}");
            }

            if (string.IsNullOrEmpty(templateFiles))
            {
                var outputFile = Path.Combine(objDirectory, $"{protoSourceFile}.cs");
                Log.LogMessage(MessageImportance.High, $"Output file path: {outputFile}");
                var outputFileInfo = new FileInfo(outputFile);
                
                Generator.Generate(inputFileInfo, outputFileInfo, importPaths, Log, projectDirectory);    
            }
            else
            {
                
            }
        }
    }
}