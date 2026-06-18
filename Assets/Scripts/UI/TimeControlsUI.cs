using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dechange
{
    /// <summary>
    /// Binds the time-controls panel to SimClock.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class TimeControlsUI : MonoBehaviour
    {
        private static readonly double[] Rates =
        {
            -86400.0 * 365,  // -1 yr/s
            -86400.0 * 30,   // -1 mo/s
            -86400.0,        // -1 day/s
             3600.0,         // +1 hr/s
             86400.0,        // +1 day/s  ← default (index 4)
             86400.0 * 7,    // +1 wk/s
             86400.0 * 30,   // +1 mo/s
             86400.0 * 365,  // +1 yr/s
             86400.0 * 3650, // +10 yr/s
        };

        private static readonly string[] RateLabels =
        {
            "−1 yr/s", "−1 mo/s", "−1 day/s",
            "+1 hr/s", "+1 day/s", "+1 wk/s",
            "+1 mo/s", "+1 yr/s", "+10 yr/s",
        };

        private const int DefaultRateIndex = 4;

        private int _rateIndex = DefaultRateIndex;
        private Label _dateLabel;
        private Label _rateLabel;

        void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;

            _dateLabel = root.Q<Label>("lbl-date");
            _rateLabel = root.Q<Label>("lbl-rate");

            Bind(root, "btn-slower", () => StepRate(-1));
            Bind(root, "btn-pause",  SimClock.Instance.Pause);
            Bind(root, "btn-play",   SimClock.Instance.Play);
            Bind(root, "btn-faster", () => StepRate(+1));
            Bind(root, "btn-scale",  ScaleService.Instance.ToggleMode);

            SimClock.Instance.OnTimeChanged += _ => RefreshDate();
            RefreshDate();
            RefreshRate();

            // Apply default rate
            SimClock.Instance.SetRate(Rates[_rateIndex]);
        }

        private static void Bind(VisualElement root, string name, Action action)
        {
            var btn = root.Q<Button>(name);
            if (btn != null) btn.clicked += action;
            else Debug.LogWarning($"[TimeControlsUI] Button '{name}' not found in UXML.");
        }

        private void StepRate(int delta)
        {
            _rateIndex = Mathf.Clamp(_rateIndex + delta, 0, Rates.Length - 1);
            SimClock.Instance.SetRate(Rates[_rateIndex]);
            RefreshRate();
        }

        private void RefreshDate()
        {
            if (_dateLabel == null) return;
            _dateLabel.text = JulianToString(SimClock.Instance.JulianDate);
        }

        private void RefreshRate()
        {
            if (_rateLabel == null) return;
            _rateLabel.text = RateLabels[_rateIndex];
        }

        // Standard Julian Date → Gregorian calendar (proleptic)
        private static string JulianToString(double jd)
        {
            int l = (int)jd + 68569;
            int n = 4 * l / 146097;
            l -= (146097 * n + 3) / 4;
            int i = 4000 * (l + 1) / 1461001;
            l -= 1461 * i / 4 - 31;
            int j = 80 * l / 2447;
            int day = l - 2447 * j / 80;
            l = j / 11;
            int month = j + 2 - 12 * l;
            int year = 100 * (n - 49) + i + l;

            return $"{year:D4} {MonthName(month)} {day:D2}";
        }

        private static string MonthName(int m) => m switch
        {
            1 => "Jan", 2 => "Feb", 3 => "Mar", 4 => "Apr",
            5 => "May", 6 => "Jun", 7 => "Jul", 8 => "Aug",
            9 => "Sep", 10 => "Oct", 11 => "Nov", _ => "Dec"
        };
    }
}
