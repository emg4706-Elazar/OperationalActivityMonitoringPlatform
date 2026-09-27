from collections import deque
import pandas as pd


class AnomalyDetector:

    def __init__(self,
                 window_size,
                 z_threshold,
                 critical_threshold
                 ):

        self.window_size = window_size
        self.z_threshold = z_threshold
        self.critical_threshold = critical_threshold
        self.windows: dict[str, deque[float]] = {}


    def get_window(self, source_id):
        if source_id not in self.windows:
            self.windows[source_id] = deque(
                maxlen=self.window_size)

        return self.windows[source_id]


    def process(self,source_id, value):
        current_window = self.get_window(source_id)

        if len(current_window) < self.window_size:
            current_window.append(value)
            return None

        window_series = pd.Series(
            list(current_window),
            dtype=float
        )

        mean = window_series.mean()
        standard_deviation = window_series.std()

        if standard_deviation == 0:
            current_window.append(value)
            return None

        z_score = (value - mean) / standard_deviation
        absolute_z_score = abs(z_score)

        if absolute_z_score >= self.critical_threshold:
            severity = "Critical"
        elif absolute_z_score >= self.z_threshold:
            severity = "Warning"
        else:
            severity = "Normal"

        current_window.append(value)

        if severity == "Normal":
            return None

        return {
            "mean": mean,
                "standard_deviation": standard_deviation,
                "z_score": z_score,
                "severity": severity
        }

