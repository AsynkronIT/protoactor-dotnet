// -----------------------------------------------------------------------
// <copyright file="Generator.cs" company="Asynkron AB">
//      Copyright (C) 2015-2020 Asynkron AB All rights reserved
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Google.Protobuf.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using ProtoBuf;

namespace Proto.GrainGenerator
{
    public static class Generator
    {
        internal static void Generate(FileInfo input, FileInfo output, IEnumerable<DirectoryInfo> importPath, TaskLoggingHelper log, string rootPath, string? templatePath=null)
        {
            var set = GetSet(importPath);
            
            var inputReader = input.OpenText();
            var defaultOutputName = output?.FullName ?? Path.GetFileNameWithoutExtension(input.Name);
            var relativePath = Path.GetRelativePath(rootPath, defaultOutputName);
            
            set.Add(relativePath, true, inputReader);
            set.Process();
            
            
            var template = Template.DefaultTemplate;

            if (!string.IsNullOrEmpty(templatePath))
            {
                var relativeTemplatePath = Path.GetRelativePath(rootPath, templatePath);
                
                log.LogMessage(MessageImportance.High, $"Using custom template {relativeTemplatePath}");
                template = File.ReadAllText(relativeTemplatePath, Encoding.Default);
            }
            
            var gen = new CodeGenerator(template);
            var codeFiles = gen.Generate(set).ToList();

            foreach (var codeFile in codeFiles)
            {
                log.LogMessage(MessageImportance.High, $"Saving generated file {codeFile.Name}");
                File.WriteAllText(codeFile.Name, codeFile.Text);
            }
        }

        private static FileDescriptorSet GetSet(IEnumerable<DirectoryInfo> importPaths)
        {
            var set = new FileDescriptorSet();

            foreach (var path in importPaths)
            {
                set.AddImportPath(path.FullName);
            }

            return set;
        }
    }
}