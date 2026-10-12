using System;
using System.IO;
using Microsoft.Win32;
using System.Windows.Forms;

namespace NoxShared
{
	public class NoxDb
	{
		/// <summary>
		/// Nox installation directory
		/// </summary>
		public static string NoxPath;
		
		/// <summary>
		/// Must be pointing to a valid file in order for GetStream to work
		/// </summary>
		protected static string dbFile;
		
		protected static FileStream GetStream() { return File.OpenRead(NoxPath + dbFile); }
		
		const string REGISTRY_PATH = "SOFTWARE\\Westwood\\Nox";
		
		static readonly RegistryKey installPathKey;
		static readonly string directorySeparator = Path.DirectorySeparatorChar.ToString();
		
		/// <summary>
		/// Used for validating Nox installation path
		/// </summary>
		static bool CheckThingDb()
		{
           
            return File.Exists(NoxPath + directorySeparator + "video.bag");

		}
		
		/// <summary>
		/// Requests user to select Nox installation directory by hand.
		/// </summary>
		static void FindNoxPath()
		{
			var fbd = new FolderBrowserDialog();
			fbd.Description = "Please select your Nox install directory.";
			if (fbd.ShowDialog() == DialogResult.OK)
			{
				NoxPath = fbd.SelectedPath;
				// Look up for thing.db //thing.db???? why not video.bag?
				if (CheckThingDb()) return;
			}
			
			// Failure (fallthrough)
          
			MessageBox.Show("Failed to find or access Nox installation directory.\nEditor requires some important game resources in order to start properly.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			Environment.Exit(0);
		}
		
		static NoxDb()
		{
			NoxPath = Settings.Default.NoxInstallDir;
			// NOX_DATA is also used by OpenNox to point at the game data
			string envPath = Environment.GetEnvironmentVariable("NOX_DATA");
			if (!string.IsNullOrEmpty(envPath))
				NoxPath = envPath;
			// a valid NOX_DATA is used for this run only, not saved to the settings
			bool fromEnv = !string.IsNullOrEmpty(envPath) && CheckThingDb();
			// Check for Nox installation path
			if (!CheckThingDb())
			{
				// Try to use path from registry, request user if failed
				try
				{
					installPathKey = Registry.LocalMachine.OpenSubKey(REGISTRY_PATH);
				}
				catch (Exception)
				{
					// no registry access (Mono on Linux)
					installPathKey = null;
				}

				if (installPathKey == null) FindNoxPath();
				else
				{
					object val = installPathKey.GetValue("InstallPath");
					if (val == null) FindNoxPath();
					else NoxPath = (string) val;
				}

				// Get directory part if it's a path to file
				if (Path.HasExtension(NoxPath))
					NoxPath = Path.GetDirectoryName(NoxPath);
			}
			if (!NoxPath.EndsWith(directorySeparator, StringComparison.InvariantCultureIgnoreCase)) NoxPath += directorySeparator;
			if (!fromEnv && NoxPath != Settings.Default.NoxInstallDir)
			{
				// Save path for later
				Settings.Default.NoxInstallDir = NoxPath;
				Settings.Default.Save();
			}
		}
	}
}
