using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Play : main {
	public NavMeshAgent mNavMesh;
	public int skillcast=-1; 
	int skillNext=-1;
	main targetNext;
	int atkani=-1;
	int currentskillstate=-1;
	GameObject indicator;
	GameObject indicatorAoe;
	GameObject indicatorArrow;
	public GameObject Respawnpoint;
	public GameObject DEADPoiint;
	[SerializeField] float RespawnTime;
	float RespawnTimeEnd;
	public KeyCode[] skillHotkeys = new KeyCode[] {KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R};
	public GameObject Mcamera;

	void Start () {
		skillcast = -1;
		mNavMesh = GetComponent<NavMeshAgent>();
		m_DoState = IDLE;
		hp = hpmax;
	}



	HashSet<string> cmdEvents = new HashSet<string>();

	public void CmdCancelAction() { cmdEvents.Add("CancelAction"); }


	Vector3 navigatePos = Vector3.zero;
	float navigateStop = 0;
    public void CmdNavigateTo(Vector3 pos, float stoppingDistance) {
		navigatePos = pos; navigateStop = stoppingDistance;
		cmdEvents.Add("NavigateTo");
	}




	void MouseControl()
	{

		bool left = Input.GetMouseButtonDown (0);
		bool right = Input.GetMouseButtonDown (1) && !EventSystem.current.IsPointerOverGameObject ();

		var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit)) {

			if (left || right) {

				if (left) {


					var entity = hit.transform.GetComponent<main>();
					if (entity) {
						if (0 < skillcast && skillcast < skills.Count) {
							if ((entity is minion||entity is Tower||entity is HeroAI||entity is MinionSpawnTower||entity is Base)&&entity.team!=team) {
								
								if (entity.hp > 0) {
				
									if (skills[skillcast].IsReady()) {
										SetTarget (entity);
										if (target) LookAtY(target.transform.position);
										skillsetpoint = hit.point;
										Useskill(skillcast);
										skillcast = -1;
									} else {
										CmdNavigateTo(entity.Coll.ClosestPointOnBounds(transform.position),
											skills[skillcast].castRange);
									}
								} else {

									CmdNavigateTo(entity.Coll.ClosestPointOnBounds(transform.position), 4f);
								}
							} 
						} 
					}
					else if (skillcast!= -1&&skills[skillcast].linsskills!=null&&skills[skillcast].IsReady()) {
					     	target = null;
						mNavMesh.ResetPath ();
							LookAtY(hit.point);
						skillsetpoint = hit.point;
					
							Useskill(skillcast);
							skillcast = -1;

					}


					else if (skillcast!= -1&&skills[skillcast].aoeskills!=null&&skills[skillcast].IsReady()) {
						
							target = null;
							mNavMesh.ResetPath ();
							LookAtY (hit.point);
							skillsetpoint = hit.point;
							Useskill (skillcast);
							skillcast = -1;

					
					}

				} 
				else if (right) {
					
				var entity = hit.transform.GetComponent<main>();
					if ((entity is minion||entity is Tower||entity is HeroAI||entity is MinionSpawnTower||entity is Base)&&entity.team!=team) {
						if (entity.hp > 0) {
							if (skills.Count > 0 && skills [0].IsReady ()) {
								SetTarget (entity);
								Useskill (0);
							}
							else {
								CmdNavigateTo(entity.Coll.ClosestPointOnBounds(transform.position),
									skills.Count > 0 ? skills[0].castRange : 0f);
							}
						}
					} else {
						
						CmdNavigateTo(hit.point, 0f);
					}
					skillcast = -1;
				}

			}
		}

	}



	public void Useskill(int index)
	{
		if ((m_DoState == IDLE || m_DoState == MOVE || m_DoState == CAST) &&
			0 <= index && index < skills.Count) {
	
			if (skills[index].learned && skills[index].IsReady()) {
				if (currskill == -1 || m_DoState != CAST)
					currskill = index;
				else if (currskill != index)
					skillNext = index;
			}
		}

	}


	void SetTarget(main tr) {
			if (m_DoState == IDLE || m_DoState == MOVE)
				target = tr;
			else if (m_DoState == CAST)
			targetNext = tr;
		
	}







	void IDLE()
	{
		if (bEnd ()) {
			m_DoState = END;


			return;
		}
		if (bDied ()) {
			mNavMesh.ResetPath();
			target = null;
			currskill =skillNext= -1;	
			RespawnTimeEnd = Time.time + RespawnTime;
			m_DoState = DEAD;
			return;
		}



		if (bNavigateTo()) {

			currskill = skillNext = -1;
			// move
			mNavMesh.stoppingDistance = navigateStop;
			mNavMesh.destination = navigatePos;
			m_DoState = MOVE;
			return;
		}

		if (bATKRequest()) {

			var skill = skills[currskill];
			targetNext = target;


			if (CastCheckDistance(skill)) {

				mNavMesh.ResetPath();
				skill.castTimeEnd = Time.time + skill.castTime;
				skills [currskill] = skill;
				GetComponent<Animator> ().SetTrigger ("Flag");
				m_DoState= CAST;
				return;
			} else {
	
				mNavMesh.stoppingDistance = skill.castRange;
				mNavMesh.destination = target.Coll.ClosestPointOnBounds(transform.position);
				m_DoState= MOVE;
				return;
			}

		}

		if (bSkillRequest ()) {

			var skill = skills[currskill];
			targetNext = target; 

			if (CastCheckDistance(skill)||skill.linsskills!=null) {
				if (target != null)
				skillsetpoint = target.transform.position;
				    mNavMesh.ResetPath();
					skill.castTimeEnd = Time.time + skill.castTime;
				    skills[currskill] = skill;
				GetComponent<Animator>().SetTrigger("Flag");
				m_DoState= CAST;
				return;
			}else if (target != null) {

				mNavMesh.stoppingDistance = skill.castRange;
				mNavMesh.destination = target.Coll.ClosestPointOnBounds (transform.position);
				m_DoState = MOVE;
				return;
			} else {

				mNavMesh.stoppingDistance = skill.castRange;
				mNavMesh.destination = skillsetpoint;
				m_DoState= MOVE;
				return;
			}

		}

	}


	void MOVE()
	{if (bEnd ()) {
			m_DoState = END;


			return;
		}
		if (bDied ()) {
			mNavMesh.ResetPath();
			target = null;
			currskill =skillNext= -1;		
			RespawnTimeEnd = Time.time + RespawnTime;
			m_DoState = DEAD;
			return;
		}


		if (bNavigateTo()) {
			
			currskill = skillNext = -1;
			// move
			mNavMesh.stoppingDistance = navigateStop;
			mNavMesh.destination = navigatePos;
			m_DoState = MOVE;
			return;
		}

		if (bATKRequest()) {

			var skill = skills[currskill];
			targetNext = target; 

			if (CastCheckDistance(skill)) {
				
				mNavMesh.ResetPath();
				skill.castTimeEnd = Time.time + skill.castTime;
				skills [currskill] = skill;
				GetComponent<Animator> ().SetTrigger ("Flag");
				m_DoState= CAST;
				return;
			} else {
				
				mNavMesh.stoppingDistance = skill.castRange;
				mNavMesh.destination = target.Coll.ClosestPointOnBounds(transform.position);
				m_DoState= MOVE;
				return;
			}

		}

		if (bSkillRequest ()) {

			var skill = skills[currskill];
			targetNext = target; 
			if (CastCheckDistance (skill) || skill.linsskills != null) {
				if(target!=null)
				skillsetpoint = target.transform.position;
				mNavMesh.ResetPath ();
				skill.castTimeEnd = Time.time + skill.castTime;
				skills [currskill] = skill;
				GetComponent<Animator> ().SetTrigger ("Flag");
				m_DoState = CAST;
				return;
			} else if (target != null) {

				mNavMesh.stoppingDistance = skill.castRange;
				mNavMesh.destination = target.Coll.ClosestPointOnBounds (transform.position);
				m_DoState = MOVE;
				return;
			} else {

				mNavMesh.stoppingDistance = skill.castRange;
				mNavMesh.destination = skillsetpoint;
				m_DoState= MOVE;
				return;
			}


		}
	}



	void CAST()
	{if (bEnd ()) {
			m_DoState = END;


			return;
		}
		if (bDied ()) {
			mNavMesh.ResetPath();
			target = null;
			currskill =skillNext= -1;		
			RespawnTimeEnd = Time.time + RespawnTime;
			m_DoState = DEAD;
			return;
		}




		if (target) LookAtY(target.transform.position);





		if (bNavigateTo()) {
			currskill = skillNext = -1;
			// move
			mNavMesh.stoppingDistance = navigateStop;
			mNavMesh.destination = navigatePos;
			m_DoState = MOVE;
			return;
		}


		if (bTargetDisappeared()) {
			
			currskill = skillNext = -1;
			m_DoState = IDLE;
			return;

		}
		if (bTargetDied()) {
			

			currskill = skillNext = -1;
			m_DoState = IDLE;
			return;
			
		}





		if (bATKFinished()) {
			
			if (!CastCheckDistance (skills [currskill])) {

				target = null;
				currskill = -1;
				m_DoState = IDLE;
				return;
			}


			DealDamage(target,skills[0].damage);
			var skill = skills[currskill];
			skill.cooldownEnd = Time.time + skill.cooldown;
			skills[currskill] = skill;
		


			if (skillNext != -1) {
				currskill = skillNext;
				skillNext = -1;
		
			} else currskill = skill.followupDefaultAttack ? 0 : -1;


			if (targetNext != null) {
				target = targetNext;
				targetNext = null;
			}
		

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

			var skill = skills[currskill];
			Castskill(skill);
		
			skill.cooldownEnd = Time.time + skill.cooldown;
			skills[currskill] = skill;
			if (skillNext != -1) {
				currskill = skillNext;
				skillNext = -1;
			
			} else currskill = skill.followupDefaultAttack ? 0 : -1;


			if (targetNext != null) {
				target = targetNext;
				targetNext = null;
			}


			m_DoState= IDLE;
			return;
		}

	}


	void DEAD()
	{
		deadTime+=Time.deltaTime;
		float gray;

		gray = deadTime * 0.3f;
		if(gray>=1f)gray=1f;
		Mcamera.GetComponent<BW> ().grayScaleAmount = gray;


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
			mNavMesh.Warp (Respawnpoint.transform.position);
			deadTime = 0f;
			hp = hpmax;
			disableInvisible ();
			Mcamera.GetComponent<BW> ().grayScaleAmount = 0;
			m_DoState = IDLE;
			return;
		}

	}


	void END(){
		mNavMesh.enabled = false;
	}

	bool bDied() {
		return hp == 0;
	}


	bool bSkillRequest() {
		return 1 <= currskill && currskill < skills.Count;        
	}


	bool bSkillFinished() {
		return 1 <= currskill && currskill < skills.Count &&
			skills[currskill].CastTimeRemaining() == 0f;        
	}

	bool bATKRequest() {
		return 0 == currskill && currskill < skills.Count;        
	}

	bool bATKFinished() {
		return 0 == currskill && currskill < skills.Count &&
			skills[currskill].IsReady()&&
			skills[currskill].CastTimeRemaining() == 0f; ;        
	}

	bool bTargetDisappeared() {
		return target == null&&skills[currskill].linsskills==null&&skills[currskill].aoeskills==null;
	}

	bool bTargetDied() {
		return target != null && target.hp == 0;
	}


	bool bEnd()
	{
		return EndGame.end == true;
	}



	bool bNavigateTo(){
		return cmdEvents.Remove("NavigateTo");
	}
		
	bool bCancelAction() { 
		return cmdEvents.Remove("CancelAction"); 
	}

	void SetIndicator(){
		if (indicator!=null)
			indicator.transform.position = transform.position;
		if (indicatorArrow != null) {
			indicatorArrow.transform.position = transform.position;
			var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast (ray, out hit,100,1<<15)) {
				indicatorArrow.transform.LookAt(new Vector3(hit.point.x, transform.position.y, hit.point.z)); 
			}


		}
		if (indicatorAoe != null) {
			var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast (ray, out hit,100,1<<15)) {
				Vector3 mpoint = new Vector3 (hit.point.x, 0, hit.point.z);
				indicatorAoe.transform.position = mpoint;
			}
		}

		if (skillcast != currentskillstate) {
		
			if (indicator)
				Destroy (indicator);
			if (indicatorAoe)
				Destroy (indicatorAoe);
			if (indicatorArrow)
				Destroy (indicatorArrow);


		}

	

		if (skillcast != 0 && skillcast != -1) {
			if (!indicator&&skills [skillcast].castRange<500) {
				indicator = Instantiate (Resources.Load ("Indicator/Indicator"), transform.position,Quaternion.identity)as GameObject;
				float Range = skills [skillcast].castRange;
				Range = Range / 5;
				indicator.transform.localScale = new Vector3 (Range, Range, Range);
			}
			if (!indicatorArrow&&skills [skillcast].castRange>500) {
				indicatorArrow = Instantiate (Resources.Load ("Indicator/indicatorArrow"), new Vector3(100,100,100),Quaternion.identity)as GameObject;

			}

			if (!indicatorAoe&&skills [skillcast].aoeRadius>0&&skills [skillcast].aoeskills!=null) {
				indicatorAoe = Instantiate (Resources.Load ("Indicator/Indicator"),new Vector3(100,100,100),Quaternion.identity)as GameObject;
				float Range = skills [skillcast].aoeRadius;
				Range = Range / 5;
				indicatorAoe.transform.localScale = new Vector3 (Range, Range, Range);
			
			}

			currentskillstate = skillcast;

		} else {
			if (indicator)
				Destroy (indicator);
			if (indicatorAoe)
				Destroy (indicatorAoe);
			if (indicatorArrow)
				Destroy (indicatorArrow);


		}
			

	}


	void Update () {
		m_DoState ();
		SetIndicator ();
		if (m_DoState == IDLE  || m_DoState == MOVE ) {
			MouseControl ();
		}else if (m_DoState == CAST) {            
			
			if (target) LookAtY(target.transform.position);

			MouseControl ();

			}


		}




	void LateUpdate() {

		GetComponent<Animator>().SetInteger("HP", hp);
		GetComponent<Animator>().SetFloat("Speed", mNavMesh.velocity.magnitude);
		GetComponent<Animator>().SetInteger("ATTACK",currskill );


	}
}