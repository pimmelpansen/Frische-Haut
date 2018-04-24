float3 addTopCoat(PixelInput input, float3 baseResult) {
	float3 baseNormal = combineNormals(
		SAMPLE_NORMAL_TEX(NormalMap),
		SAMPLE_BUMP_TEX(BumpStrength));
	float3 topCoatNormal = combineNormals(
		baseNormal,
		SAMPLE_BUMP_TEX(TopCoatBump));

	float3 topCoatColor = SAMPLE_COLOR_TEX(TopCoatColor);

	float topCoatWeight = SAMPLE_FLOAT_TEX(TopCoatWeight);

	if (TopCoatColorEffect == 1) {
		float mean = meanValue(topCoatColor);
		float max = maxValue(topCoatColor);
		topCoatWeight *= mean;
		topCoatColor /= max;
	}

	float glossRoughness = SAMPLE_FLOAT_TEX(TopCoatRoughness);
	float3 glossNormalColor = topCoatColor;
	float3 glossGrazingColor = topCoatColor;
	float glossNormalWeight = topCoatWeight;
	float glossGrazingWeight = topCoatWeight;

	if (TopCoatLayeringMode == TopCoatLayeringMode_Reflectivity) {
		glossNormalWeight *= SAMPLE_FLOAT_TEX(TopCoatReflectivity) * 0.08;
	}
	else if (TopCoatLayeringMode == TopCoatLayeringMode_Fresnel) {
		glossNormalWeight *= reflectivityFromIOR(SAMPLE_FLOAT_TEX(TopCoatIOR));
	}
	else if (TopCoatLayeringMode == TopCoatLayeringMode_CustomCurve) {
		glossNormalWeight *= SAMPLE_FLOAT_TEX(TopCoatCurveNormal);
		glossGrazingWeight *= SAMPLE_FLOAT_TEX(TopCoatCurveGrazing);
	}

	float4 topCoatLayer = calculateGenericGlossLayer(
		input, topCoatNormal,
		glossRoughness,
		glossNormalColor, glossGrazingColor,
		glossNormalWeight, glossGrazingWeight
	);
	float3 result = applyLayer(baseResult, topCoatLayer);

	return result;
}
