﻿using SharpDX;
using Valve.VR;
using static MathExtensions;

public class EyeLookAtAnimator : IProceduralAnimator {
	private static readonly float RotationAngleRejectionThreshold = MathUtil.DegreesToRadians(80);
	
	private readonly ChannelSystem channelSystem;
	private readonly BoneSystem boneSystem;
	
	private readonly Bone leftEyeBone;
	private readonly Bone rightEyeBone;
	
	private readonly LaggedVector3Forecaster headPositionForecaster = new LaggedVector3Forecaster(0.08f);

	public EyeLookAtAnimator(ChannelSystem channelSystem, BoneSystem boneSystem) {
		this.channelSystem = channelSystem;
		this.boneSystem = boneSystem;

		leftEyeBone = boneSystem.BonesByName["lEye"];
		rightEyeBone = boneSystem.BonesByName["rEye"];
	}
	
	private void UpdateEye(ChannelOutputs outputs, StagedSkinningTransform[] boneTotalTransforms, ChannelInputs inputs, Bone eyeBone, Vector3 targetPosition) {
		var eyeParentTotalTransform = boneTotalTransforms[eyeBone.Parent.Index];
		Vector3 targetPositionInRotationFreeEyeSpace = eyeParentTotalTransform.InverseTransform(targetPosition * 100) - eyeBone.CenterPoint.GetValue(outputs);

		var targetRotation = QuaternionExtensions.RotateBetween(Vector3.BackwardRH, targetPositionInRotationFreeEyeSpace);
		targetRotation = Quaternion.RotationAxis(
			targetRotation.Axis,
			TukeysBiweight(targetRotation.Angle, RotationAngleRejectionThreshold));

		eyeBone.SetEffectiveRotation(inputs, outputs, targetRotation);
	}

	public void Update(ChannelInputs inputs, float time) {
		headPositionForecaster.Update(PlayerPositionUtils.GetHeadGamePosition());
		var forecastHeadPosition = headPositionForecaster.ForecastValue;

		var outputs = channelSystem.Evaluate(null, inputs);
		var boneTotalTransforms = boneSystem.GetBoneTransforms(outputs);
		
		UpdateEye(outputs, boneTotalTransforms, inputs, leftEyeBone, forecastHeadPosition);
		UpdateEye(outputs, boneTotalTransforms, inputs, rightEyeBone, forecastHeadPosition);
	}
}
