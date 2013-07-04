///////////////////////////////////////////////////////////////////////////////
///
/// MsgReceiver
/// 
/// A MsgReceiver solves a stupid problem with Unity's SendMessage. Unity will
/// only SendMessage to *my* custom script methods and not its own! Therefore,
/// if we want to activate a default Unity method, we need to go through this
/// MsgReceiver class. :-(
/// 
///////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[System.Serializable] // Required so it shows up in the inspector 
[LitJson.ExportType(LitJson.ExportType.NoExport)]	//	use this to prevent specific fields from being exported by LitJson library
[AddComponentMenu ("FSM/MsgReceiver")]
public class MsgReceiver : MonoBehaviour
{
	public bool				m_bDebugThis = false;
	public List<string>		m_LastMessageList = new List<string>();
	
	public void AddDebugMessage(string s)
	{
		if (m_bDebugThis) {
			m_LastMessageList.Add(s);
		}
	}
	
	public void EnableDebugAndTrace()
	{
		if (m_bDebugThis) {
			Rlplog.TrcFlag = true;
			Rlplog.DbgFlag = true;
		}
	}
	
	public void DisableDebugAndTrace()
	{
		/*
		if (m_bDebugThis) {
			DebugOptions options = GameObject.FindObjectOfType(typeof(DebugOptions)) as DebugOptions;
			if (options != null) {
				Rlplog.TrcFlag = options.m_bTrace;
				Rlplog.DbgFlag = options.m_bDebug;
			}
		}
		*/
	}
	
	public void TestReceiveMsg()
	{
		EnableDebugAndTrace();
		string msg = Rlplog.Debug("MsgReceiver.TestReceiveMsg", "Test Message Received!");
		AddDebugMessage(msg);
		DisableDebugAndTrace();
	}
	
	public void DestroyThis()
	{
		EnableDebugAndTrace();
		string msg = Rlplog.Debug("MsgReceiver.DestroyThis", this.name);
		AddDebugMessage(msg);
		DestroyObject(this.gameObject);
		DisableDebugAndTrace();
	}
	
	public void AnimationPlay(string[] args)
	{
		EnableDebugAndTrace();
		string animName = args[0];
		string msg = Rlplog.Debug("MsgReceiver.AnimationPlay", this.name + ", " + animName);
		AddDebugMessage(msg);
		
		if (this.gameObject.animation)
			this.gameObject.animation.Play(animName);
		DisableDebugAndTrace();
	}
	
	//	play a PlayMaker event
	public void PlayMakerEvent(string[] args)
	{
		EnableDebugAndTrace();
		string msg = Rlplog.Debug("MsgReceiver.PlayMakerEvent", this.name);
		AddDebugMessage(msg);
#if _HutongGamesPlayMaker

		PlayMakerFSM playmaker = this.GetComponent<PlayMakerFSM>();
		if (playmaker != null) {
			playmaker.Fsm.Event(args[0]);
		}
#endif //	#if _HutongGamesPlayMaker

		DisableDebugAndTrace();
	}
	
	//	renderer.enabled = false
	public void Hide()
	{
		EnableDebugAndTrace();
		string msg = Rlplog.Debug("MsgReceiver.Hide", this.name);
		AddDebugMessage(msg);
		CustomExtensions.GameObjectExtension.Hide(this.gameObject, null);
		DisableDebugAndTrace();
	}
	
	//	renderer.enabled = true
	public void Unhide()
	{
		EnableDebugAndTrace();
		string msg = Rlplog.Debug("MsgReceiver.Unhide", this.name);
		AddDebugMessage(msg);
		CustomExtensions.GameObjectExtension.Unhide(this.gameObject, null);
		DisableDebugAndTrace();
	}
	
	//	gameobject.active = true
	public void Activate(string[] componentNames)
	{
		EnableDebugAndTrace();
		string msg = Rlplog.Trace("MsgReceiver.Activate", this.name);
		AddDebugMessage(msg);
		CustomExtensions.GameObjectExtension.Activate(this.gameObject, componentNames);
		DisableDebugAndTrace();
	}
	

	public void ActivateRecursively(string[] componentNames)
	{
		EnableDebugAndTrace();
		string msg = Rlplog.Trace("MsgReceiver.ActivateRecursively", this.name);
		AddDebugMessage(msg);
		CustomExtensions.GameObjectExtension.ActivateRecursively(this.gameObject, componentNames);
		DisableDebugAndTrace();
	}
	
	public void Deactivate(string[] componentNames)
	{
		EnableDebugAndTrace();
		string msg = Rlplog.Trace("MsgReceiver.Deactivate", this.name);
		AddDebugMessage(msg);
		CustomExtensions.GameObjectExtension.Deactivate(this.gameObject, componentNames);
		DisableDebugAndTrace();
	}
	

	public void DeactivateRecursively(string[] componentNames)
	{
		EnableDebugAndTrace();
		string msg = Rlplog.Trace("MsgReceiver.DeactivateRecursively", this.name);
		AddDebugMessage(msg);
		CustomExtensions.GameObjectExtension.DeactivateRecursively(this.gameObject, componentNames);
		DisableDebugAndTrace();
	}
	
	public void AnimationEventTrigger(float f)
	{
		EnableDebugAndTrace();
		string msg = Rlplog.Trace("MsgReceiver.AnimationEventTrigger", f.ToString());
		AddDebugMessage(msg);
		DisableDebugAndTrace();
	}
	
