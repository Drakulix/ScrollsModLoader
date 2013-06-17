using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using JsonFx.Json;
using UnityEngine;

namespace ScrollsModLoader
{
	public class RepoManager : IOkCallback
	{
		public List<Item> repositories = new List<Item>();
		private Dictionary<Repo, List<Item>> modsPerRepo = new Dictionary<Repo, List<Item>>();
		private ModManager modManager;

		public RepoManager (ModManager modManager)
		{
			this.modManager = modManager;

			//add repos
			this.readRepository ("http://mods.ScrollsGuide.com/");

			//load repo list
			String installPath = Platform.getGlobalScrollsInstallPath();
			String modLoaderPath = installPath + "ModLoader" + System.IO.Path.DirectorySeparatorChar;
			if (!File.Exists (modLoaderPath+"repo.ini")) {
				File.CreateText (modLoaderPath+"repo.ini").Close();
			}
			String[] repos = File.ReadAllLines (modLoaderPath+"repo.ini");
			foreach (String repo in repos)
				this.readRepository(repo);

		}

		public void tryAddRepository(string url) {
			if (!this.readRepository (url)) {
				App.Popups.ShowOk (this, "addFail", "Failure", url + " does not seem to be a valid Mod Repository", "OK");
			} else {
				this.updateRepoList ();
			}
		}

		private bool readRepository(string url) {

			//normalize it
			Uri urlNorm = new Uri (url);
			url = urlNorm.Host;

			String repoinfo = null;
			try {
				WebClient client = new WebClient ();
				repoinfo = client.DownloadString (new Uri("http://"+url+"/repoinfo"));
			} catch {
				return false;
			}

			RepoInfoMessage message = null;
			try {
				JsonReader reader = new JsonReader();
				message = reader.Read(repoinfo, typeof(RepoInfoMessage)) as RepoInfoMessage;
			} catch {
				return false;
			}

			if (message == null) {
				return false;
			}

			if (!message.msg.Equals("success")) {
				return false;
			}

			Repo repo = message.data;
			repo.tryToGetFavicon ();
			repositories.Add(repo);

			return this.tryToFetchModList (repo);
		}

		public void removeRepository(Repo repo) {
			repositories.Remove (repo);
			updateRepoList ();
		}

		public bool tryToFetchModList(Repo repo) {

			String modlist = null;
			try {
				WebClient client = new WebClient ();
				modlist = client.DownloadString (new Uri(repo.url+"modlist"));
			} catch {
				repositories.Remove (repo);
				return false;
			}

			ModListMessage message = null;
			try {
				JsonReader reader = new JsonReader();
				message = reader.Read(modlist, typeof(ModListMessage)) as ModListMessage;
			} catch {
				repositories.Remove (repo);
				return false;
			}

			if (message == null) {
				repositories.Remove (repo);
				return false;
			}

			if (!message.msg.Equals("success")) {
				repositories.Remove (repo);
				return false;
			}

			modsPerRepo.Add (repo, new List<Item>(message.data));
			return true;
		}

		public List<Item> getModListForRepo(Repo source) {
			List<Item> modlist = null;
			foreach (Repo repo in repositories)
				if (repo.Equals (source))
						modlist = modsPerRepo [repo];
			if (modlist != null) {
				modlist = modlist.FindAll (delegate(Item mod) {
					foreach (LocalMod lmod in modManager.installedMods) {
						if (lmod.Equals(mod) && lmod.source.Equals(source))
							return false;
					}
					return true;
				});
			}
			return modlist;
		}

		public Mod getModOnRepo(Repo source, Mod localMod) {
			foreach (Repo repo in repositories)
				if (repo.Equals (source))
					return (Mod)modsPerRepo [repo].Find (localMod.Equals);
			return null;
		}

		public void updateRepoList() {
			String installPath = Platform.getGlobalScrollsInstallPath();
			String modLoaderPath = installPath + "ModLoader" + System.IO.Path.DirectorySeparatorChar;
			File.Delete (modLoaderPath+"repo.ini");
			StreamWriter repoWriter = File.CreateText (modLoaderPath+"repo.ini");
			foreach (Repo repo in repositories) {
				if (repo.Equals(repositories[0])) continue;
				repoWriter.WriteLine (repo.url);
			}
			repoWriter.Flush ();
			repoWriter.Close ();
		}

		public void PopupOk (string popupType)
		{
			return;
		}
	}

	public class Repo : Item {

		public String name;
		public String url;
		public int version;
		public int mods;
		private WWW tex;

		public Repo () {}

		public void tryToGetFavicon() {
			try {
				this.tex = new WWW (url + "/favicon.png");
			} catch {
				this.tex = null;
			}
		}

		public bool selectable ()
		{
			return true;
		}
		public Texture getImage ()
		{
			if (tex == null) {
				try {
					this.tex = new WWW (url+"/favicon.png");
				} catch {
					this.tex = null;
					return null;
				}
			}
			return tex.texture;
		}
		public string getName ()
		{
			return name;
		}
		public string getDesc ()
		{
			return url;
		}

		public override bool Equals(object repo) {
			if (repo is Repo)
				return (this.name.Equals ((repo as Repo).name) && this.url.Equals ((repo as Repo).url));
			else
				return false;
		}
	}
}

