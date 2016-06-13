# myenvironment
IoT showcase project with the purpose of logging and monitoring environmental data from several sensors. 

myEnvironment started with a Dht22 sensor and the highly appreciated library https://github.com/porrey/dht by Daniel Porrey that jump started the development process with some nice temperature and humidity data on a Raspberry Pi 2.
Capturing environmental data is one thing, but using the data is another issue. To make the data available for displaying, storing and further processing needs some additional pieces to add to our puzzle:
- Storage local and globally: 
  SQLite on client devices (IoT and other), Azure BLOB storage for cloud persistence, and (future work) Azure stream analytics for high capacity/real-time data processing.
- Monitoring:
  To display the captured data on an attached monitor or a client app we implement some nice charting on an UWP app.
- Analyzing:
  Producing alerts for critical conditions like mold inducing values.
- Connecting:
  Alljoyn is used to connect myEnvironment devices to the monitoring app with ease and comfort.
