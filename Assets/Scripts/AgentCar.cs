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
    private static int cntGoal=0;
    private const int CarNum = 5;
    private float mindist=0;
    private float time=0;
    private const float ylocate = 0.08f;
    private const float rayDistance = 3.0f;
    private float[] rayAngles; //前方への赤外線
    //yamlのカリキュラム内容を取ってくるやつ
    FloatPropertiesChannel curriculum;
 　 
  
    // スタート時に呼ばれる
    public override void Initialize()
    {
	curriculum = SideChannelUtils.GetSideChannel<FloatPropertiesChannel>();
    }

    // エピソード開始時に呼ばれる
    public override void OnEpisodeBegin()
    {
        // RollerAgentの落下時
        //if (this.transform.position.y < -0.5f)
        //{
            // RollerAgentの位置と速度をリセット
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
	this.transform.rotation = Quaternion.Euler(0,0,0);
        this.transform.position = new Vector3(
	    Random.value * 4.8f - 2.4f, ylocate, Random.value * 4.8f - 2.4f);
        //}

        // Targetの位置のリセット
        target.position = new Vector3(
            Random.value * 4.8f - 2.4f, ylocate, Random.value * 4.8f - 2.4f);
    }
// 状態取得時に呼ばれる
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target.position); //Targetの位置XYZ
	sensor.AddObservation(rBody.angularVelocity.y); //向きのみの学習
	
   }

// 行動実行時に呼ばれる
    public override void OnActionReceived(float[] vectorAction)
    {
	// 時間経過
	time +=1;
        // RollerAgentに力を加える
	//車の動き
	vectorAction[0] = vectorAction[0] - 1; // 0~2を -1~1に変更
	this.gameObject.transform.Rotate(vectorAction[0] * 0.8f,0, 0);
	rBody.velocity = this.gameObject.transform.localRotation * new Vector3(0f, -1.0f, 0.7f); //前向きの速度は一定0.6f
	/* つかってたやつ(ボールの動き)
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];
        rBody.AddForce(controlSignal * 60);
	*/
        // RollerAgentがTargetの位置に到着
        float distanceToTarget = Vector3.Distance(
            this.transform.position, target.position);
    
        if (distanceToTarget < 0.14f)
        {
	    time = 0; //目的地についたのでリセット
            AddReward(3.0f);
	    cntGoal+=1;
	    if(cntGoal>9) { EndEpisode(); }
	    target.position = new Vector3(
            Random.value * 4.8f - 2.4f, ylocate, Random.value * 4.8f - 2.4f);
	    mindist = Vector3.Distance(
            this.transform.position, target.position);
        }else if(mindist > distanceToTarget){ //近づくと点数アップ
	    AddReward(mindist-distanceToTarget);
            mindist = distanceToTarget;
	}
	if(time%1000==0){
	    AddReward(-time/100000.0f);
	}
	
        // RollerAgentが落下
        if (this.transform.position.y < -0.5f)
        {
            AddReward(-1.0f);
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
	      AddReward(-1.0f);
	      Debug.Log(t.gameObject.tag);
	      this.rBody.angularVelocity = Vector3.zero;
              this.rBody.velocity = Vector3.zero;
	      this.transform.rotation = Quaternion.Euler(0,0,0);
              this.transform.position = new Vector3(
	       Random.value * 4.8f - 2.4f, ylocate, Random.value * 4.8f - 2.4f);    	
	 }
	 //Debug.Log(t.gameObject.tag);
    }

}
