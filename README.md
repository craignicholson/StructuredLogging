# Structured Logging

Using C#, log4net, Filebeat, ELK (elasticsearch, logstash, kibana).  {.Net Environment}

Quick Review

1. Logs come from the apps in various formats.  Having a defined schema for logs is a good thing. We will do this with json.
1. Filebeat soaks up the logs and monitors other stuff on the server and send it to LogStash.
1. LogStash converts it all to a common format and sends it to ElasticSearch.
1. We use Kibana to make cool graphs and analyze what's in ElasticSearch.

Our goal is to create a structured log which is a collection of the same fields we can
use across all applications.  The goal is send all logs to one system so operations can
use these logs to monitor and troubleshoot applications.

## Setup for this Project

If you want to create your own project or add the same to an existing one below are the steps needed.

1. Add log4net using the NuGet Package Manager
1. Add Newtonsoft.Json using the NuGet Package Manager
1. Add code block to AssemblyInfo.cs file
1. Add a log4net.config file with a right click solution and add new item, general, config, and name file log4net.config
1. Click on log4net.config and set properties in the VS Window, to Copy to Output Directory = Always, or if newer.
1. Copy and Paste the xml data below into the log4.net.config file.
1. Add class file StucturedMessage.cs

We use Newtonsoft.Json to generate the json from our C# objects.  Having the json generated this way
is much safer then building strings which might contain characters we would have to strip out and replace.
Invalid characters will create invalid Json.

## Add assemnbly info for log4net

```csharp
// Manually Add of log4net by Craig Nicholson
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Log4net.config", Watch = true)]
```

- Add reference to Logger in program.cs

```csharp
private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
```

## Update log4net.config

Typical log4net.config file

```xml

<?xml version="1.0" encoding="utf-8" ?>
<log4net>
  <appender name="ConsoleAppender" type="log4net.Appender.ConsoleAppender">
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="%date %-5level: %message%newline" />
    </layout>
  </appender>
  <appender name="LogFileAppender" type="log4net.Appender.RollingFileAppender,log4net">
    <param name="File" value="log/OutageEventChangedNotification.log"/>
    <lockingModel type="log4net.Appender.FileAppender+MinimalLock,log4net" />
    <appendToFile value="true" />
    <rollingStyle value="Size" />
    <maxSizeRollBackups value="2" />
    <maximumFileSize value="1MB" />
    <staticLogFileName value="true" />
    <layout type="log4net.Layout.PatternLayout,log4net">
      <param name="ConversionPattern" value="%d [%t] %-5p %c %m%n"/>
    </layout>
  </appender>
  <root>
    <level value="ALL" />
    <appender-ref ref="ConsoleAppender" />
    <appender-ref ref="LogFileAppender" />
  </root>
</log4net>


```

The following log4net.config is setup to output JSON to the logs.

- Writes output to the console
- Writes output to the logs/ directory

There are more appenders we need to review and see what interesting things we can do with the other appenders.
https://logging.apache.org/log4net/release/manual/introduction.html#appenders

```xml
<?xml version="1.0" encoding="utf-8" ?>
<log4net>
  <appender name="ConsoleAppender" type="log4net.Appender.ConsoleAppender">
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="{&quot;EntryDate&quot;:&quot;%date{ISO8601}&quot;,&quot;Level&quot;:&quot;%-5level&quot;,&quot;EntityName&quot;:&quot;%property{EntityName}&quot;,&quot;Message&quot;:%message}%newline" />
    </layout>
  </appender>
  <appender name="LogFileAppender" type="log4net.Appender.RollingFileAppender,log4net">
    <param name="File" value="logs/OutageEventChangedNotification.log"/>
    <lockingModel type="log4net.Appender.FileAppender+MinimalLock,log4net" />
    <appendToFile value="true" />
    <rollingStyle value="Size" />
    <maxSizeRollBackups value="2" />
    <maximumFileSize value="1MB" />
    <staticLogFileName value="true" />
    <layout type="log4net.Layout.PatternLayout,log4net">
      <param name="ConversionPattern" value="{&quot;EntryDate&quot;:&quot;%date{ISO8601}&quot;,&quot;thread&quot;:&quot;[%thread]&quot;,&quot;Level&quot;:&quot;%-5level&quot;,&quot;EntityName&quot;:&quot;%property{EntityName}&quot;,&quot;Message&quot;:%message}%newline"/>
    </layout>
  </appender>
  <root>
    <level value="ALL" />
    <appender-ref ref="ConsoleAppender" />
    <appender-ref ref="LogFileAppender" />
  </root>
</log4net>
```

