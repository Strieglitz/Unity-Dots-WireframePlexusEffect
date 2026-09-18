using UnityEngine;

namespace WireframePlexusShader {

    /// <summary>Same order as EaseType of the ECS version, so the two enums convert by their int value.</summary>
    public enum PlexusEase {
        Linear,
        EaseInQuad, EaseOutQuad, EaseInOutQuad,
        EaseInCubic, EaseOutCubic, EaseInOutCubic,
        EaseInQuart, EaseOutQuart, EaseInOutQuart,
        EaseInQuint, EaseOutQuint, EaseInOutQuint,
        EaseInSine, EaseOutSine, EaseInOutSine,
        EaseInExpo, EaseOutExpo, EaseInOutExpo,
        EaseInCirc, EaseOutCirc, EaseInOutCirc,
        EaseInElastic, EaseOutElastic, EaseInOutElastic,
        EaseInBack, EaseOutBack, EaseInOutBack,
        EaseInBounce, EaseOutBounce, EaseInOutBounce
    }

    public static class PlexusEasing {

        const float BackC1 = 1.70158f;
        const float BackC2 = BackC1 * 1.525f;
        const float BackC3 = BackC1 + 1f;
        const float ElasticC4 = 2f * Mathf.PI / 3f;
        const float ElasticC5 = 2f * Mathf.PI / 4.5f;

        /// <summary>Maps t in 0..1 through the given easing curve.</summary>
        public static float Evaluate(PlexusEase ease, float t) {
            switch (ease) {
                case PlexusEase.EaseInQuad: return t * t;
                case PlexusEase.EaseOutQuad: return t * (2f - t);
                case PlexusEase.EaseInOutQuad: return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;

                case PlexusEase.EaseInCubic: return t * t * t;
                case PlexusEase.EaseOutCubic: return 1f + Pow(t - 1f, 3);
                case PlexusEase.EaseInOutCubic: return t < 0.5f ? 4f * t * t * t : 1f + Pow(2f * t - 2f, 3) * 0.5f;

                case PlexusEase.EaseInQuart: return t * t * t * t;
                case PlexusEase.EaseOutQuart: return 1f - Pow(t - 1f, 4);
                case PlexusEase.EaseInOutQuart: return t < 0.5f ? 8f * t * t * t * t : 1f - 8f * Pow(t - 1f, 4);

                case PlexusEase.EaseInQuint: return t * t * t * t * t;
                case PlexusEase.EaseOutQuint: return 1f + Pow(t - 1f, 5);
                case PlexusEase.EaseInOutQuint: return t < 0.5f ? 16f * t * t * t * t * t : 1f + 16f * Pow(t - 1f, 5);

                case PlexusEase.EaseInSine: return 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
                case PlexusEase.EaseOutSine: return Mathf.Sin(t * Mathf.PI * 0.5f);
                case PlexusEase.EaseInOutSine: return -0.5f * (Mathf.Cos(Mathf.PI * t) - 1f);

                case PlexusEase.EaseInExpo: return t <= 0f ? 0f : Mathf.Pow(2f, 10f * t - 10f);
                case PlexusEase.EaseOutExpo: return t >= 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
                case PlexusEase.EaseInOutExpo:
                    if (t <= 0f) return 0f;
                    if (t >= 1f) return 1f;
                    return t < 0.5f ? Mathf.Pow(2f, 20f * t - 10f) * 0.5f : (2f - Mathf.Pow(2f, -20f * t + 10f)) * 0.5f;

                case PlexusEase.EaseInCirc: return 1f - Mathf.Sqrt(Mathf.Max(0f, 1f - t * t));
                case PlexusEase.EaseOutCirc: return Mathf.Sqrt(Mathf.Max(0f, 1f - (t - 1f) * (t - 1f)));
                case PlexusEase.EaseInOutCirc:
                    return t < 0.5f
                        ? (1f - Mathf.Sqrt(Mathf.Max(0f, 1f - 4f * t * t))) * 0.5f
                        : (Mathf.Sqrt(Mathf.Max(0f, 1f - (-2f * t + 2f) * (-2f * t + 2f))) + 1f) * 0.5f;

                case PlexusEase.EaseInElastic:
                    if (t <= 0f) return 0f;
                    if (t >= 1f) return 1f;
                    return -Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((t * 10f - 10.75f) * ElasticC4);
                case PlexusEase.EaseOutElastic:
                    if (t <= 0f) return 0f;
                    if (t >= 1f) return 1f;
                    return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * ElasticC4) + 1f;
                case PlexusEase.EaseInOutElastic:
                    if (t <= 0f) return 0f;
                    if (t >= 1f) return 1f;
                    return t < 0.5f
                        ? -(Mathf.Pow(2f, 20f * t - 10f) * Mathf.Sin((20f * t - 11.125f) * ElasticC5)) * 0.5f
                        : Mathf.Pow(2f, -20f * t + 10f) * Mathf.Sin((20f * t - 11.125f) * ElasticC5) * 0.5f + 1f;

                case PlexusEase.EaseInBack: return BackC3 * t * t * t - BackC1 * t * t;
                case PlexusEase.EaseOutBack: return 1f + BackC3 * Pow(t - 1f, 3) + BackC1 * Pow(t - 1f, 2);
                case PlexusEase.EaseInOutBack:
                    return t < 0.5f
                        ? Pow(2f * t, 2) * ((BackC2 + 1f) * 2f * t - BackC2) * 0.5f
                        : (Pow(2f * t - 2f, 2) * ((BackC2 + 1f) * (t * 2f - 2f) + BackC2) + 2f) * 0.5f;

                case PlexusEase.EaseInBounce: return 1f - BounceOut(1f - t);
                case PlexusEase.EaseOutBounce: return BounceOut(t);
                case PlexusEase.EaseInOutBounce:
                    return t < 0.5f ? (1f - BounceOut(1f - 2f * t)) * 0.5f : (1f + BounceOut(2f * t - 1f)) * 0.5f;

                default: return t;
            }
        }

        static float Pow(float value, int exponent) {
            float result = 1f;
            for (int i = 0; i < exponent; i++) result *= value;
            return result;
        }

        static float BounceOut(float t) {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;
            if (t < 1f / d1) return n1 * t * t;
            if (t < 2f / d1) { t -= 1.5f / d1; return n1 * t * t + 0.75f; }
            if (t < 2.5f / d1) { t -= 2.25f / d1; return n1 * t * t + 0.9375f; }
            t -= 2.625f / d1;
            return n1 * t * t + 0.984375f;
        }
    }
}
