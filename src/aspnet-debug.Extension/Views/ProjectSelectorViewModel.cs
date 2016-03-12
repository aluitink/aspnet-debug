using System;
using System.Collections.Generic;

namespace aspnet_debug.Extension.Views
{
    public class ProjectDefinition
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }

    public class ProjectSelectorViewModel
    {
        public List<ProjectDefinition> Projects { get; set; }
    }
}