## Example of a JSON log output

```Json
{
    "EntryDate": "2018-01-17 11:39:13,644",
    "thread": "[WorkPool-Session#1:Connection(2b0af7b7-f141-4525-b3af-d982863aafd7,amqp://etss-appdev:5672)]",
    "Level": "INFO ",
    "EntityName": "ElectSolve",
    "Message": {
        "environmentVariables": {
            "machineName": "DESKTOP-L2HKL4V",
            "ipAddress": "fe80::8957:cd3f:cbeb:7ef9%6",
            "operatingSystem": "Microsoft Windows NT 6.2.9200.0",
            "userName": "SYSTEM",
            "userDomainName": "WORKGROUP",
            "totalMemory": 1076608,
            "workingSet": 53874688
        },
        "appName": "OutageEventChangedNotificationConsumerService.exe",
        "correlationId": "d488fd0d-5b0b-4ae1-ada0-859d9e038e1b",
        "methodName": "ListenForHeartbeat",
        "message": "Heartbeat Received!",
        "error": null,
        "stackTrace": null,
        "elapsedMilliseconds": 0,
        "localDateTime": "2018-01-17T11:39:13.6441902-05:00",
        "exceptionReceived": null,
        "object": null,
        "objectType": null
    }
}
```

## Pipleline to collect log files - Docker

### [Install Docker](https://docs.docker.com/docker-for-windows/install/)

Install Docker and download the ELK Stack

> docker pull sebp/elk
> docker images
> docker run -p 5601:5601 -p 9200:9200 -p 5044:5044 -it --name elk sebp/
> docker ps
> docker stop my_container

