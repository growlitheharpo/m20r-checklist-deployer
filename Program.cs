﻿using System;
using System.IO;
using System.Linq;

namespace M20R_Checklist_Deployer
{
	class Program
	{
		private const string M20R_Checklist_File = "M20R_Ovation_Checklist.xml";

		static void Main(string[] args)
		{
			if (WriteWarningUserAccepts())
				DeployChecklist();

			Console.WriteLine();
			Console.WriteLine("Press any key to continue...");
			Console.ReadKey(true);
		}

		private static bool WriteWarningUserAccepts()
		{
			Console.WriteLine("+========================================================================+");
			Console.WriteLine("|               In-Game Checklist for Carenado M20R Ovation              |");
			Console.WriteLine("|                        github.com/growlitheharpo                       |");
			Console.WriteLine("|                                                                        |");
			Console.WriteLine("| This program adds a very rudimentary checklist for the Carenado M20R.  |");
			Console.WriteLine("| You must purchase this aircraft separately (through the in-game        |");
			Console.WriteLine("| marketplace) for this to work. The checklist is currently very basic   |");
			Console.WriteLine("| and attempts to mimic the PDF checklist provided by Carenado by just   |");
			Console.WriteLine("| re-using steps from the framework created by Asobo for the stock       |");
			Console.WriteLine("| aircraft. Camera hints are not yet implemented and it is unlikely to   |");
			Console.WriteLine("| work with the AI co-pilot.                                             |");
			Console.WriteLine("|                                                                        |");
			Console.WriteLine("+========================================================================+");
			Console.WriteLine();
			Console.Write("Enter (Y/y) to continue, or any other key to exit without installing: ");

			var input = Console.ReadLine();
			Console.WriteLine();
			return input.ToLower() == "y";
		}

		private static void DeployChecklist()
		{
			Console.WriteLine("Preparing to deploy...");
			var packagesLocation = SimDetection.FindSimPackagesFolder();
			if (string.IsNullOrEmpty(packagesLocation))
			{
				Console.WriteLine("Failed to find install location for Microsoft Flight Simulator.");
				return;
			}

			Console.WriteLine($"MSFS Packages location found: {packagesLocation}");

			var ovationLocation = FindFolderForPackage(packagesLocation, "carenado-aircraft-m20r-ovation");
			if (string.IsNullOrEmpty(ovationLocation))
			{
				Console.WriteLine("Failed to find the Carenado M20R Ovation in your packages folder.");
				Console.WriteLine("Please ensure you have purchased and installed it.");
				return;
			}

			Console.WriteLine($"M20R Package Located: {Path.GetRelativePath(packagesLocation, ovationLocation)}");

			if (!WriteChecklistFile(ovationLocation))
			{
				Console.WriteLine("Failed to update the checklist file.");
			}

			Console.WriteLine("Deployed checklist file successfully!");
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

			// Prefer community if one were to exist (since it will overlay), then official
			var communityVer = e.FirstOrDefault(p => IsSubdirectory(Path.Combine(root, "Community"), p));
			if (communityVer != default)
				return communityVer;

			var officialVer = e.FirstOrDefault(p => IsSubdirectory(Path.Combine(root, "Official"), p));
			if (officialVer != default)
				return officialVer;

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
				Console.WriteLine("Failed to find Carenado_M20R_Ovation SimObject within the package folder.");
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
