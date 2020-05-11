﻿using CoreBackup.Models.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using CoreBackup.Models.IO;
using DynamicData.Binding;
using System.Linq;
using System.IO;

namespace CoreBackup.Models.Tasks
{
    class SyncActions
    {
        private ObservableCollectionExtended<FileInformation> leftFiles;
        private ObservableCollectionExtended<FileInformation> rightFiles;

        private Dictionary<string, List<string>> leftConfigPaths;
        private Dictionary<string, List<string>> rightConfigPaths;

        public SyncActions(ObservableCollectionExtended<FileInformation> leftFiles, ObservableCollectionExtended<FileInformation> rightFiles)
        {
            this.leftFiles = leftFiles;
            this.rightFiles = rightFiles;
            leftConfigPaths = new Dictionary<string, List<string>>();
            rightConfigPaths = new Dictionary<string, List<string>>();
            GetConfigPaths();
        }

        private void GetConfigPaths()
        {
            foreach (KeyValuePair<string, ConfigHub> entry in CoreTask.tasksList)
            {
                foreach (Configuration leftConf in entry.Value.LeftSources)
                {
                    var leftConfigList = leftConf.GetConfigPaths();
                    leftConfigPaths.Add(entry.Key, leftConfigList);
                }
                foreach (Configuration rightConf in entry.Value.RightSources)
                {
                    var rightConfigList = rightConf.GetConfigPaths();
                    rightConfigPaths.Add(entry.Key, rightConfigList);
                }
            }
        }

        public void SyncMirror()
        {
            var targetPathRight = rightFiles.FirstOrDefault().LocalPath;
            var targetPathLeft = leftFiles.FirstOrDefault().LocalPath;
            foreach (var item in leftFiles)
            {
                if (item.FileVersion == FileVersion.Newer)
                {
                    string fileName = "test.txt";
                    string sourcePath = @"C:\Users\Public\TestFolder";
                    string targetPath = @"C:\Users\Public\TestFolder\SubDir";

                    //string sourceFile = Path.Combine(sourcePath, fileName);
                    string destFile = Path.Combine(targetPathRight, item.RelativePath);
                    string sourceFile = item.FullPath;

                    File.Copy(sourceFile, destFile, true);
                }
            }
        }

        public void SyncToLeft()
        {
            var leftFile = leftFiles.FirstOrDefault();
            string targetPathLeft;
            if (leftFile == null)
            {
                var configName = rightFiles.FirstOrDefault().ConfigurationName;
                targetPathLeft = leftConfigPaths[configName].FirstOrDefault();
            }
            else
            {
                targetPathLeft = leftFile.LocalPath;
            }
            foreach (var item in leftFiles)
            {
                if (item.FileVersion == FileVersion.Newer)
                {
                    string destFile = Path.Combine(targetPathLeft, item.RelativePath);
                    string sourceFile = item.FullPath;
                    var path = destFile.Substring(0, destFile.LastIndexOf(Path.DirectorySeparatorChar));
                    /*
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    File.Copy(sourceFile, destFile, true);*/
                }
            }
        }

        public void SyncToLeftOverride()
        {

        }
        public void SyncToRight()
        {
            var rightFile = rightFiles.FirstOrDefault();
            string targetPathRight;
            if(rightFile == null)
            {
                var configName = leftFiles.FirstOrDefault().ConfigurationName;
                targetPathRight = rightConfigPaths[configName].FirstOrDefault();
            }
            else
            {
                targetPathRight = rightFile.LocalPath;
            }
            foreach (var item in leftFiles)
            {
                if (item.FileVersion == FileVersion.Newer)
                {
                    string destFile = Path.Combine(targetPathRight, item.RelativePath);
                    string sourceFile = item.FullPath;
                    var path = destFile.Substring(0, destFile.LastIndexOf(Path.DirectorySeparatorChar));
                    /*
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    File.Copy(sourceFile, destFile, true);*/
                }
            }
        }
        public void SyncToRightOverride()
        {

        }
    }

}
