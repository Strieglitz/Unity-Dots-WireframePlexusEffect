

using Unity.Mathematics;
namespace WireframePlexus {
    public class EasingFunctions {
        public static float GetEaseValue(EaseType tweenType, float t) {
            switch (tweenType) {
                case EaseType.EaseInQuad:
                    return EaseInQuad(t);
                case EaseType.EaseOutQuad:
                    return EaseOutQuad(t);
                case EaseType.EaseInOutQuad:
                    return EaseInOutQuad(t);
                case EaseType.EaseInCubic:
                    return EaseInCubic(t);
                case EaseType.EaseOutCubic:
                    return EaseOutCubic(t);
                case EaseType.EaseInOutCubic:
                    return EaseInOutCubic(t);
                case EaseType.EaseInQuart:
                    return EaseInQuart(t);
                case EaseType.EaseOutQuart:
                    return EaseOutQuart(t);
                case EaseType.EaseInOutQuart:
                    return EaseInOutQuart(t);
                case EaseType.EaseInQuint:
                    return EaseInQuint(t);
                case EaseType.EaseOutQuint:
                    return EaseOutQuint(t);
                case EaseType.EaseInOutQuint:
                    return EaseInOutQuint(t);
                case EaseType.EaseInSine:
                    return EaseInSine(t);
                case EaseType.EaseOutSine:
                    return EaseOutSine(t);
                case EaseType.EaseInOutSine:
                    return EaseInOutSine(t);
                case EaseType.EaseInExpo:
                    return EaseInExpo(t);
                case EaseType.EaseOutExpo:
                    return EaseOutExpo(t);
                case EaseType.EaseInOutExpo:
                    return EaseInOutExpo(t);
                case EaseType.EaseInCirc:
                    return EaseInCirc(t);
                case EaseType.EaseOutCirc:
                    return EaseOutCirc(t);
                case EaseType.EaseInOutCirc:
                    return EaseInOutCirc(t);
                case EaseType.EaseInElastic:
                    return EaseInElastic(t);
                case EaseType.EaseOutElastic:
                    return EaseOutElastic(t);
                case EaseType.EaseInOutElastic:
                    return EaseInOutElastic(t);
                case EaseType.EaseInBack:
                    return EaseInBack(t);
                case EaseType.EaseOutBack:
                    return EaseOutBack(t);
                case EaseType.EaseInOutBack:
                    return EaseInOutBack(t);
                default:
                    return Linear(t);
            }
        }


        public static float EaseInQuint(float t) {
            return t * t * t * t * t;
        }
        public static float EaseInOutQuart(float t) {
            return t < 0.5f ? 8 * t * t * t * t : 1 - 8 * (--t) * t * t * t;
        }
        public static float EaseOutQuart(float t) {
            return 1 - (--t) * t * t * t;
        }
        public static float EaseInOutCubic(float t) {
            return t < 0.5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
        }
        public static float EaseInOutElastic(float t) {
            if (t == 0) return 0;
            if (t == 1) return 1;
            if ((t /= 0.5f) == 2) return 1;
            return t < 1 ? -0.5f * math.pow(2, 10 * (t - 1)) * math.sin((t - 1.1f) * (2 * math.PI) / 4) : 0.5f * math.pow(2, -10 * (t - 1)) * math.sin((t - 1.1f) * (2 * math.PI) / 4) + 1;
        }
        public static float EaseOutElastic(float t) {
            if (t == 0) return 0;
            if (t == 1) return 1;
            return math.pow(2, -10 * t) * math.sin((t * 10 - 0.75f) * (2 * math.PI) / 3) + 1;
        }
        public static float EaseInElastic(float t) {
            if (t == 0) return 0;
            if (t == 1) return 1;
            return -math.pow(2, 10 * t - 10) * math.sin((t * 10 - 10.75f) * (2 * math.PI) / 3);
        }
        public static float EaseInOutCirc(float t) {
            return t < 0.5f ? (1 - math.sqrt(1 - 4 * (t * t))) / 2 : (math.sqrt(-((2 * t) - 3) * ((2 * t) - 1)) + 1) / 2;
        }
        public static float EaseOutCirc(float t) {

            return math.sqrt(1 - (t = t - 1) * t);
        }
        public static float EaseInOutQuint(float t) {
            return t < 0.5f ? 16 * t * t * t * t * t : 1 + 16 * (--t) * t * t * t * t;
        }
        public static float EaseOutQuint(float t) {
            return 1 + (--t) * t * t * t * t;
        }
        public static float EaseInOutExpo(float t) {
            if (t == 0) return 0;
            if (t == 1) return 1;
            if ((t /= 0.5f) < 1) return 0.5f * math.pow(2, 10 * (t - 1));
            return 0.5f * (-math.pow(2, -10 * --t) + 2);
        }
        public static float EaseOutExpo(float t) {
            return t == 1 ? 1 : 1 - math.pow(2, -10 * t);
        }
        public static float EaseInExpo(float t) {
            return t == 0 ? 0 : math.pow(2, 10 * t - 10);
        }
        public static float EaseInOutSine(float t) {
            return -0.5f * (math.cos(math.PI * t) - 1);
        }
        public static float EaseInSine(float t) {
            return 1 - math.cos((t * math.PI) / 2);
        }
        public static float EaseOutCubic(float t) {
            return (--t) * t * t + 1;
        }
        public static float EaseInCubic(float t) {
            return t * t * t;
        }
        public static float EaseInOutQuad(float t) {
            return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
        }
        public static float EaseInQuart(float t) {
            return t * t * t * t;
        }
        public static float EaseOutBounce(float t) {
            if (t < 1 / 2.75f) {
                return 7.5625f * t * t;
            } else if (t < 2 / 2.75f) {
                return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
            } else if (t < 2.5 / 2.75) {
                return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
            } else {
                return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
            }
        }
        public static float EaseOutQuad(float t) {
            return t * (2 - t);
        }
        public static float EaseInCirc(float t) {
            return 1 - math.sqrt(1 - t * t);
        }
        public static float EaseInBounce(float t) {
            return 1 - EaseOutBounce(1 - t);
        }
        public static float EaseInQuad(float t) {
            return t * t;
        }
        public static float EaseInOutBounce(float t) {
            if (t < 0.5f) {
                return EaseInBounce(t * 2) * 0.5f;
            } else {
                return EaseOutBounce(t * 2 - 1) * 0.5f + 0.5f;
            }
        }
        public static float Linear(float t) {
            return t;
        }
        public static float EaseOutSine(float t) {
            return math.sin((t * math.PI) / 2);
        }
        public static float EaseInBack(float t) {
            return t * t * (2.70158f * t - 1.70158f);
        }
        public static float EaseOutBack(float t) {
            return 1 + (t -= 1) * t * (2.70158f * t + 1.70158f);
        }
        public static float EaseInOutBack(float t) {
            float s = 1.70158f * 1.525f;
            if ((t /= 0.5f) < 1) return 0.5f * (t * t * ((s + 1) * t - s));
            return 0.5f * ((t -= 2) * t * ((s + 1) * t + s) + 2);
        }
    }
}