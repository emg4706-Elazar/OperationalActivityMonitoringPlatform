import pandas as pd
import confluent_kafka
import os
import json
import time

filepath = os.getenv("DATA_FILE_PATH")
bootstrap_servers = os.getenv("KAFKA_BOOTSTRAP_SERVERS")
topic_name = os.getenv("KAFKA_TOPIC_NAME")
client_id = os.getenv("KAFKA_CLIENT_ID")
delay = float(os.getenv("PUBLISH_DELAY_SECONDS", "0.1"))

df = pd.read_csv(filepath, dtype=str)


# Convert to timestamp for sort
df["parsed_timestamp"] = pd.to_datetime(
    df["timestamp"],
    errors='coerce',
    utc=True
)

# Sort the values by parsed timestamp
df = df.sort_values(
    "parsed_timestamp",
    na_position="last"
)

# Delete the helper column
df = df.drop(columns="parsed_timestamp")

# configure the variables producer
conf = {
    'bootstrap.servers': bootstrap_servers,
    'client.id': client_id
}

producer = confluent_kafka.Producer(conf)

for row in df.itertuples(index=False, name=None):

    event = {
        column: None if pd.isna(value) else value
        for column, value in zip(df.columns, row)
    }

    producer.produce(
        topic=topic_name,
        key=event["source_id"],
        value=json.dumps(event)
    )

    producer.poll(0)
    time.sleep(delay)

remaining = producer.flush()

print(f"Published {len(df)} events.")
print(f"Unset events {remaining}.")
