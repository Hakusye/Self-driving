using System.Collections.Generic;
using static System.Console;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;
using MLAgents.SideChannels; //カリキュラム学習を使うときの宣言

/********** すること ***********
一定の条件で
cirriculumの内容を変更していく
例:
0:移動、当たらないだけで偉い!
1:目的地についたら更に報酬あげちゃう
2:遅いと罰金
みたいな。確率による変化はなしでいいかも
(報酬はかえずに車の台数だけを変化させるのも大いにあり)
*******************************/

public class AgentCar : Agent
{
    public Transform target;
    public Rigidbody rBody;
    public SettingReward SettingRewards;
    private static int cntGoal=0;
    private const int CarNum = 5;
    private float mindist=0;
    private float time=0;
    private const float ylocate = 0.08f;
    private const float rayDistance = 3.0f;
    private float[] rayAngles; //前方への赤外線
    private int curic;
　  private float current_speed;
    
    //yamlのカリキュラム内容を取ってくるやつ
    FloatPropertiesChannel curriculum;
 　 
  
    // スタート時に呼ばれる
    public override void Initialize()
    {
	SettingRewards = new SettingReward();
	curic = 0; //ぶつからないだけでRewardのパターン
	curriculum = SideChannelUtils.GetSideChannel<FloatPropertiesChannel>();
	current_speed = 0.3f;
    }

    // エピソード開始時に呼ばれる
    public override void OnEpisodeBegin()
    {
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
	this.transform.rotation = Quaternion.Euler(0,0,0);
        this.transform.position = new Vector3(
	    Random.value * 4.8f - 2.4f, ylocate, Random.value * 4.8f - 2.4f);

        // Targetの位置のリセット
        target.position = new Vector3(
            Random.value * 4.8f - 2.4f, ylocate, Random.value * 4.8f - 2.4f);
    }
// 状態取得時に呼ばれる
    public override void CollectObservations(VectorSensor sensor)
    {
	sensor.AddObservation(this.transform.position); //現在地
        sensor.AddObservation(target.position); //Targetの位置XYZ
	sensor.AddObservation(rBody.angularVelocity); //車のベクトル保持
	sensor.AddObservation(current_speed); // 現在の速度	
   }

// 行動実行時に呼ばれる
    public override void OnActionReceived(float[] act)
    {
	// 時間経過
	time +=1;
        // RollerAgentに力を加える
	//車の動き
	this.gameObject.transform.Rotate(Mathf.Clamp(act[0], -1, 1) * 0.6f,0, 0);
	//vectorAction[1]で速度調整したい
	current_speed += Mathf.Clamp(act[1],-0.1f,0.3f); //ブレーキアクセルを再現
	if(current_speed < 0.5f) current_speed = 0.5f;
	if(current_speed > 3.0f) current_speed = 3.0f;
	rBody.velocity = this.gameObject.transform.localRotation * new Vector3(0f, -3.5f,current_speed);
        // RollerAgentがTargetの位置に到着
        float distanceToTarget = Vector3.Distance(
            this.transform.position, target.position);
        
	AddReward(SettingRewards.dist_reward(curic,mindist,distanceToTarget,current_speed));
	AddReward(SettingRewards.time_reward(curic,time));            
	if(mindist > distanceToTarget) mindist = distanceToTarget;

        if (distanceToTarget < 0.14f && curic > 0)
        {
	    time = 0; //目的地についたのでリセット
	    cntGoal+=1;
	    if(cntGoal>9) { EndEpisode(); }
	    target.position = new Vector3(
            Random.value * 4.8f - 2.4f, ylocate, Random.value * 4.8f - 2.4f);
	    mindist = Vector3.Distance(
            this.transform.position, target.position);
        }
	
        // RollerAgentが落下
        if (this.transform.position.y < -0.5f)
        {
            AddReward(SettingRewards.collision_reward(curic));
	    this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
	    this.transform.rotation = Quaternion.Euler(0,0,0);
            this.transform.position = new Vector3(
		 Random.value * 4.8f - 2.4f, ylocate, Random.value * 4.8f - 2.4f);  
        }
    }

    // 衝突判定
    void OnCollisionEnter(Collision t)
　　{
         if(t.gameObject.tag=="car" ||　t.gameObject.tag=="wall"){
	      AddReward(SettingRewards.collision_reward(curic));
	      //Debug.Log(t.gameObject.tag);
	      this.rBody.angularVelocity = Vector3.zero;
              this.rBody.velocity = Vector3.zero;
	      this.transform.rotation = Quaternion.Euler(0,0,0);
              this.transform.position = new Vector3(
	       Random.value * 4.8f - 2.4f, ylocate, Random.value * 4.8f - 2.4f);    	
	 }
    }
    //時間で定期的に呼ばれる。詳しいことはわかんない
    void FixedUpdate()
    {
	curic = (int)curriculum.GetPropertyWithDefault("ChallengeComplexity",0); //右の数字なんもわかってない
    }

}