	//	for animation FSMs to enter the next state. Usually this is for transition states
	public void EnterNextState(Object FSobject)
	{
		EnableDebugAndTrace();
		string msg = Rlplog.Trace("EnterNextState", FSobject.name);
		AddDebugMessage(msg);
		if (FSobject == null) {
			Rlplog.Error("EnterNextState", "EnterNextState has null FiniteState. Can't determine which state is next if I don't know this state.");
			return;
		}
		GameObject stateGO = FSobject as GameObject;
		FiniteState state = stateGO.GetComponent<FiniteState>();
		if (state) {
			state.GotoNextState();
		}
		DisableDebugAndTrace();
	}
	
	public void EnableCamera()
	{
		EnableDebugAndTrace();
		string msg = Rlplog.Trace("MsgReceiver.EnableCamera", this.name);
		AddDebugMessage(msg);
		Camera cam = this.GetComponent<Camera>();
		if (cam != null) {
			cam.enabled = true;
		}
		DisableDebugAndTrace();
	}
	
	public void DisableCamera()
	{
		EnableDebugAndTrace();
		string msg = Rlplog.Trace("MsgReceiver.DisableCamera", this.name);
		AddDebugMessage(msg);
		Camera cam = this.GetComponent<Camera>();
		if (cam != null) {
			cam.enabled = false;
		}
		DisableDebugAndTrace();
	}

	/*
	 * You must create an EndOfAnimation AnimationEvent in the AnimationClip to receive this trigger from the Unity system.
	 */
	public void EndOfAnimationTrigger(string animClipName)
	{
		EnableDebugAndTrace();
		string msg = Rlplog.Trace("MsgReceiver.EndOfAnimationTrigger", this.name + ", " + animClipName);
		AddDebugMessage(msg);
		//	tell my subscribers
		Publisher publisher = this.GetComponent<Publisher>();
		if (publisher)
			publisher.SendSubscriberMessage("EndOfAnimationTrigger", this.name, animClipName);
		DisableDebugAndTrace();
	}
	
	public void EndOfAnimationTriggerObjArg(UnityEngine.Object obj)
	{
		EnableDebugAndTrace();
		string msg = Rlplog.Trace("MsgReceiver.EndOfAnimationTrigger", this.name + ", " + obj.name);
		AddDebugMessage(msg);
		EndOfAnimationTrigger(obj.name);
		DisableDebugAndTrace();
	}
	
	public void PlayBackwards(string[] parameters)
	{
		if (parameters.Length==0) {
			Rlplog.Error("MsgReceiver.PlayBackwards", this.name + ": No parameters passed. Expecting name of animation to PlayBackwards.");
		}
		else {
			Rlplog.Trace("MsgReceiver.PlayBackwards", this.name + ": PlayBackwards " + parameters[0]);
			if (this.animation) {
				if (!this.animation.GetClip(parameters[0])) {
					//this.animation.AddClip(parameters[0], parameters[0]);
					Debug.LogWarning(this.name + " does not have Animation Clip " + parameters[0] + " attached.");
				}
	
				foreach(AnimationState state in this.animation)
				{
					if (state.name == parameters[0]) {
						state.speed = -1.0f;
						state.time = state.length * 0.99f;
						state.wrapMode = WrapMode.Once;
					}
				}
				this.animation.Play(parameters[0]);
			}
		}
	}
	
	public void Play(string[] parameters)
	{
		if (parameters.Length==0) {
			Rlplog.Error("MsgReceiver.Play", this.name + ": No parameters passed. Expecting name of animation to Play.");
		}
		else {
			Rlplog.Trace("MsgReceiver.Play", this.name + ": Play " + parameters[0]);
			if (this.animation) {
				if (!this.animation.GetClip(parameters[0])) {
					//this.animation.AddClip(parameters[0], parameters[0]);
					Debug.LogWarning(this.name + " does not have Animation Clip " + parameters[0] + " attached.");
				}
				this.animation.Play(parameters[0]);
			}
		}
	}
	public void Stop(string[] parameters)
	{
		if (parameters.Length==0) {
			Rlplog.Error("MsgReceiver.Stop", this.name+": No parameters passed. Expecting name of animation to Stop.");
		}
		else {
			Rlplog.Trace("MsgReceiver.Stop", this.name + ": Stop " + parameters[0]);
			if (this.animation) {
				this.animation.Stop(parameters[0]);
			}
		}
	}
	public void CrossFade(string[] parameters)
	{
		if (parameters.Length==0) {
			Rlplog.Error("MsgReceiver.CrossFade", this.name+": No parameters passed. Expecting name of animation to CrossFade.");
		}
		else {
			Rlplog.Trace("MsgReceiver.CrossFade", this.name + ": CrossFade " + parameters[0]);
			if (this.animation) {
				this.animation.CrossFade(parameters[0]);
			}
		}
	}
	public void Rewind(string[] parameters)
	{
		if (parameters.Length==0) {
			Rlplog.Error("MsgReceiver.Rewind", this.name+": No parameters passed. Expecting name of animation to Rewind.");
		}
		else {
			Rlplog.Trace("MsgReceiver.Rewind", this.name + ": Rewind "  + parameters[0]);
			if (this.animation) {
				if (!this.animation.GetClip(parameters[0])) {
					//this.animation.AddClip(parameters[0], parameters[0]);
					Debug.LogWarning(this.name + " does not have Animation Clip " + parameters[0] + " attached.");
				}
				this.animation.Rewind(parameters[0]);
				this.animation[parameters[0]].time = 0.0f;
				this.animation.Sample();
				this.animation.Stop(parameters[0]);
			}
		}
	}
	
	//	AudioSource methods
	public void AudioPlay(string[] parameters)
	{
		//EnableDebugAndTrace();
		string msg = Rlplog.Trace("MsgReceiver.AudioPlay", this.name);
		//AddDebugMessage(msg);
		if (this.audio != null) {
			this.audio.Play();
		}
		//DisableDebugAndTrace();
	}
}