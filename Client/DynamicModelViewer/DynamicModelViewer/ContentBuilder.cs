#region File Description
//-----------------------------------------------------------------------------
// ContentBuilder.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Build.BuildEngine;
#endregion

namespace DynamicModelViewer
{
    /// <summary>
    /// This class wraps the MSBuild functionality needed to build XNA Framework
    /// content dynamically at runtime. It creates a temporary MSBuild project
    /// in memory, and adds whatever content files you choose to this project.
    /// It then builds the project, which will create compiled .xnb content files
    /// in a temporary directory. After the build finishes, you can use a regular
    /// ContentManager to load these temporary .xnb files in the usual way.
    /// </summary>
    class ContentBuilder : IDisposable
    {
        #region Fields


        // What importers or processors should we load?
        const string xnaVersion = ", Version=3.1.0.0, PublicKeyToken=6d5c3888ef60e27d";

        static string[] pipelineAssemblies =
        {
            "Microsoft.Xna.Framework.Content.Pipeline.FBXImporter" + xnaVersion,
            "Microsoft.Xna.Framework.Content.Pipeline.XImporter" + xnaVersion,
            "Microsoft.Xna.Framework.Content.Pipeline.TextureImporter" + xnaVersion,
            "Microsoft.Xna.Framework.Content.Pipeline.EffectImporter" + xnaVersion,
        };


        // MSBuild objects used to dynamically build content.
        Engine msBuildEngine;
        Project msBuildProject;
        ErrorLogger errorLogger;


        // Temporary directories used by the content build.
        string buildDirectory;
        string baseDirectory;

        // Have we been disposed?
        bool isDisposed;


        #endregion

        #region Properties


        /// <summary>
        /// Gets the output directory, which will contain the generated .xnb files.
        /// </summary>
        public string OutputDirectory
        {
            get { return Path.Combine(buildDirectory, "bin/Content"); }
        }

        public string OutputExtention
        {
            get { return "tmp/bin/content"; }
        }


        #endregion

        #region Initialization


        /// <summary>
        /// Creates a new content builder.
        /// </summary>
        public ContentBuilder()
        {
            CreateBuildDirectory();
            CreateBuildProject();
        }


        /// <summary>
        /// Finalizes the content builder.
        /// </summary>
        ~ContentBuilder()
        {
            Dispose(false);
        }


        /// <summary>
        /// Disposes the content builder when it is no longer required.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Implements the standard .NET IDisposable pattern.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                isDisposed = true;
                Directory.Delete(buildDirectory, true);
            }
        }


        #endregion

        #region MSBuild


        /// <summary>
        /// Creates a temporary MSBuild content project in memory.
        /// </summary>
        void CreateBuildProject()
        {
            string projectPath = Path.Combine(buildDirectory, "content.contentproj");
            string outputPath = Path.Combine(buildDirectory, "bin");

            // Create the build engine.
            msBuildEngine = new Engine();

            // Hook up our custom error logger.
            errorLogger = new ErrorLogger();

            msBuildEngine.RegisterLogger(errorLogger);
            msBuildEngine.DefaultToolsVersion = "3.5";
            // Create the build project.
            msBuildProject = new Project(msBuildEngine);

            msBuildProject.FullFileName = projectPath; 

            msBuildProject.SetProperty("XnaPlatform", "Windows");
            msBuildProject.SetProperty("XnaFrameworkVersion", "v3.1");
            msBuildProject.SetProperty("Configuration", "Release");
            msBuildProject.SetProperty("OutputPath", outputPath);

            // Register any custom importers or processors.
            foreach (string pipelineAssembly in pipelineAssemblies)
            {
                msBuildProject.AddNewItem("Reference", pipelineAssembly);
            }
 	            // Reference SkinnedModelPipeline.  
 	        string applicationDirectory = AppDomain.CurrentDomain.BaseDirectory;  
 	        string dllPath = Path.Combine(applicationDirectory, "SkinnedModelPipeline.dll");  
 	        string importDll = Path.GetFullPath(dllPath);
            this.msBuildProject.AddNewItem("Reference", importDll);  

            // Include the standard targets file that defines
            // how to build XNA Framework content.
            msBuildProject.AddNewImport("$(MSBuildExtensionsPath)\\Microsoft\\XNA " +
                                        "Game Studio\\v3.1\\Microsoft.Xna.GameStudio" +
                                        ".ContentPipeline.targets", null);
        }


        /// <summary>
        /// Adds a new content file to the MSBuild project. The importer and
        /// processor are optional: if you leave the importer null, it will
        /// be autodetected based on the file extension, and if you leave the
        /// processor null, data will be passed through without any processing.
        /// </summary>
        public void Add(string filename, string name, string importer, string processor)
        {
            BuildItem buildItem = msBuildProject.AddNewItem("Compile", filename);

            buildItem.SetMetadata("Link", Path.GetFileName(filename));
            buildItem.SetMetadata("Name", name);

            if (!string.IsNullOrEmpty(importer))
                buildItem.SetMetadata("Importer", importer);

            if (!string.IsNullOrEmpty(processor))
                buildItem.SetMetadata("Processor", processor);
        }


        /// <summary>
        /// Removes all content files from the MSBuild project.
        /// </summary>
        public void Clear()
        {
            msBuildProject.RemoveItemsByName("Compile");
        }


        /// <summary>
        /// Builds all the content files which have been added to the project,
        /// dynamically creating .xnb files in the OutputDirectory.
        /// Returns an error message if the build fails.
        /// </summary>
        public string Build()
        {
            // Clear any previous errors.
            errorLogger.Errors.Clear();

            // Build the project.
            if (!msBuildProject.Build())
            {
                // If the build failed, return an error string.
                return string.Join("\n", errorLogger.Errors.ToArray());
            }

            return null;
        }


        #endregion

        #region Temp Directories


        /// <summary>
        /// Creates a temporary directory in which to build content.
        /// </summary>
        /// 
        void CreateBuildDirectory()
        {
            baseDirectory = System.IO.Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, "../Content/");
            buildDirectory = System.IO.Path.Combine(baseDirectory, "tmp");
            Directory.CreateDirectory(buildDirectory);

        }
        #endregion
    }
}
