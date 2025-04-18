namespace Hexa.NET.ImGui.Widgets
{
    using Hexa.NET.Utilities;

    public struct AnimationState
    {
        public float Time;
        public float Speed;
        public float Duration;
        public AnimationType Type;
    }

    public enum AnimationType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        Bounce,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic
    }

    public static class AnimationManager
    {
        private static UnsafeDictionary<uint, AnimationState> states = new();
        private static UnsafeQueue<uint> removeQueue = new();

        public static bool AnyActive => states.Count != 0;

        public static int ActiveCount => states.Count;

        public static void Clear()
        {
            states.Clear();
            removeQueue.Clear();
        }

        public static void Release()
        {
            states.Release();
            removeQueue.Release();
        }

        public static void ResetAnimation(uint id)
        {
            if (states.TryGetValue(id, out AnimationState value))
            {
                value.Time = 0;
                states[id] = value;
            }
        }

        public static void StopAnimation(uint id)
        {
            states.Remove(id);
        }

        public static void AddAnimation(uint id, float duration, float speed, AnimationType type)
        {
            states[id] = new AnimationState
            {
                Time = 0,
                Speed = speed,
                Duration = duration,
                Type = type
            };
        }

        public static void AddAnimation(uint id, float time, float duration, float speed, AnimationType type)
        {
            states[id] = new AnimationState
            {
                Time = time,
                Speed = speed,
                Duration = duration,
                Type = type
            };
        }

        public static void RemoveAnimation(uint id)
        {
            if (states.ContainsKey(id))
            {
                states.Remove(id);
            }
        }

        public static void Tick()
        {
            var deltaTime = ImGui.GetIO().DeltaTime;
            foreach (var state in states)
            {
                var value = state.Value;
                value.Time += deltaTime * value.Speed;
                states[state.Key] = value;
                if (value.Time >= value.Duration)
                {
                    removeQueue.Enqueue(state.Key);
                }
            }

            while (removeQueue.TryDequeue(out var key))
            {
                states.Remove(key);
            }
        }

        public static float GetAnimationValue(uint id)
        {
            if (!states.TryGetValue(id, out AnimationState value)) return -1;

            var state = value;
            float t = state.Time / state.Duration;

            switch (state.Type)
            {
                case AnimationType.Linear:
                    return t;

                case AnimationType.EaseIn:
                    return t * t;

                case AnimationType.EaseOut:
                    return t * (2 - t);

                case AnimationType.EaseInOut:
                    return t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;

                case AnimationType.Bounce:
                    return BounceEaseOut(t);

                case AnimationType.EaseInQuad:
                    return t * t;

                case AnimationType.EaseOutQuad:
                    return t * (2 - t);

                case AnimationType.EaseInOutQuad:
                    return t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;

                case AnimationType.EaseInCubic:
                    return t * t * t;

                case AnimationType.EaseOutCubic:
                    t--;
                    return t * t * t + 1;

                case AnimationType.EaseInOutCubic:
                    return t < 0.5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;

                default:
                    return t;
            }
        }

        private static float BounceEaseOut(float t)
        {
            if (t < 1 / 2.75f)
            {
                return 7.5625f * t * t;
            }
            else if (t < 2 / 2.75f)
            {
                t -= 1.5f / 2.75f;
                return 7.5625f * t * t + 0.75f;
            }
            else if (t < 2.5f / 2.75f)
            {
                t -= 2.25f / 2.75f;
                return 7.5625f * t * t + 0.9375f;
            }
            else
            {
                t -= 2.625f / 2.75f;
                return 7.5625f * t * t + 0.984375f;
            }
        }
    }
}