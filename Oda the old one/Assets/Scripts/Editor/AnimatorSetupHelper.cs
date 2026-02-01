using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class AnimatorSetupHelper : EditorWindow
{
    [MenuItem("Tools/Setup Jump Animations")]
    private static void SetupJumpAnimations()
    {
        string controllerPath = "Assets/Tyb ASSETS/Samurai/new samurai$/Samurai Control.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        
        if (controller == null)
        {
            Debug.LogError("Animator Controller not found at: " + controllerPath);
            return;
        }

        AnimationClip jumpClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Tyb ASSETS/Samurai/new samurai$/Jump.anim");
        AnimationClip fallClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Tyb ASSETS/Samurai/new samurai$/Fall.anim");
        AnimationClip hitGroundClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Tyb ASSETS/Samurai/new samurai$/Hit Ground.anim");

        if (jumpClip == null || fallClip == null || hitGroundClip == null)
        {
            Debug.LogError("Animation clips not found! Please check the paths.");
            return;
        }

        AnimatorControllerLayer baseLayer = controller.layers[0];
        AnimatorStateMachine stateMachine = baseLayer.stateMachine;

        AddParameterIfNotExists(controller, "isJumping", AnimatorControllerParameterType.Bool);
        AddParameterIfNotExists(controller, "isFalling", AnimatorControllerParameterType.Bool);
        AddParameterIfNotExists(controller, "isGrounded", AnimatorControllerParameterType.Bool);

        AnimatorState jumpState = FindOrCreateState(stateMachine, "Jump", jumpClip);
        AnimatorState fallState = FindOrCreateState(stateMachine, "Fall", fallClip);
        AnimatorState hitGroundState = FindOrCreateState(stateMachine, "Hit Ground", hitGroundClip);

        hitGroundState.speed = 1.5f;

        AnimatorState idleState = FindStateByName(stateMachine, "Iddle");
        AnimatorState walkState = FindStateByName(stateMachine, "Walk");

        if (idleState != null)
        {
            CreateTransitionIfNotExists(idleState, jumpState, "isJumping", true, false, 0.1f);
        }

        if (walkState != null)
        {
            CreateTransitionIfNotExists(walkState, jumpState, "isJumping", true, false, 0.1f);
        }

        CreateTransitionIfNotExists(jumpState, fallState, "isFalling", true, false, 0.1f);
        CreateTransitionIfNotExists(fallState, hitGroundState, "isGrounded", true, false, 0.05f);

        if (idleState != null)
        {
            CreateTransitionWithExitTime(hitGroundState, idleState, 0.9f, 0.1f);
            
            var jumpToIdle = CreateTransitionIfNotExists(jumpState, idleState, null, false, false, 0.15f);
            if (jumpToIdle != null && jumpToIdle.conditions.Length < 2)
            {
                jumpToIdle.AddCondition(AnimatorConditionMode.If, 0, "isGrounded");
                jumpToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isFalling");
            }
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        
        Debug.Log("✅ Animator setup complete! Jump, Fall, and Hit Ground animations are now connected.");
    }

    private static void AddParameterIfNotExists(AnimatorController controller, string paramName, AnimatorControllerParameterType type)
    {
        foreach (var param in controller.parameters)
        {
            if (param.name == paramName) return;
        }
        controller.AddParameter(paramName, type);
        Debug.Log($"Added parameter: {paramName}");
    }

    private static AnimatorState FindOrCreateState(AnimatorStateMachine stateMachine, string stateName, AnimationClip clip)
    {
        foreach (var childState in stateMachine.states)
        {
            if (childState.state.name == stateName)
            {
                childState.state.motion = clip;
                return childState.state;
            }
        }

        AnimatorState newState = stateMachine.AddState(stateName);
        newState.motion = clip;
        Debug.Log($"Created state: {stateName}");
        return newState;
    }

    private static AnimatorState FindStateByName(AnimatorStateMachine stateMachine, string stateName)
    {
        foreach (var childState in stateMachine.states)
        {
            if (childState.state.name == stateName)
            {
                return childState.state;
            }
        }
        return null;
    }

    private static AnimatorStateTransition CreateTransitionIfNotExists(AnimatorState from, AnimatorState to, string conditionParam, bool conditionValue, bool hasExitTime, float duration)
    {
        foreach (var transition in from.transitions)
        {
            if (transition.destinationState == to)
            {
                return transition;
            }
        }

        AnimatorStateTransition newTransition = from.AddTransition(to);
        newTransition.hasExitTime = hasExitTime;
        newTransition.exitTime = 0.9f;
        newTransition.duration = duration;
        newTransition.hasFixedDuration = true;

        if (!string.IsNullOrEmpty(conditionParam))
        {
            newTransition.AddCondition(conditionValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, conditionParam);
        }

        Debug.Log($"Created transition: {from.name} → {to.name}");
        return newTransition;
    }

    private static void CreateTransitionWithExitTime(AnimatorState from, AnimatorState to, float exitTime, float duration)
    {
        foreach (var transition in from.transitions)
        {
            if (transition.destinationState == to && transition.hasExitTime)
            {
                return;
            }
        }

        AnimatorStateTransition newTransition = from.AddTransition(to);
        newTransition.hasExitTime = true;
        newTransition.exitTime = exitTime;
        newTransition.duration = duration;
        newTransition.hasFixedDuration = true;

        Debug.Log($"Created exit time transition: {from.name} → {to.name}");
    }
}
