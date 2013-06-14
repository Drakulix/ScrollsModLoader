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

		public RepoManager ()
		{

			//add repos
			this.tryAddRepository ("http://mods.ScrollsGuide.com/");

			//load repo list
			String installPath = Platform.getGlobalScrollsInstallPath();
			String modLoaderPath = installPath + "ModLoader" + System.IO.Path.DirectorySeparatorChar;
			if (!File.Exists (modLoaderPath+"repo.ini")) {
				File.CreateText (modLoaderPath+"repo.ini").Close();
			}
			String[] repos = File.ReadAllLines (modLoaderPath+"repo.ini");
			foreach (String repo in repos)
				this.tryAddRepository(repo);

		}

		public void tryAddRepository(string url) {

			//normalize it
			Uri urlNorm = new Uri (url);
			url = urlNorm.Host;

			String repoinfo = null;
			try {
				WebClient client = new WebClient ();
				repoinfo = client.DownloadString (new Uri(url+"/repoinfo"));
			} catch {
				return;
			}

			RepoInfoMessage message = null;
			try {
				JsonReader reader = new JsonReader();
				message = reader.Read(repoinfo, typeof(RepoInfoMessage)) as RepoInfoMessage;
			} catch {
				App.Popups.ShowOk (this, "addFail", "Failure", url + " does not seem to be a valid Mod Repository", "OK");
				return;
			}

			if (message == null) {
				App.Popups.ShowOk (this, "addFail", "Failure", url + " does not seem to be a valid Mod Repository", "OK");
				return;
			}

			if (!message.msg.Equals("success")) {
				App.Popups.ShowOk (this, "addFail", "Failure", url + " does not seem to be a valid Mod Repository", "OK");
				return;
			}

			Repo repo = message.data;
			repositories.Add(repo);

			this.tryToFetchModList (repo);
		}

		public void tryToFetchModList(Repo repo) {

			String modlist = null;
			try {
				WebClient client = new WebClient ();
				modlist = client.DownloadString (new Uri(repo.url+"/modlist"));
			} catch {
				return;
			}

			ModListMessage message = null;
			try {
				JsonReader reader = new JsonReader();
				message = reader.Read(modlist, typeof(ModListMessage)) as ModListMessage;
			} catch {
				repositories.Remove (repo);
				App.Popups.ShowOk (this, "addFail", "Failure", repo.url + " does not seem to be a valid Mod Repository", "OK");
				return;
			}

			if (message == null) {
				repositories.Remove (repo);
				App.Popups.ShowOk (this, "addFail", "Failure", repo.url + " does not seem to be a valid Mod Repository", "OK");
				return;
			}

			if (!message.msg.Equals("success")) {
				repositories.Remove (repo);
				App.Popups.ShowOk (this, "addFail", "Failure", repo.url + " does not seem to be a valid Mod Repository", "OK");
				return;
			}

			modsPerRepo.Add (repo, new List<Item>(message.data));
		}

		public Mod getModOnRepo(Repo source, Mod localMod) {
			foreach (Repo repo in repositories)
				if (repo.Equals (source))
					return (Mod)modsPerRepo [repo].Find (localMod.Equals);
			return null;
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

		public Repo () {
			try {
				this.tex = new WWW (url+"/favicon.png");
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
			if (tex.isDone)
				return tex.texture;
			else
				return null;
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

