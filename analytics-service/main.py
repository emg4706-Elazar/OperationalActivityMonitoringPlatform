import os
import datetime

from anomaly_detector import AnomalyDetector
from validation import Validation
from confluent_kafka import Producer, Consumer, KafkaError, KafkaException
import json
import sys

# Read all environments variables
KAFKA_BOOTSTRAP_SERVERS = os.environ["KAFKA_BOOTSTRAP_SERVERS"]
KAFKA_INPUT_TOPIC = os.environ["KAFKA_INPUT_TOPIC"]
KAFKA_OUTPUT_TOPIC = os.environ["KAFKA_OUTPUT_TOPIC"]
KAFKA_GROUP_ID = os.environ["KAFKA_GROUP_ID"]
KAFKA_CLIENT_ID = os.environ["KAFKA_CLIENT_ID"]

WINDOW_SIZE = int(os.environ["WINDOW_SIZE"])
Z_THRESHOLD = float(os.environ["Z_THRESHOLD"])
CRITICAL_THRESHOLD = float(os.environ["CRITICAL_THRESHOLD"])


# Validate the business values
if WINDOW_SIZE < 2:
    raise ValueError("WINDOW_SIZE must be at least 2")

if Z_THRESHOLD <= 0:
    raise ValueError("Z_THRESHOLD must be positive")

if CRITICAL_THRESHOLD < Z_THRESHOLD:
    raise ValueError(
        "CRITICAL_THRESHOLD must be greater than "
        "or equal to Z_THRESHOLD"
    )


# Generate the validator object of messages
validator = Validation()

# Generate the anomaly detector object
detector = AnomalyDetector(
        WINDOW_SIZE,
       Z_THRESHOLD,
       CRITICAL_THRESHOLD
)

# Generate the producer
producer_config = {'bootstrap.servers': KAFKA_BOOTSTRAP_SERVERS,
                   'client.id': KAFKA_CLIENT_ID}

producer = Producer(producer_config)

# Generate the consumer
consumer_config = {'bootstrap.servers': KAFKA_BOOTSTRAP_SERVERS,
                   'group.id': KAFKA_GROUP_ID,
                   'client.id': KAFKA_CLIENT_ID,
                   'enable.auto.commit': False,
                   'auto.offset.reset': 'earliest'}

consumer = Consumer(consumer_config)



# Process Loop
try:
    consumer.subscribe([KAFKA_INPUT_TOPIC])

    while True:

        msg = consumer.poll(timeout=0.1)
        if msg is None: continue

        if msg.error():
            if msg.error().code() == KafkaError._PARTITION_EOF:
                sys.stderr.write(
                    f"{msg.topic()} [{msg.partition()}] "
                    f"reached offset {msg.offset()}\n"
                )
                continue
            raise KafkaException(msg.error())


        else:
            try:
                raw_value = msg.value()
                if raw_value is None:
                    raise ValueError("Kafka message value is null")

                json_message = raw_value.decode("utf-8")
                reading = json.loads(json_message)

            except (UnicodeDecodeError,
                json.JSONDecodeError,
                ValueError) as error:
                print(f"Invalid kafka message: {error}")

                consumer.commit(
                    message=msg,
                    asynchronous=False
                )
                continue

            valid_reading = validator.validate(reading)

            if valid_reading is None:
                consumer.commit(
                    message=msg,
                    asynchronous=False
                )
                continue

            source_id = valid_reading.get("source_id")
            value = valid_reading.get("value")

            anomaly = detector.process(source_id, value)

            if anomaly is None:
                consumer.commit(
                    message=msg,
                    asynchronous=False
                )
                continue

            anomaly.update(valid_reading)
            anomaly["detected_at"] = (datetime.datetime
                                      .now(datetime.timezone.utc)
                                      .isoformat()
                                      .replace("+00:00", "Z"))

            producer.produce(
                topic=KAFKA_OUTPUT_TOPIC,
                value=json.dumps(anomaly),
                key=anomaly.get("event_id"))

            remaining = producer.flush(timeout=10)

            if remaining > 0:
                raise TimeoutError("Anomaly delivery timed out.")

            consumer.commit(
                message=msg,
                asynchronous=False
            )

finally:
    producer.flush(timeout=10)
    consumer.close()




