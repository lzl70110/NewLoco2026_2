using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

// Project Tree Exporter (.exe generator)
// Short EN comments only

string root = Directory.GetCurrentDirectory();

string outFull = Path.Combine(root, "project_tree.txt");
string outCompact = Path.Combine(root, "project_tree_compact.txt");

// Excluded directories
string[] excludeDirs =
{
    "bin", "obj", ".git", ".vs", "node_modules", "wwwroot\\lib", "wwwroot\\dist"
};

// Allowed file extensions
string[] allowedExt = { ".sln", ".cs", ".cshtml", ".json", ".resx" };

bool ShouldExclude(string path)
    {
    foreach (var d in excludeDirs)
        {
        if (path.Contains(Path.DirectorySeparatorChar + d + Path.DirectorySeparatorChar) ||
            path.EndsWith(Path.DirectorySeparatorChar + d))
            return true;
        }
    return false;
    }

// Collect matching files and folders
var all = Directory.GetFileSystemEntries(root, "*", SearchOption.AllDirectories)
    .Where(p =>
        !ShouldExclude(p) &&
        (Directory.Exists(p) || allowedExt.Contains(Path.GetExtension(p)))
    )
    .OrderBy(p => p)
    .ToList();

// Build tree-like output
List<string> lines = new();
foreach (var item in all)
    {
    string rel = item.Replace(root + Path.DirectorySeparatorChar, "");
    int depth = rel.Split(Path.DirectorySeparatorChar).Length - 1;
    string prefix = new string(' ', depth * 2) + "|-- ";
    lines.Add(prefix + Path.GetFileName(item));
    }

// Write full output
File.WriteAllLines(outFull, lines, Encoding.UTF8);

// Compact filtering
string[] keepPatterns =
{
    ".sln",
    "Program.cs",
    "Controllers",
    "Services",
    "Views\\Shared\\_Layout.cshtml",
    "Views\\Shared\\Components",
    "Views\\Locomotives",
    "LocoDbContext.cs",
    "Locomotive.cs",
    "Resources"
};

var compact = lines.Where(l =>
    keepPatterns.Any(p => l.Contains(p, StringComparison.OrdinalIgnoreCase))
).ToList();

// Write compact
File.WriteAllLines(outCompact, compact, Encoding.UTF8);

Console.WriteLine("Done.");
Console.WriteLine("Full: project_tree.txt");
Console.WriteLine("Compact: project_tree_compact.txt");