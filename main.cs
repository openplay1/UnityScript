using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;


public enum Team {Good, Evil, Jungle};


public abstract class main : MonoBehaviour {
	private Shader OriginShader;
	[SerializeField] Vector3 pOffset ;

	public GameObject booldbar;

	public int bushstate;


	public Vector3 skillsetpoint;

	public GameObject Damprefab;
	public float deadTime;
	public int level = 1;
	public int hpmax;
	private int _hp;
	public int hp
	{
		get{return Mathf.Min (_hp, hpmax); }
		set{ _hp = Mathf.Clamp(value, 0, hpmax);}


	}

	public int mp;
	public main target;
	public Team team;


	public int currskill=-1;


	public delegate void DoState ();
	public DoState m_DoState = null;


	public Collider Coll;


	public Skilllist[] Skilllist;
	public List<skill> skills=new List<skill>();



	public void LookAtY(Vector3 pos) {

		transform.LookAt(new Vector3(pos.x, transform.position.y, pos.z));
	}

	public bool CastCheckDistance(skill skillc) {
		
		if(target != null)
		{return  ClosestDistance (Coll, target.Coll) <= skillc.castRange;}
		else  
		{   
			var pos = new Vector3 (transform.position.x, skillsetpoint.y, transform.position.z);
			return  Vector3.Distance (pos, skillsetpoint)<= skillc.castRange;}
	}

	public  float ClosestDistance(Collider a, Collider b) {
		var posA = a.transform.position;
		var posB = b.transform.position;

		return Vector3.Distance(a.ClosestPointOnBounds(posB),
			b.ClosestPointOnBounds(posA));
	}	

	public virtual void Watched(main GO) {
	}

	public void Hide()
	{
		var ren = GetComponentsInChildren<Renderer> ();
		foreach (var t in ren)
		t.enabled=false;
		booldbar.SetActive( false);
   	}

	public void Show()
	{
		var ren = GetComponentsInChildren<Renderer> ();
		foreach (var t in ren)
		t.enabled = true;
		booldbar.SetActive  (true);
	}


	public void Invisible()
	{

		var ren = GetComponentsInChildren<Renderer> ();		
		foreach (var m in ren) {
			m.material.shader = Shader.Find ("Custom/test");
			m.material.SetFloat ("_Alpha", 0.5f);
		}
	}

	public void disableInvisible()
	{

		var ren = GetComponentsInChildren<Renderer> ();		
		foreach (var m in ren)
			m.material.shader = OriginShader;
	}

	public static Dictionary<Team, HashSet<main>> teams = new Dictionary<Team, HashSet<main>>() {
		{Team.Good, new HashSet<main>()},
		{Team.Evil, new HashSet<main>()},
		{Team.Jungle, new HashSet<main>()}
	};


	public void Castskill(skill castskill){
	
		if (castskill.projectile == null && castskill.linsskills == null&&castskill.aoeskills==null) {
			// deal damage directly
			DealDamage (target, castskill.damage);
		} else if (castskill.projectile != null && castskill.linsskills == null&&castskill.aoeskills==null) {

			var pos = transform.position + pOffset;
			var go = (GameObject)Instantiate (castskill.projectile.gameObject, pos, Quaternion.identity);
			var proj = go.GetComponent<Projectile> ();
			proj.target = target;
			proj.caster = this;
			proj.damage = castskill.damage;
			proj.aoeRadius = castskill.aoeRadius;
		
		} else if (castskill.projectile == null && castskill.linsskills != null&&castskill.aoeskills==null) {

			var pos = transform.position ;
			var go = (GameObject)Instantiate (castskill.linsskills.gameObject, pos, transform.rotation);
			var Linesk = go.GetComponent<LineSkills> ();
			Linesk.damage = castskill.damage;
			Linesk.setpoint=skillsetpoint;
			Linesk.caster = this;


		}else if (castskill.projectile == null && castskill.linsskills == null&&castskill.aoeskills!=null) {
			
			var go = (GameObject)Instantiate (castskill.aoeskills.gameObject, skillsetpoint,Quaternion.identity);
			var aoe = go.GetComponent<AoeSkills> ();
			aoe.damage = castskill.damage;
			aoe.aoeRadius = castskill.aoeRadius;
			aoe.setpoint=skillsetpoint;
			aoe.caster = this;


		}

	
	}



	public virtual void DealDamage(main target ,int Damage){

		if ((this is Play||target is Play)&&target.team!=team) {
			GameObject tm = Resources.Load ("DamageMesh") as GameObject;
			tm.GetComponent<TextMesh> ().text = ("" + Damage);	
			Vector3 posup = new Vector3 (target.transform.position.x, target.transform.position.y + 3, target.transform.position.z);
			Instantiate (tm, posup, Quaternion.identity);
		}

		if (target.team!=team)
		target.hp -= Damage;
		
	
	}


	void Awake () {
		OriginShader=GetComponentInChildren<Renderer>().material.shader;
		teams[team].Add(this);
		Coll = GetComponentInChildren<Collider> ();
		foreach (var t in Skilllist)
		skills.Add(new skill(t));
		      


		booldbar = (GameObject)Instantiate(Resources.Load("UI/Bloodall"));
		var blood = booldbar.GetComponentInChildren<hphead> ();
		blood.player = this;
		var bloodcolor = booldbar.GetComponentInChildren<HPGG> ();
		if (team == Team.Evil) {
			bloodcolor.color = Color.red;
			bloodcolor.texture = Resources.Load ("UI/Texture/BloodImage.png")as Texture;
		}


		bushstate = 0;
	
	}
	// Use this for initialization
	void Start () {

	}

	void OnDestroy() {
		teams[team].Remove(this);
		Destroy (booldbar);
	}

	
	// Update is called once per frame
	void Update () {


	




	}
}
