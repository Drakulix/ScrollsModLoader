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
			this.readRepository ("http://mods.Scrollsguide.com/");

			//load repo list
			String installPath = Platform.getGlobalScrollsInstallPath();
			String modLoaderPath = installPath + "ModLoader" + System.IO.Path.DirectorySeparatorChar;
			if (!File.Exists (modLoaderPath+"repo.ini")) {
				File.CreateText (modLoaderPath+"repo.ini").Close();
			}
			String[] repos = File.ReadAllLines (modLoaderPath+"repo.ini");
			foreach (String repo in repos)
				this.readRepository(repo);
			//This hurts, but we need to do it to have Github repos/ssl repos.
			//Trust Everyone:
			System.Net.ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;



		}

		public void tryAddRepository(string url) {
			Uri uri = new Uri (url);
			foreach (Item repo in repositories) {
				if ((repo as Repo).urlUri.Equals (uri))
					return;
			}

			if (!this.readRepository (url)) {
				App.Popups.ShowOk (this, "addFail", "Failure", url + " does not seem to be a valid Mod Repository", "OK");
			} else {
				this.updateRepoList ();
			}
		}

		private bool readRepository(string url) {

			//normalize it
			Uri urlNorm = new Uri (url);

			String repoinfo = null;
			try {
				WebClientTimeOut client = new WebClientTimeOut ();
				repoinfo = client.DownloadString (new Uri(urlNorm,"repoinfo"));
			} catch (WebException ex) {
				Console.WriteLine ("Failed to read repoinfo from URL: " + new Uri(urlNorm,"repoinfo"));
				Console.WriteLine (ex);
				return false;
			}

			RepoInfoMessage message = null;
			try {
				JsonReader reader = new JsonReader();
				message = reader.Read(repoinfo, typeof(RepoInfoMessage)) as RepoInfoMessage;
			} catch {
				Console.WriteLine("Failed to parse RepoInfo-JSON");
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
				WebClientTimeOut client = new WebClientTimeOut ();
				modlist = client.DownloadString (new Uri(repo.urlUri,"modlist"));
			} catch (WebException ex) {
				Console.WriteLine ("Failed to read modlist from URL: " + repo.urlUri);
				Console.WriteLine (ex);
				repositories.Remove (repo);
				return false;
			}

			ModListMessage message = null;
			try {
				JsonReader reader = new JsonReader();
				message = reader.Read(modlist, typeof(ModListMessage)) as ModListMessage;
			} catch {
				Console.WriteLine ("Failed to parse ModList-JSON");
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

		public Mod getModOnRepo(Repo source, LocalMod localMod) {
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
				repoWriter.WriteLine (repo.urlUri);
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
		public Uri urlUri { get {return new Uri (url);}}
		public String url;
		public int version;
		public int mods;
		private WWW tex;
		private Texture texture;

		public Repo () {}

		public void tryToGetFavicon() {
			try {
				this.tex = new WWW (urlUri + "favicon.png");
			} catch {
				texture = new Texture ();
				this.tex = null;
			}
		}

		public bool selectable ()
		{
			return true;
		}
		public Texture getImage ()
		{
			if (texture != null)
				return texture;
			if (tex == null) {
				try {
					this.tex = new WWW (urlUri+"favicon.png");
				} catch {
					this.tex = null;
					texture = new Texture ();
					return texture;
				}
			} else {
				if (tex.isDone) {
					texture = tex.texture;
					tex = null;
				}
			}
			return texture;
		}
		public string getName ()
		{
			return name;
		}
		public string getDesc ()
		{
			return urlUri.ToString();
		}

		public override bool Equals(object repo) {
			if (repo is Repo)
				return (this.name.Equals ((repo as Repo).name) && this.urlUri.Equals ((repo as Repo).urlUri));
			else
				return false;
		}

		public override int GetHashCode () {
			return urlUri.GetHashCode();
		}
	}
}

