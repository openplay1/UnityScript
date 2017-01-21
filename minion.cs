using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class minion : main {
	public NavMeshAgent agent;
	public NavMeshObstacle obs;
	public List<GameObject>goal1;
	public GameObject navi;
	public Rigidbody rb;
	[SerializeField] float followdist =10.0f;


	void Start () {
		if (team == Team.Evil)
			Hide ();
		deadTime = 0f;
		rb = GetComponent<Rigidbody> ();
		if (gameObject.tag == "Player.bot") {
			goal1 = new List<GameObject> (Routes.routemap["Goodbot"]);
		} 
		else if(gameObject.tag == "Evil.bot")
		{
			goal1 = new List<GameObject> (Routes.routemap["Evilbot"]);
		}
		else if(gameObject.tag == "Player.mid")
		{
			goal1 = new List<GameObject> (Routes.routemap["Goodmid"]);

		}
		else if(gameObject.tag == "Evil.mid")
		{
			goal1 = new List<GameObject> (Routes.routemap["Evilmid"]);

		}
		else if(gameObject.tag == "Player.top")
		{
			goal1 = new List<GameObject> (Routes.routemap["Goodtop"]);

		}
		else if(gameObject.tag == "Evil.top")
		{

			goal1 = new List<GameObject> (Routes.routemap["Eviltop"]);
		}




		m_DoState = IDLE;
		hp = hpmax;


	}



	public float CurrentCastRange() {
		return 0 <= currskill && currskill < skills.Count ? skills[currskill].castRange : 0;
	}



	void IDLE()
	{		if (bEnd ()) {
			m_DoState = END;


			return;
		}

		if (bDied ()) {
			target = null;
			currskill = -1;		
			m_DoState = DEAD;
			return;
		}


		if (bTargetTooFar()) {

			target = null;
			currskill = -1;

				m_DoState = IDLE;
				return;

		}


		if (bToSeek()) {
			obs.enabled = false;
			agent.enabled = true;
			agent.stoppingDistance =  0 ;
			agent.destination = target.Coll.ClosestPointOnBounds(transform.position);
			m_DoState = MOVE;
			return;
		}






		if (bSkillRequest()) {

			var skill = skills[currskill];
			GetComponent<Animator>().SetTrigger("ANIMA");
			skill.castTimeEnd = Time.time + skill.castTime;
			skills[currskill] = skill;
			m_DoState = CAST;
			return;
		}












		if (bWatch()) {	
			if (skills.Count > 0) currskill = 0;
			else Debug.LogError(name + " has no skills to attack with.");
			m_DoState = IDLE;
			return;
		}


		if (goal1 != null) {
			obs.enabled = false;
			agent.enabled = true;
			agent.stoppingDistance = 0;
			agent.destination = goal1 [0].gameObject.transform.position;
			m_DoState = MOVE;
			return;
		} else {
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

		obs.enabled = false;
		agent.enabled = true;
		transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(navi.transform.forward), Time.deltaTime * 8);
		transform.position=Vector3.Lerp(transform.position, navi.transform.position, Time.deltaTime*2);


		if (bDied ()) {
			target = null;
			currskill = -1;		
			m_DoState = DEAD;
			return;
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
			obs.enabled = false;
			agent.enabled = true;
			agent.stoppingDistance =0 ;
		
			agent.destination = target.Coll.ClosestPointOnBounds(transform.position);
			m_DoState = MOVE;
			return;
		}



		if (bSkillRequest()) {

			var skill = skills[currskill];
			GetComponent<Animator>().SetTrigger("ANIMA");
			skill.castTimeEnd = Time.time + skill.castTime;
			skills[currskill] = skill;
			m_DoState = CAST;
			return;
		}

		if (bWatch()) {
			
			if (skills.Count > 0) currskill = 0;
			else Debug.LogError(name + " has no skills to attack with.");
			agent.ResetPath();
		
			m_DoState = IDLE;
			return;
		}

		if (goal1 != null) {

			agent.stoppingDistance = 0;
			agent.destination = goal1 [0].gameObject.transform.position;

			m_DoState = MOVE;
			return;
		} else {
			m_DoState = IDLE;
			return;
		}

	}



	void CAST()
	{
		if (bEnd ()) {
			m_DoState = END;


			return;
		}

		agent.enabled = false;
		obs.enabled = true;

		if (bDied ()) {
			target = null;
			currskill = -1;		
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
			if (!CastCheckDistance (skills [currskill])) {

				target = null;
				currskill = -1;
				m_DoState = IDLE;
				return;
			}
			Castskill(skills[currskill]);
			if (target.hp == 0) target = null;
			currskill = -1;
			m_DoState = IDLE;

			return;
		}

	}


	void DEAD()
	{
		obs.enabled = false;
		deadTime+=Time.deltaTime;
		if (deadTime >= 2f)
			Destroy (gameObject.transform.parent.gameObject);
	}


	void END()
	{	
		agent.enabled = false;
		obs.enabled = false;


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


	bool bEnd()
	{
		return EndGame.end == true;
	}


	public override void Watched(main GO) {

		if (hp > 0 && GO != null && GO.hp > 0 && GO.team != team &&GO.bushstate==0) {

			if (target == null || ClosestDistance(Coll, GO.Coll) < ClosestDistance(Coll, target.Coll) * 0.8f)
				target = GO;
		}
	}

















	void pointcheck(){
		if (Vector3.Distance (transform.position, goal1 [0].gameObject.transform.position) <= 3.0f) {
			if(goal1.Count>1)
			goal1.RemoveAt (0);
		}
	}















	void Update () {
		
		pointcheck ();
		m_DoState ();

		GetComponent<Animator>().SetInteger("HP", hp);
		GetComponent<Animator>().SetFloat("Speed", agent.velocity.magnitude);
		GetComponent<Animator>().SetInteger("Attack", currskill);
	}
}
