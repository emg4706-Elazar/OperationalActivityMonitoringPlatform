import math
import pandas as pd


class Validation:

    def validate(self, message: dict):
        if not isinstance(message, dict):
            return None

        event_id = message.get("event_id")
        source_id = message.get("source_id")
        timestamp = message.get("timestamp")
        value = message.get("value")


        if not isinstance(event_id, str) or not event_id.strip():
            return None

        if not isinstance(source_id, str) or not source_id.strip():
            return None

        parsed_timestamp = pd.to_datetime(
            timestamp,
            errors='coerce',
            utc=True
        )

        if pd.isna(parsed_timestamp):
            return None

        parsed_value = pd.to_numeric(
            value,
            errors='coerce'
        )

        if pd.isna(parsed_value):
            return None

        parsed_value = float(parsed_value)

        if not math.isfinite(parsed_value):
            return None

        return {
            "event_id": event_id,
            "source_id": source_id,
            "timestamp": timestamp,
            "value": parsed_value
        }