- [Install Filebeat](https://www.elastic.co/downloads/beats/filebeat)

1. Unzip the packatge you downloaded (filebeat-6.1.1-windows-x86_64.zip)
1. Run Powershell as Administrator on install-service-filebeat.ps1
1. Create a filebeat.yml or edit the existing file. TODO: Create folder of sample filebeat.yml files.

```yml
#=========================== Filebeat prospectors =============================

filebeat.prospectors:

# Each - is a prospector. Most options can be set at the prospector level, so
# you can use different prospectors for various configurations.
# Below are the prospector specific configurations.

- type: log

  # Change to true to enable this prospector configuration.
  enabled: true

  # Paths that should be crawled and fetched. Glob based paths.
  paths:
    - C:\Users\craig\source\repos\OutageEventChangedNotificationSolution\OutageEventChangedNotificationConsumerConsole\bin\Debug\log\*
  document_type: craigtest
  json.keys_under_root: true
  json.add_error_key: true

#============================= Filebeat modules ===============================

filebeat.config.modules:
  # Glob pattern for configuration loading
  path: ${path.config}/modules.d/*.yml

  # Set to true to enable config reloading
  reload.enabled: false

  # Period on which files under path should be checked for changes
  #reload.period: 10s

#==================== Elasticsearch template setting ==========================

setup.template.settings:
  index.number_of_shards: 3
  #index.codec: best_compression
  #_source.enabled: false

#============================== Kibana =====================================

# Starting with Beats version 6.0.0, the dashboards are loaded via the Kibana API.
# This requires a Kibana endpoint configuration.
setup.kibana:

  # Kibana Host
  # Scheme and port can be left out and will be set to the default (http and 5601)
  # In case you specify and additional path, the scheme is required: http://localhost:5601/path
  # IPv6 addresses should always be defined as: https://[2001:db8::1]:5601
  #host: "localhost:5601"

#================================ Outputs =====================================

# Configure what output to use when sending the data collected by the beat.
#-------------------------- Elasticsearch output ------------------------------
output.elasticsearch:
  # Array of hosts to connect to.
  hosts: ["localhost:9200"]

  # Optional protocol and basic auth credentials.
  #protocol: "https"
  #username: "elastic"
  #password: "changeme"

#----------------------------- Logstash output --------------------------------
#output.logstash:
  # The Logstash hosts
  #hosts: ["localhost:5044"]

  # Optional SSL. By default is off.
  # List of root certificates for HTTPS server verifications
  #ssl.certificate_authorities: ["/etc/pki/root/ca.pem"]

  # Certificate for SSL client authentication
  #ssl.certificate: "/etc/pki/client/cert.pem"

  # Client Certificate Key
  #ssl.key: "/etc/pki/client/cert.key"

```

### Install for Production on Windows Server

1. Download and install Filebeat

- Java http://www.oracle.com/technetwork/java/javase/downloads/jdk9-downloads-3848520.html
- ElasticSearch (http://localhost:9200/)
  --https://www.elastic.co/guide/en/elasticsearch/reference/current/windows.html
  --https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-6.1.1.msi
  --https://stackoverflow.com/questions/22924300/removing-data-from-elasticsearch
  -- curl -X DELETE 'http://localhost:9200/_all'
- LogStash on windows
- Kibana (http://localhost:5601)
- Filebeat (Runs local on the client)
 --https://www.elastic.co/downloads/beats/filebeat
 -- unzip/tar the package
 -- Run install-service-filebeat.pd1

Edit Filebeat yml


> cd C:\Users\craig\Downloads\filebeat-6.1.1-windows-x86_64\filebeat-6.1.1-windows-x86_64
> install-service-filebeat.pd1

Edit the filebeat .yml

> Start-Service filebeat
> Stop-Service filebeat
C:\ProgramData\filebeat\logs

PS C:\Users\craig\Downloads\filebeat-6.1.1-windows-x86_64\filebeat-6.1.1-windows-x86_64> history

  Id CommandLine
  -- -----------
   1 curl -XGET 'localhost:9600/_node/logging?pretty'
   2 cd C:\Users\craig\Downloads\filebeat-6.1.1-windows-x86_64\filebeat-6.1.1-windows-x86_64
   3 service filebeat start
   4 ls
   5 ./filebeat.exe start
   6 Start-Service filebeat
   7 history
   8 docker ps
   9 docker images
  10 docker ps
  11 docker stop elk
  12 docker ps
  13 docker images

PS C:\Users\craig\Downloads\filebeat-6.1.1-windows-x86_64\filebeat-6.1.1-windows-x86_64> history

  Id CommandLine
  -- -----------
   1 Set-ExecutionPolicy Unrestricted
   2 cd C:\Users\craig\Downloads\filebeat-6.1.1-windows-x86_64\filebeat-6.1.1-windows-x86_64
   3 .\install-service-filebeat.ps1
   4 Service-Start filebeat
   5 Start-Serivce filebeat
   6 Start-Service
   7 Start-Service filebeat
   8 Stop-Service filebeat
   9 Invoke-RestMethod -Method Delete "http://localhost:9200/filebeat-*"...
  10 Stop-Service filebeat
  11 Start-Service filebeat
  12 Stop-Service filebeat
  13 Start-Service filebeat
  14 Stop-Service filebeat
  15 Invoke-RestMethod DELETE 'http://localhost:9200/_all'
  16 Invoke-RestMethod DELETE "http://localhost:9200/_all"
  17 Start-Service filebeat
  18 Stop-Service filebeat
  19 Start-Service filebeat
  20 Stop-Service filebeat



### Filebeat

- https://discuss.elastic.co/t/need-configuration-to-send-json-string-to-elk-server-through-filebeat/72493/

## Additional Log Aggreation Tools

https://coralogix.com/integrations/coralogix-logstash-integration/
https://logentries.com/
https://www.loggly.com/docs/net-logs/
https://nxlog.co/documentation/nxlog-user-guide#what-nxlog-is-not - fowards logs to aggregation UI - like loggly
https://stackify.com/best-log-management-tools/
https://stackify.com/csharp-logging-best-practices/

## Referennces

https://blog.rapid7.com/2016/04/27/how-to-ensure-self-describing-log-data-using-log4net/
