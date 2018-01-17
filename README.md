# Structured Logging

Using C#, log4net, Filebeat, ELK (elasticsearch, logstash, kibana).  The windows way.  :-(

Quick Review

1. Logs come from the apps in various formats.
1. Filebeat soaks up the logs and monitors other stuff on the server and send it to LogStash
1. LogStash converts it all to a common format and sends it to ElasticSearch.
1. We use Kibana to make cool graphs and analyze what's in ElasticSearch.

Our goal is to create a structured log which is a collection of the same fields we can
use across all applications.  The goal is send all logs to one system so operations can
use these logs to monitor and troubleshoot applications.

## Setup Project

- Add log4net using the NuGet Package Manager
- Newtonsoft.Json
- Add code block to AssemblyInfo.cs file
- right click solution and add new item, general, config, and name file log4net.config
- click on log4net.config and set properties in the VS Window, to Copy to Output Directory = Always, or if newer.

We use Newtonsoft.Json to generate the json from our C# objects.  Having the json generated this way 
is much safer then building strings which might contain characters we would have to strip out and replace.
Invalid characters will create invalid Json.

## Add assemnbly info for log4net

```csharp

// Manually Add of log4net by Craig Nicholson
// [assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config")]
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

This log4net.config is setup to output JSON to the logs.

- Write output to the console
- Write output to the logs/ directory

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

## Log Aggreations

### Install for Production

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

```yml
###################### Filebeat Configuration Example #########################

# This file is an example configuration file highlighting only the most common
# options. The filebeat.reference.yml file from the same directory contains all the
# supported options with more comments. You can use it as a reference.
#
# You can find the full configuration reference here:
# https://www.elastic.co/guide/en/beats/filebeat/index.html

# For more available modules and options, please see the filebeat.reference.yml sample
# configuration file.

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
    #- /var/log/*.log
    - C:\Users\craig\source\repos\OutageEventChangedNotificationSolution\OutageEventChangedNotificationConsumerConsole\bin\Debug\log\*
  document_type: craigtest
  json.keys_under_root: true
  json.add_error_key: true

  # Exclude lines. A list of regular expressions to match. It drops the lines that are
  # matching any regular expression from the list.
  #exclude_lines: ['^DBG']

  # Include lines. A list of regular expressions to match. It exports the lines that are
  # matching any regular expression from the list.
  #include_lines: ['^ERR', '^WARN']

  # Exclude files. A list of regular expressions to match. Filebeat drops the files that
  # are matching any regular expression from the list. By default, no files are dropped.
  #exclude_files: ['.gz$']

  # Optional additional fields. These fields can be freely picked
  # to add additional information to the crawled log files for filtering
  #fields:
  #  level: debug
  #  review: 1

  ### Multiline options

  # Mutiline can be used for log messages spanning multiple lines. This is common
  # for Java Stack Traces or C-Line Continuation

  # The regexp Pattern that has to be matched. The example pattern matches all lines starting with [
  #multiline.pattern: ^\[

  # Defines if the pattern set under pattern should be negated or not. Default is false.
  #multiline.negate: false

  # Match can be set to "after" or "before". It is used to define if lines should be append to a pattern
  # that was (not) matched before or after or as long as a pattern is not matched based on negate.
  # Note: After is the equivalent to previous and before is the equivalent to to next in Logstash
  #multiline.match: after


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

#================================ General =====================================

# The name of the shipper that publishes the network data. It can be used to group
# all the transactions sent by a single shipper in the web interface.
#name:

# The tags of the shipper are included in their own field with each
# transaction published.
#tags: ["service-X", "web-tier"]

# Optional fields that you can specify to add additional information to the
# output.
#fields:
#  env: staging


#============================== Dashboards =====================================
# These settings control loading the sample dashboards to the Kibana index. Loading
# the dashboards is disabled by default and can be enabled either by setting the
# options here, or by using the `-setup` CLI flag or the `setup` command.
#setup.dashboards.enabled: false

# The URL from where to download the dashboards archive. By default this URL
# has a value which is computed based on the Beat name and version. For released
# versions, this URL points to the dashboard archive on the artifacts.elastic.co
# website.
#setup.dashboards.url:

#============================== Kibana =====================================

# Starting with Beats version 6.0.0, the dashboards are loaded via the Kibana API.
# This requires a Kibana endpoint configuration.
setup.kibana:

  # Kibana Host
  # Scheme and port can be left out and will be set to the default (http and 5601)
  # In case you specify and additional path, the scheme is required: http://localhost:5601/path
  # IPv6 addresses should always be defined as: https://[2001:db8::1]:5601
  #host: "localhost:5601"

#============================= Elastic Cloud ==================================

# These settings simplify using filebeat with the Elastic Cloud (https://cloud.elastic.co/).

# The cloud.id setting overwrites the `output.elasticsearch.hosts` and
# `setup.kibana.host` options.
# You can find the `cloud.id` in the Elastic Cloud web UI.
#cloud.id:

# The cloud.auth setting overwrites the `output.elasticsearch.username` and
# `output.elasticsearch.password` settings. The format is `<user>:<pass>`.
#cloud.auth:

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

#================================ Logging =====================================

# Sets log level. The default log level is info.
# Available log levels are: critical, error, warning, info, debug
#logging.level: debug

# At debug level, you can selectively enable logging only for some components.
# To enable all selectors use ["*"]. Examples of other selectors are "beat",
# "publish", "service".
#logging.selectors: ["*"]

```

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

### Testing Locally

- Install docker

> docker pull sebp/elk
> docker images
> docker run -p 5601:5601 -p 9200:9200 -p 5044:5044 -it --name elk sebp/
> docker ps
> docker stop my_container

### Filebeat

- https://discuss.elastic.co/t/need-configuration-to-send-json-string-to-elk-server-through-filebeat/72493/

## Additional Log Aggreation Tools

https://coralogix.com/integrations/coralogix-logstash-integration/
https://logentries.com/
https://www.loggly.com/docs/net-logs/
https://nxlog.co/documentation/nxlog-user-guide#what-nxlog-is-not - fowards logs to aggregation UI - like loggly
https://stackify.com/best-log-management-tools/
https://stackify.com/csharp-logging-best-practices/
