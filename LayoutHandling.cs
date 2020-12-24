using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace M20R_Checklist_Deployer
{
	static class LayoutHandling
	{
		class SimDataFileInfo
		{
			public string path { get; set; }
			public long size { get; set; }
			public long date { get; set; }
		}

		class LayoutRootData
		{
			public List<SimDataFileInfo> content { get; set; }
		}

		public static bool UpdateLayoutForFile(string packageRoot, string filePath)
		{
			var layoutJsonPath = Path.Combine(packageRoot, "layout.json");
			if (!File.Exists(layoutJsonPath))
				return false;

			var fileInfo = new FileInfo(filePath);
			var packageRelativePath = GetPackageRelativePath(packageRoot, filePath);

			try
			{
				var root = JsonSerializer.Deserialize<LayoutRootData>(File.ReadAllText(layoutJsonPath));

				if (!fileInfo.Exists)
				{
					// Delete files that were removed
					root.content.RemoveAll(x => x.path == packageRelativePath);
				}
				else
				{
					// Add or update files that still exist
					var entry = root.content.Where(e => e.path.Equals(packageRelativePath, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
					if (entry == null)
					{
						entry = new SimDataFileInfo();
						entry.path = packageRelativePath;
						root.content.Add(entry);
					}

					entry.date = DateTime.Now.Ticks - DateTime.UnixEpoch.Ticks;
					entry.size = fileInfo.Length;
				}

				File.WriteAllText(layoutJsonPath, JsonSerializer.Serialize(
					root,
					new JsonSerializerOptions
					{
						WriteIndented = true,
					}
				));
			}
			catch
			{
				return false;
			}

			return true;
		}

		private static string GetPackageRelativePath(string packageRoot, string filePath)
		{
			return Path.GetRelativePath(packageRoot, filePath).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}
	}
}
