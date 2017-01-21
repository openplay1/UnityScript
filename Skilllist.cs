using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
[CreateAssetMenu(fileName="New Skill", menuName="Skill", order=999)]
public class Skilllist : ScriptableObject {

	public string category;
	public bool followupDefaultAttack;

	[TextArea(1, 30)] public string tooltip;
	public Sprite image;
	public Projectile projectile; 
	public LineSkills linsskills;
	public AoeSkills aoeskills;



	[System.Serializable]
	public struct SkillLevel {

		public int damage;
		public float castTime;
		public float cooldown;
		public float castRange;
		public float aoeRadius;
		public int manaCosts;
		public int healsHp;
		public int healsMp;
		public float buffTime;
		public int buffsHpMax;
		public int buffsMpMax;
		public int buffsDamage;
		public int buffsDefense;
		public float buffsHpPercentPerSecond; 
		public float buffsMpPercentPerSecond; 


	
		public int requiredLevel; 
	}


	public SkillLevel[] levels = new SkillLevel[]{new SkillLevel()}; 

	public bool learnDefault; 

	static Dictionary<string, Skilllist> cache = null;
	public static Dictionary<string, Skilllist> dict {
		get {
				
			if (cache == null)
				cache = Resources.LoadAll<Skilllist>("Skill").ToDictionary(
					item => item.name, item => item
				);
			return cache;
		}
	}


}
