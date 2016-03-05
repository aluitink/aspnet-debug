using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace aspnet_debug.Extension
{
    public class Solution
    {
        public bool IsDirty { get { return _dteSolution.IsDirty; } }
        public bool IsOpen { get { return _dteSolution.IsOpen; } }
        public bool IsSaved { get { return _dteSolution.Saved; } }

        public string FileName { get { return _dteSolution.FileName; } }
        public string FullName { get { return _dteSolution.FullName; } }

        public IEnumerable<Project> Projects
        {
            get
            {
                foreach (EnvDTE.Project project in _dteSolution.Projects)
                {
                    yield return new Project(project);
                }
            }
        }

        private readonly EnvDTE.Solution _dteSolution;
        public Solution(EnvDTE.Solution dteSolution)
        {
            _dteSolution = dteSolution;
        }
    }

    public class Project
    {
        public bool IsDirty { get { return _dteProject.IsDirty; } }
        public bool IsSaved { get { return _dteProject.Saved; } }

        public string FileName { get { return _dteProject.FileName; } }
        public string FullName { get { return _dteProject.FullName; } }

        public IEnumerable<ProjectItem> Items
        {
            get
            {
                foreach (EnvDTE.ProjectItem projectItem in _dteProject.ProjectItems)
                {
                    yield return new ProjectItem(projectItem);
                }
            }
        }

        private readonly EnvDTE.Project _dteProject;

        public Project(EnvDTE.Project dteProject)
        {
            _dteProject = dteProject;
        }
    }

    public class ProjectItem
    {
        public bool IsDirty { get { return _dteProjectItem.IsDirty; } }
        public bool IsOpen { get { return _dteProjectItem.IsOpen; } }
        public bool IsSaved { get { return _dteProjectItem.Saved; } }

        public string FileName { get { return _dteProjectItem.FileNames[0]; } }

        public string Name { get { return _dteProjectItem.Name; } }

        private readonly EnvDTE.ProjectItem _dteProjectItem;
        public ProjectItem(EnvDTE.ProjectItem dteProjectItem)
        {
            _dteProjectItem = dteProjectItem;
        }
    }

    public class SolutionProvider
    {
        private readonly DTE _dte;

        public SolutionProvider(DTE dte)
        {
            _dte = dte;
        }

        public Solution GetOpenSolution()
        {
            var dteSolution = _dte.Solution;
            return new Solution(dteSolution);
        }
    }
}
