


### Data Dicovery Finding
- ישנם 8 תחנות
- 2092 אירועים
- 12 אירועים אינם תקינים
- לא נמצאו כפילויות בשורות תקינות
- התחנות נמצאו שוות בין שני הקבצים
- ה producer אינו מסיר או מתקן שורות לא תקינות

### Station Loader
- טוען מידע מקובץ .csv
- ושולח ל mysql
- טעינה חוזרת לא צריכה ליצור כפילויות



### MySql
-station loader טבלת תחנות
- טבלת חריגות מ persistence-consumer
- טבלת יומן התראות מקבל מ alert-worker


### Api
- מאפשר לבצע שאילתות ל MySql
- מאפשר לבצע שאילתות ל mongo


### Producer
-  טוען מידע גולמי מקובץ activity-readings.csv
- מגדיר סוגי נתונים
- ממיין לפי זמן
- שולח את המידע ל kafka
- אינו מבצע בדיקות תקינות ואני משנה את הערכים


### Kafka
- מקבלת את שורות הקובץ הגולמי מה producer
- מקבל מ analytics_service את תוצאות זיהוי החריגות


### Raw consumer
- צורך מ kafka את activity_readings
- עושה וולידציה על  activity_readings
- שומר את התוצאה ב mongo

###  Analytics_Service
- צורך מ kafka את השורות הגולמיות של activity_readings
- מבצע וולידציה
- מבצע זיהוי חריגות על הנתונים
- מפרסם ב anomalies kafka
- מחזיק חלון נפרד לכל תחנה



### Persistence Consumer
- צורך מ kafka את תוצאות זיהוי השגיאות
- שומר ב mysql
- שולח ל elasticsearch עם המזהה שנוצר ב mysql
- שולח ל rabit חריגות קריטיות


### Elasticsearch
- שומר את תוצאות זיהוי החריגות שהגיעו מ persistence-consumer


### Alert Worker
- שולף מה rabit ושולח ל mysql


### Kibana
- מאפשר dashbord אל elasticsearch וכן ביצוע אנליזות




## Messages Interface Defination
### activity readings
- fields: event_id , source_id, timestamp, value
- type : str
- not Nan
- Produce by Producer
- Consume by Raw consumer
- Raw consumer send to mongoDB
- Analytics-service consume from kafka
- Statistics-service Consume from mongo



### anomaly
{
    "event_id": str,
    "source_id": str,
    "timestamp": str,
    "value": double,
    "mean": double,
    "standard_deviation": double,
    "z_score": double,
    "severity": str,
    "detected_at": str
}


### התראה
{
    "anomaly_id" : str,
    "source_id": str,
    "severity": str,
    "detected_at": datetime
}



OperationalActivityMonitoring/
├── data/
├── notebooks/
├── station-loader/
├── producer/
├── raw-consumer/
├── analytics-service/
├── persistence-consumer/
├── alert-worker/
├── statistics-service/
├── api/
├── database/
├── docker-compose.yml
├── README.md
└── .gitignore