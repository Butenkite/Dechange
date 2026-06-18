using System;
using UnityEngine;

namespace Dechange
{
    /// <summary>
    /// Single source of truth for simulation time. All orbital positions are pure
    /// functions of JulianDate, making rewind and fast-forward free operations.
    /// </summary>
    public class SimClock : MonoBehaviour
    {
        public static SimClock Instance { get; private set; }

        // Seconds of simulation time per real second. Negative = rewind.
        [SerializeField] private double _rate = 86400.0; // 1 day / real second default
        [SerializeField] private bool _paused = false;

        // Seconds elapsed since J2000 (2000 Jan 1, 12:00 TT)
        private double _simSeconds;

        public double SimSeconds => _simSeconds;

        // Julian Date 2451545.0 = J2000 epoch
        public double JulianDate => 2451545.0 + _simSeconds / 86400.0;

        public double Rate
        {
            get => _rate;
            set => _rate = value;
        }

        public bool Paused => _paused;

        public event Action<double> OnTimeChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Update()
        {
            if (_paused) return;
            _simSeconds += _rate * Time.deltaTime;
            OnTimeChanged?.Invoke(_simSeconds);
        }

        public void Play() => _paused = false;
        public void Pause() => _paused = true;

        public void Stop()
        {
            _paused = true;
            _simSeconds = 0;
            OnTimeChanged?.Invoke(_simSeconds);
        }

        public void SetRate(double rate)
        {
            _rate = rate;
            _paused = false;
        }

        public void Scrub(double simSeconds)
        {
            _simSeconds = simSeconds;
            OnTimeChanged?.Invoke(_simSeconds);
        }
    }
}
