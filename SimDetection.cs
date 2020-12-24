using System;
using System.IO;
using System.Linq;

namespace M20R_Checklist_Deployer
{
	// The following largely comes from msfs-mod-manager, translated from Python
	// https://github.com/NathanVaughn/msfs-mod-manager/blob/master/src/main/python/lib/flight_sim.py
	static class SimDetection
	{
		private const string FlightSimulatorCfg = "FlightSimulator.CFG";
		private const string UserCfg = "UserCfg.opt";

		public static string FindSimPackagesFolder()
		{
			// Try to find it with Steam
			{
				var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				var msfs = Path.Join(appData, "Microsoft Flight Simulator");
				if (IsSimCacheFolder(msfs))
				{
					string packages = GetPackagesFromUserCfg(msfs);
					if (!string.IsNullOrEmpty(packages))
					{
						return packages;
					}
				}
			}

			// MS Store detection
			{
				var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				var msfs = Path.Join(localAppData, "Packages", "Microsoft.FlightSimulator_8wekyb3d8bbwe", "LocalCache");
				if (IsSimCacheFolder(msfs))
				{
					string packages = GetPackagesFromUserCfg(msfs);
					if (!string.IsNullOrEmpty(packages))
					{
						return packages;
					}
				}
			}

			// Boxed edition
			{
				var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				var msfs = Path.Join(localAppData, "MSFSPackages");
				if (IsSimPackagesFolder(msfs))
				{
					return msfs;
				}
			}

			// Skip fallback steam detections

			// TODO: User prompt on failure?

			return null;
		}

		private static bool IsSimCacheFolder(string path)
		{
			// Returns if FlightSimulator.CFG exists inside the given directory. Not a perfect test, but a solid guess.
			// https://github.com/NathanVaughn/msfs-mod-manager/blob/master/src/main/python/lib/flight_sim.py#L145
			return Directory.Exists(path) && File.Exists(Path.Combine(path, FlightSimulatorCfg));
		}

		private static bool IsSimPackagesFolder(string path)
		{
			if (!Directory.Exists(path))
				return false;

			var subdirs = Directory.GetDirectories(path).Select(Path.GetFileName);

			var desiredFolders = new[] { "Official", "Community" };
			return desiredFolders.All(d => subdirs.Contains(d, StringComparer.OrdinalIgnoreCase));
		}

		private static string GetPackagesFromUserCfg(string userCfgFolder)
		{
			var cfgFilePath = Path.Join(userCfgFolder, UserCfg);

			try
			{
				using (var cfgStream = File.OpenRead(cfgFilePath))
				using (var reader = new StreamReader(cfgStream))
				{
					for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
					{
						var splits = line.Split("InstalledPackagesPath ");
						if (splits.Length > 1)
						{
							var folder = splits.Skip(1).First().Trim('"');
							if (IsSimPackagesFolder(folder))
							{
								return folder;
							}
						}
					}
				}
			}
			catch
			{
			}

			return null;
		}
	}
}
