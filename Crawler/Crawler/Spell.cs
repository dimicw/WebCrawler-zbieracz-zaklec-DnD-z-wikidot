using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawler
{
	public class Spell
	{
		public string name;
		public string source;
		public int level;
		public string school;
		public string castingTime;
		public bool ritual;
		public string range;
		public string components;
		public bool v, s, m;
		public string duration;
		public bool concentration;
		public string[] description;
		public string atHigherLevels;
		public string spellLists;
		public Classes spellListsDivided;

		public Spell()
		{
			name = "";
			source = "";
			level = -1;
			school = "";
			castingTime = "";
			range = "";
			components = "";
			duration = "";
			description = new string[0];
			atHigherLevels = "";
			spellLists = "";
			autoFill();
		}

		public Spell(string name, string source, int level, string school, string castingTime, string range, 
			string components, string duration, string[] description, string spellLists, string atHigherLevels)
		{
			this.name = name;
			this.source = source;
			this.level = level;
			this.school = school;
			this.castingTime = castingTime;
			this.range = range;
			this.components = components;
			this.duration = duration;
			this.description = description;
			this.spellLists = spellLists;
			this.atHigherLevels = atHigherLevels;
			autoFill();
		}

		public void autoFill()
		{
			//VSM
			string vsm;
			if (components.Contains("("))
				vsm = Crawler.Between(components, "", " (");
			else
				vsm = components;
			if (vsm.ToLower().Contains("v")) v = true;
			if (vsm.ToLower().Contains("s")) s = true;
			if (vsm.ToLower().Contains("m")) m = true;

			//classes
			spellListsDivided = new Classes(spellLists);

			//concentration
			if (duration.ToLower().Contains("concentration"))
				concentration = true;

			//ritual
			if (castingTime.ToLower().Contains("ritual"))
				ritual= true;
		}
	}

	public class Classes
	{
		public bool artificer;
		public bool bard;
		public bool cleric;
		public bool druid;
		public bool paladin;
		public bool ranger;
		public bool sorcerer;
		public bool warlock;
		public bool wizard;

		public Classes()
		{
			artificer = false;
			bard = false;
			cleric = false;
			druid = false;
			paladin = false;
			ranger = false;
			sorcerer = false;
			warlock = false;
			wizard = false;
		}

		public Classes(string spellLists)
		{
			artificer = false;
			bard = false;
			cleric = false;
			druid = false;
			paladin = false;
			ranger = false;
			sorcerer = false;
			warlock = false;
			wizard = false;
			Fill(spellLists);
		}

		public void Fill(string spellLists)
		{
			if (spellLists.ToLower().Contains("artificer")) artificer = true;
			if (spellLists.ToLower().Contains("bard")) bard = true;
			if (spellLists.ToLower().Contains("cleric")) cleric = true;
			if (spellLists.ToLower().Contains("druid")) druid = true;
			if (spellLists.ToLower().Contains("paladin")) paladin = true;
			if (spellLists.ToLower().Contains("ranger")) ranger = true;
			if (spellLists.ToLower().Contains("sorcerer")) sorcerer = true;
			if (spellLists.ToLower().Contains("warlock")) warlock = true;
			if (spellLists.ToLower().Contains("wizard")) wizard = true;
		}
	}
}
