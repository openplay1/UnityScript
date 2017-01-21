using UnityEngine;
using System.Collections;

public struct skill  {


		public string name;


		public bool learned;
		public int level;
		public float castTimeEnd; 
		public float cooldownEnd; 
		public float buffTimeEnd; 



	     public skill(Skilllist template) {
			name = template.name;


			learned = template.learnDefault;
			level = 1;
		    castTimeEnd=cooldownEnd=buffTimeEnd=Time.time; 



		}



	public int damage {
		get { return Skilllist.dict[name].levels[level-1].damage; }
	}
	public float castTime {
		get { return Skilllist.dict[name].levels[level-1].castTime; }
	}
	public float cooldown {
		get { return Skilllist.dict[name].levels[level-1].cooldown; }
	}
	public float castRange {
		get { return Skilllist.dict[name].levels[level-1].castRange; }
	}
	public float aoeRadius {
		get { return Skilllist.dict[name].levels[level-1].aoeRadius; }
	}
	public bool followupDefaultAttack {
		get { return Skilllist.dict[name].followupDefaultAttack; }
	}
	public Projectile projectile {
		get { return Skilllist.dict[name].projectile; }
	}

	public LineSkills linsskills {
		get { return Skilllist.dict[name].linsskills; }
	}

	public AoeSkills aoeskills {
		get { return Skilllist.dict[name].aoeskills; }
	}

	public float CastTimeRemaining() {
		
		var tTime = Time.time ;

	
		return tTime >= castTimeEnd ? 0f : castTimeEnd - tTime;
	}

	public bool IsCasting() {

		return CastTimeRemaining() > 0f;
	}

	public float CooldownRemaining() {
		
		var tTime = Time.time ;


		return tTime >= cooldownEnd ? 0f : cooldownEnd - tTime;
	}
		

	public bool IsReady() {
		return CooldownRemaining() == 0f;
	}    



}
