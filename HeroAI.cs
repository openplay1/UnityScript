using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class HeroAI :  main {
	public NavMeshAgent agent;
	[SerializeField] float followdist =10.0f;
	 main TowerInfo;
	 main PlayerInfo;
	public main Friendly;
	public List<GameObject>PointInfo;
	GameObject MoveTarget;
	int MoveIndex;
	public GameObject Respawnpoint;
	public GameObject DEADPoiint;
	[SerializeField] float RespawnTime;
	float RespawnTimeEnd;

	float hppercent;


	void Start () {
		MoveIndex = 0;
		m_DoState = IDLE;

		hp = hpmax;



	
		if (gameObject.tag == "Player.bot") {
			PointInfo = new List<GameObject> (Routes.routemap["HeroAIbotblue"]);

		} 
		else if(gameObject.tag == "Evil.bot")
		{
			PointInfo = new List<GameObject> (Routes.routemap["HeroAIbotred"]);
		}

		else if(gameObject.tag == "Evil.mid")
		{

			PointInfo = new List<GameObject> (Routes.routemap["HeroAImid"]);
		}
		else if(gameObject.tag == "Player.top")
		{
			PointInfo = new List<GameObject> (Routes.routemap["HeroAItopblue"]);

		}
		else if(gameObject.tag == "Evil.top")
		{

			PointInfo = new List<GameObject> (Routes.routemap["HeroAItopred"]);

		}

	}


	public float CurrentCastRange() {
		return 0 <= currskill && currskill < skills.Count ? skills[currskill].castRange : 0;
	}



	void IDLE()
	{
		if (bEnd ()) {
			m_DoState = END;

			return;
		}

		if (bDied ()) {
			agent.ResetPath ();
			target = null;
			currskill = -1;		
			MoveIndex = 0;
			RespawnTimeEnd = RespawnTime + Time.time;
			m_DoState = DEAD;

			return;
		}

		if (back ()) {

			if (MoveIndex != 0) {
				agent.stoppingDistance = 0;
				agent.destination = PointInfo [MoveIndex - 1].gameObject.transform.position;
				if (Vector3.Distance (transform.position, PointInfo [MoveIndex - 1].gameObject.transform.position) <= 1.0f) {	
					MoveIndex--;
					if (MoveIndex < 0)
						MoveIndex = 0;
				}
				m_DoState = MOVE;
				return;
			}
		}



		if (bTargetTooFar()) {
			target = null;
			currskill = -1;

			m_DoState = IDLE;
			return;

		}


	


		if (bToSeek()) {
			agent.stoppingDistance = 0 ;
			agent.destination = target.Coll.ClosestPointOnBounds(transform.position);
			m_DoState = MOVE;
			return;
		}




		if (bSkillRequest()) {

			var skill = skills[currskill];
			//if (CastCheckSelf(skill) && CastCheckTarget(skill)) {
			GetComponent<Animator>().SetTrigger("Flag");
		

			skill.castTimeEnd = Time.time + skill.castTime;
			skills[currskill] = skill;
			m_DoState = CAST;
			return;
		}
			
	
		if (bWatch()) {

			if (skills.Count > 0) {
				if (skills [1].CooldownRemaining () == 0f&&(target is Play||target is minion||target is HeroAI))
					currskill = 1;
				else
					currskill = 0;

			}
			else Debug.LogError(name + " has no skills to attack with.");
			agent.ResetPath();

			m_DoState = IDLE;
			return;
		}

		if (PointInfo != null) {
			agent.stoppingDistance = 0;
			agent.destination = PointInfo [MoveIndex].gameObject.transform.position;
			if (Vector3.Distance (transform.position, PointInfo [MoveIndex].gameObject.transform.position) <= 1.0f) {	
				MoveIndex++;
				if (MoveIndex > PointInfo.Count)
					MoveIndex = PointInfo.Count;
			}
			m_DoState = MOVE;
			return;
		}else {
				m_DoState = IDLE;
				return;
			}
	}


	void MOVE()
	{
		
		if (bEnd ()) {
			m_DoState = END;


			return;
		}

		if (bDied ()) {
			agent.ResetPath ();
			target = null;
			currskill = -1;		
			MoveIndex = 0;
			RespawnTimeEnd = RespawnTime + Time.time;
			m_DoState = DEAD;
			return;
		}



		if (back ()) {
			if (MoveIndex != 0) {
				agent.stoppingDistance = 0;
				agent.destination = PointInfo [MoveIndex - 1].gameObject.transform.position;
				if (Vector3.Distance (transform.position, PointInfo [MoveIndex - 1].gameObject.transform.position) <= 1.0f) {	
					MoveIndex--;
					if (MoveIndex < 0)
						MoveIndex = 0;
				}
				m_DoState = MOVE;
				return;
			}
		}


		if (bTargetDied()) {
			target = null;
			currskill = -1;
			agent.ResetPath();
			m_DoState = IDLE;
			return;
		}

		if (bTargetTooFar()) {
			target = null;
			currskill = -1;

			agent.ResetPath();
			m_DoState = IDLE;
			return;

		}


		if (bToSeek()) {

			agent.stoppingDistance =0;
			agent.destination = target.Coll.ClosestPointOnBounds(transform.position);
			m_DoState = MOVE;
			return;
		}





		if (bSkillRequest()) {

			var skill = skills[currskill];
			GetComponent<Animator>().SetTrigger("Flag");
		

			skill.castTimeEnd = Time.time + skill.castTime;
			skills[currskill] = skill;
			m_DoState = CAST;
			return;
		}
	
		if (bWatch()) {

			if (skills.Count > 0) {
				if (skills [1].CooldownRemaining () == 0f&&(target is Play||target is minion||target is HeroAI))
					currskill = 1;
				else
				currskill = 0;

			}
			else Debug.LogError(name + " has no skills to attack with.");
			agent.ResetPath();

			m_DoState = IDLE;
			return;
		}

		if (PointInfo != null) {

			agent.stoppingDistance = 0;
			agent.destination = PointInfo [MoveIndex].gameObject.transform.position;
			if (Vector3.Distance (transform.position, PointInfo [MoveIndex].gameObject.transform.position) <= 1.0f) {	
				MoveIndex++;
				if (MoveIndex > PointInfo.Count)
					MoveIndex = PointInfo.Count;
			}
			m_DoState = MOVE;
			return;
		}else {
			m_DoState = IDLE;
			return;
		}

	}



	void CAST()
	{if (bEnd ()) {
			m_DoState = END;


			return;
		}
		agent.ResetPath();

		if (bDied ()) {
			agent.ResetPath ();
			target = null;
			currskill = -1;		
			MoveIndex = 0;
			RespawnTimeEnd = RespawnTime + Time.time;
			m_DoState = DEAD;


			return;
		}

	

		if (target) LookAtY(target.transform.position);

		if (bTargetDisappeared()) {
			target = null;
			currskill = -1;
			m_DoState = IDLE;
			return;
		}

		if (bTargetDied()) {
			target = null;
			currskill = -1;
			m_DoState = IDLE;
			return;
		}
			




		if (bSkillFinished()) {

		
			var skill = skills[currskill];
			skillsetpoint = target.transform.position;
			Castskill(skills[currskill]);

			skill.cooldownEnd = Time.time + skill.cooldown;
			skills[currskill] = skill;

			if (target.hp == 0) target = null;


			currskill = -1;
			m_DoState = IDLE;

			return;
		}

	}


	void DEAD()
	{

		deadTime+=Time.deltaTime;
		float alpha;

		if (deadTime >= 1.5f) {
			var ren = GetComponentsInChildren<Renderer> ();		
			foreach (var m in ren) {
				if(m.material.shader!=Shader.Find ("Custom/deadshader"))
					m.material.shader = Shader.Find ("Custom/deadshader");
				alpha = 1 - deadTime * 0.2f;
				if (alpha <= 0)
					alpha = 0;
				m.material.SetFloat ("_Alpha", alpha);
			}
		}


		if (deadTime >= 4f)
			transform.position = DEADPoiint.transform.position;
		if (RespawnTimeEnd <= Time.time) {
			transform.position = new Vector3 (Respawnpoint.transform.position.x, 0, Respawnpoint.transform.position.z);
			agent.Warp (Respawnpoint.transform.position);
			deadTime = 0f;
			hp = hpmax;
			disableInvisible ();
			m_DoState = IDLE;
			return;
		}

	}

	void END(){
		agent.enabled = false;

	}



	bool bHpLow() {
		return hpmax/hp>5;
	}

	bool back() {
		if (team == Team.Good)
			return (Friendly == null) || main.teams [Team.Evil].Any (t => t.target == this && !(t is Play)&&!(t is HeroAI))||(hppercent<= 0.30); /*|| (target != null && ClosestDistance (Coll, target.Coll) < 3f);*/
		else 
			return (Friendly == null) || main.teams [Team.Good].Any (t => t.target == this && !(t is Play)&&!(t is HeroAI))||(hppercent<= 0.30); /*|| (target != null && ClosestDistance (Coll, target.Coll) < 3f);*/


	}
	bool bEnd()
	{
		return EndGame.end == true;
	}

	bool bTargetDisappeared() {
		return target == null;
	}

	bool bTargetTooFar() {


		return target != null && ClosestDistance(Coll, target.Coll) > followdist;

	}



	bool bTargetDied() {
		return target != null && target.hp == 0;
	}

	bool bDied() {
		return hp == 0;
	}

	bool bToSeek() {

		return target != null &&0 <= currskill && currskill < skills.Count &&!CastCheckDistance(skills[currskill]);
	}

	bool bWatch() {
		return target != null && target.hp > 0;
	}
		

	bool bSkillRequest() {
		return 0 <= currskill && currskill < skills.Count;        
	}

	bool bSkillFinished() {
		return 0 <= currskill && currskill < skills.Count &&
			skills[currskill].CastTimeRemaining() == 0f;        
	}





	public override void Watched(main GO) {

		if (hp > 0 && GO != null && GO.hp > 0 && GO.team != team &&GO.bushstate==0) {

			if (target == null || ClosestDistance(Coll, GO.Coll) < ClosestDistance(Coll, target.Coll) )
				target = GO;
		}

		else if (hp > 0 && GO != null && GO.hp > 0 && GO.team == team ) {

			if (Friendly == null || ClosestDistance(Coll, GO.Coll) < ClosestDistance(Coll, Friendly.Coll)  )
				Friendly = GO;
		}	
	}










	void Update () {
		hppercent = (float)hp /(float)hpmax;

		m_DoState ();


		if(Friendly != null )
		{ if (ClosestDistance (Coll, Friendly.Coll) > followdist || Friendly.hp == 0)
				Friendly = null;
		}


	
		GetComponent<Animator>().SetInteger("HP", hp);
		GetComponent<Animator>().SetFloat("Speed", agent.velocity.magnitude);
		GetComponent<Animator>().SetInteger("ATTACK", currskill);
	}
}
 