using System.Collections.Generic;
using static System.Console;
using UnityEngine;
using MLAgents;
using MLAgents.Sensors;
using MLAgents.SideChannels; //カリキュラム学習を使うときの宣言

public class SettingReward
{ 
    public float dist_reward(int curic,float min_dist,float dist2target,float speed) //距離に関するリワード設定
    {
	float sum = 0.0f;
	if(curic == 0)
	{
		sum += speed/8000.0f;
	}
	if(curic > 0)
	{
	    if(min_dist > dist2target) sum += (min_dist - dist2target); //近づいたらrewardに変更
	    if(dist2target < 0.14f) sum += 3.0f; //Reward以外の処理はAgentの方で記載
	}
	return sum;
    }

    public float time_reward(int curic,float time){
	float sum=0.0f;
/***	
	if(curic == 0){
	     sum += 1.0f/6000.0f;
	}
	if(curic == 1){
	    //sum += 1.0f/100000.0f;  //時間による変化は設けない(ギリギリまで目的地にいかないのが正解になるため)
	}
***/
	if(curic == 2){
	    if(time%1000 == 0) sum -= time/100000.0f; //着くまでが遅いとマイナス
	}
	return sum;
    }
    public float collision_reward(int curic){
	return -1.0f; //一旦統一で-1にしておく
    }
}
