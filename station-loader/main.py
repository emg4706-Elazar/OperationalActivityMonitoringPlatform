import os
import pandas as pd
import mysql.connector

host = os.getenv("MYSQL_HOST")
database_name = os.getenv("MYSQL_DATABASE")
username = os.getenv("MYSQL_USER")
password = os.getenv("MYSQL_PASSWORD")
port = int(os.getenv("MYSQL_PORT"))
filepath = os.getenv("DATA_FILE_PATH")

df = pd.read_csv(filepath)

records = list(
    df[["station_id", "name", "sector", "status"]]
    .itertuples(index=False, name = None)
)

mydb = mysql.connector.connect(
    host = host,
    port = port,
    user = username,
    password = password,
    database = database_name
)


cursor = mydb.cursor()

sql = """
INSERT INTO Stations 
(Id, Name, Sector, Status, CreatedAt) 
VALUES (%s, %s, %s, %s, UTC_TIMESTAMP()) 
ON DUPLICATE KEY UPDATE 
Name = VALUES(Name),
Sector = VALUES(Sector),
Status = VALUES(Status)
"""

try:
    cursor.executemany(sql, records)

    mydb.commit()

    print(len(records), "record processed.")
except Exception as e:
    print(e)
    mydb.rollback()
    raise
finally:
    cursor.close()
    mydb.close()