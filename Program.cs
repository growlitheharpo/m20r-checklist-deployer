using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace M20R_Checklist_Deployer
{
	class Program
	{
		private const string M20R_Checklist_File = "M20R_Ovation_Checklist.xml";

		static void Main(string[] args)
		{
			var packagesLocation = SimDetection.FindSimPackagesFolder();
			if (string.IsNullOrEmpty(packagesLocation))
			{
				Console.WriteLine("Failed to find install location for Microsoft Flight Simulator.");
				return;
			}

			var ovationLocation = FindFolderForPackage(packagesLocation, "carenado-aircraft-m20r-ovation");
			if (!WriteChecklistFile(ovationLocation))
			{
				Console.WriteLine("Failed to update the checklist file.");
			}

			Console.WriteLine("Updated checklist file successfully!");
		}

		private static bool IsSubdirectory(string a, string b)
		{
			var dA = new DirectoryInfo(a);
			var dB = new DirectoryInfo(b);

			while (dB.Parent != null)
			{
				if (dB.Parent.FullName.Equals(dA.FullName, StringComparison.OrdinalIgnoreCase))
					return true;

				dB = dB.Parent;
			}

			return false;
		}

		private static string FindFolderForPackage(string root, string package)
		{
			var e = Directory.GetDirectories(root, package, SearchOption.AllDirectories);
			e = e.Where(potentialSub => !e.Any(potentialRoot => IsSubdirectory(potentialRoot, potentialSub))).ToArray();

			if (e.Length == 0)
				return null;

			// Prefer community, then official
			var communityVer = e.FirstOrDefault(p => IsSubdirectory(Path.Combine(root, "Community"), p));
			if (communityVer != default)
				return communityVer;

			var officialVer = e.FirstOrDefault(p => IsSubdirectory(Path.Combine(root, "Official"), p));
			if (officialVer != default)
				return officialVer;

			// Dunno what to do here, one of those should've worked...
			return null;
		}

		private static string GetResourceFullPath(string filename)
		{
			return $"{typeof(Program).Namespace}.{filename}";
		}

		private static Stream GetResourceStream(string filename)
		{
			return typeof(Program).Assembly.GetManifestResourceStream(GetResourceFullPath(filename));
		}

		private static bool WriteChecklistFile(string packageRoot)
		{
			var outFilePath = Path.Combine(packageRoot, "SimObjects", "Airplanes", "Carenado_M20R_Ovation");
			if (!Directory.Exists(outFilePath))
			{
				Console.WriteLine("Failed to find Carenado_M20R_Ovation within the package folder.");
				return false;
			}

			outFilePath = Path.Combine(outFilePath, "Checklist");
			Directory.CreateDirectory(outFilePath);

			outFilePath = Path.Combine(outFilePath, M20R_Checklist_File);

			using (Stream s = GetResourceStream(M20R_Checklist_File))
			using (FileStream outStream = File.Create(outFilePath))
			{
				s.CopyTo(outStream);
			}

			return LayoutHandling.UpdateLayoutForFile(packageRoot, outFilePath);
		}
	}
}
