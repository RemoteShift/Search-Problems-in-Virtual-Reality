using UnityEngine;
using UnityEngine.Events;

namespace Search.Utils
{
    public enum TimeFormat
    {
        TwelveHour,
        TwentyFourHour
    };

    public class Clock : Singleton<Clock>
    {
        public float TimeScale = 1f;

        public bool IsPaused = false;

        public int TicksPerSecond = 60;
        public int TicksPerHour = 2500;

        public TimeFormat TimeFormat;

        [Header("Events")] public UnityEvent<float> TimeScaleChanged;

        public UnityEvent Paused;
        public UnityEvent<int> HourChanged;
        public UnityEvent<int> DayChanged;
        private int _hoursPerDay = 24;
        private int _minutesPerHour = 60;
        private float _tickInterval;

        private float _ticksPerMinute;
        // A day has 60,000 ticks making day cycles easy to segment

        private float _tickTimer;
        public float DeltaTime => Time.deltaTime * TimeScale;
        public float FixedDeltaTime => Time.fixedDeltaTime * TimeScale;

        public int TickCount { get; private set; }
        public int CurrentHour => ((TickCount / TicksPerHour) % _hoursPerDay);
        public int CurrentMinute => Mathf.FloorToInt((TickCount / _ticksPerMinute) % _minutesPerHour);
        public int CurrentDay => (TickCount / TicksPerHour / _hoursPerDay) + 1;

        protected override void Awake()
        {
            base.Awake();
            TimeFormat = TimeFormat.TwentyFourHour;
            _tickInterval = 1f / (float)TicksPerSecond;
            _ticksPerMinute = (float)TicksPerHour / 60f;
        }

        private void FixedUpdate()
        {
            if (!IsPaused)
            {
                _tickTimer += FixedDeltaTime;

                while (_tickTimer >= _tickInterval)
                {
                    _tickTimer -= _tickInterval;
                    Tick();
                }
            }
        }

        public void SetTimeScale(float timeScale)
        {
            if (timeScale < 0)
            {
                Debug.LogError("Can't assign the time scale a negative number!");
                return;
            }

            TimeScale = timeScale;

            if (IsPaused)
            {
                IsPaused = false;
            }

            OnTimeScaleChanged(timeScale);
        }

        public void Pause()
        {
            TimeScale = 0f;
            IsPaused = true;
        }

        public string GetTimeFormatted()
        {
            if (TimeFormat == TimeFormat.TwentyFourHour)
            {
                return $"{CurrentHour:00}:{CurrentMinute:00}, Day {CurrentDay}";
            }
            else
            {
                int quotient = CurrentHour / 12;
                string amOrPm = quotient == 0 ? "AM" : "PM";
                int currentHour = (CurrentHour % 12 == 0) && (amOrPm.Equals("PM")) ? 12 : CurrentHour % 12;
                return $"{currentHour:00}:{CurrentMinute:00} {amOrPm}, Day {CurrentDay}";
            }
        }

        private void Tick()
        {
            TickCount++;

            if (TickCount % TicksPerHour == 0)
            {
                OnHourChanged(CurrentHour);
            }

            if (CurrentHour % _hoursPerDay == 0)
            {
                OnDayChanged(CurrentDay);
            }
        }

        private void OnTimeScaleChanged(float newScale)
        {
            if (newScale == 0)
            {
                // Should've paused
                OnPause();
                return;
            }

            if (IsPaused)
            {
                IsPaused = false;
            }

            TimeScaleChanged?.Invoke(newScale);
        }

        private void OnPause()
        {
            Paused?.Invoke();
        }

        private void OnHourChanged(int newHour)
        {
            HourChanged?.Invoke(newHour);
        }

        private void OnDayChanged(int newDay)
        {
            DayChanged?.Invoke(newDay);
        }

        public float TicksToHours(int ticks) => ticks / (float)TicksPerHour;

        public int HoursToTicks(float hours) => Mathf.RoundToInt(hours * TicksPerHour);
    }
}