﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CoreBackup.Models.Config
{
    static class CoreTask
    {
        //Dictionary<taskName, configuration>
        private static Dictionary<string, ConfigHub> tasksList = new Dictionary<string, ConfigHub>();

        public static void RemoveTaskEntry(string taskName)
        {
            tasksList.Remove(taskName);
        }


        public static void AddTaskEntry(string taskName, ConfigHub configuration)
        {
            tasksList.Add(taskName, configuration);
        }
        
    }
}