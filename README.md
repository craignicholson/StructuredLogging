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

```bash
> docker pull sebp/elk
Using default tag: latest
latest: Pulling from sebp/elk
Digest: sha256:37c37158a55218af97d182b88ffdbba44fb2827060ca3b0082f7caf3ad77c762
Status: Image is up to date for sebp/elk:latest
```

You can view the images with this command.

```bash
> docker images
REPOSITORY          TAG                 IMAGE ID            CREATED             SIZE
sebp/elk            latest              b0dc4bffd033        3 weeks ago         1.07GB
```

Run the stack.  This will get you up and running locally.
up as long as you keep this terminal open.  TODO: show the way to run this ... and not see the output. headless.

You can run this command after you have installed filebeat.  Depending on your machine docker might be expensize to run.

```bash
> docker run -p 5601:5601 -p 9200:9200 -p 5044:5044 -it --name elk sebp/

 * Starting periodic command scheduler cron                                                                                                                                              [ OK ]
 * Starting Elasticsearch Server                                                                                                                                                         [ OK ]
waiting for Elasticsearch to be up (1/30)
waiting for Elasticsearch to be up (2/30)
waiting for Elasticsearch to be up (3/30)
waiting for Elasticsearch to be up (4/30)
waiting for Elasticsearch to be up (5/30)
Waiting for Elasticsearch cluster to respond (1/30)
logstash started.
 * Starting Kibana5                                                                                                                                                                      [ OK ]
==> /var/log/elasticsearch/elasticsearch.log <==
[2018-01-18T01:03:10,167][INFO ][o.e.d.DiscoveryModule    ] [FijewPo] using discovery type [zen]
[2018-01-18T01:03:10,532][INFO ][o.e.n.Node               ] initialized
[2018-01-18T01:03:10,533][INFO ][o.e.n.Node               ] [FijewPo] starting ...
[2018-01-18T01:03:10,629][INFO ][o.e.t.TransportService   ] [FijewPo] publish_address {172.17.0.2:9300}, bound_addresses {0.0.0.0:9300}
[2018-01-18T01:03:10,636][INFO ][o.e.b.BootstrapChecks    ] [FijewPo] bound or publishing to a non-loopback or non-link-local address, enforcing bootstrap checks
[2018-01-18T01:03:13,679][INFO ][o.e.c.s.MasterService    ] [FijewPo] zen-disco-elected-as-master ([0] nodes joined), reason: new_master {FijewPo}{FijewPoYTTC32sTUGYue-g}{iSi9kftzR7mL7its4nBFyw}{172.17.0.2}{172.17.0.2:9300}
[2018-01-18T01:03:13,683][INFO ][o.e.c.s.ClusterApplierService] [FijewPo] new_master {FijewPo}{FijewPoYTTC32sTUGYue-g}{iSi9kftzR7mL7its4nBFyw}{172.17.0.2}{172.17.0.2:9300}, reason: apply cluster state (from master [master {FijewPo}{FijewPoYTTC32sTUGYue-g}{iSi9kftzR7mL7its4nBFyw}{172.17.0.2}{172.17.0.2:9300} committed version [1] source [zen-disco-elected-as-master ([0] nodes joined)]])
[2018-01-18T01:03:13,693][INFO ][o.e.h.n.Netty4HttpServerTransport] [FijewPo] publish_address {172.17.0.2:9200}, bound_addresses {0.0.0.0:9200}
[2018-01-18T01:03:13,693][INFO ][o.e.n.Node               ] [FijewPo] started
[2018-01-18T01:03:13,713][INFO ][o.e.g.GatewayService     ] [FijewPo] recovered [0] indices into cluster_state

==> /var/log/logstash/logstash-plain.log <==

==> /var/log/kibana/kibana5.log <==
{"type":"log","@timestamp":"2018-01-18T01:03:19Z","tags":["status","plugin:kibana@6.1.1","info"],"pid":190,"state":"green","message":"Status changed from uninitialized to green - Ready","prevState":"uninitialized","prevMsg":"uninitialized"}
{"type":"log","@timestamp":"2018-01-18T01:03:19Z","tags":["status","plugin:elasticsearch@6.1.1","info"],"pid":190,"state":"yellow","message":"Status changed from uninitialized to yellow - Waiting for Elasticsearch","prevState":"uninitialized","prevMsg":"uninitialized"}
{"type":"log","@timestamp":"2018-01-18T01:03:19Z","tags":["status","plugin:console@6.1.1","info"],"pid":190,"state":"green","message":"Status changed from uninitialized to green - Ready","prevState":"uninitialized","prevMsg":"uninitialized"}
{"type":"log","@timestamp":"2018-01-18T01:03:19Z","tags":["status","plugin:metrics@6.1.1","info"],"pid":190,"state":"green","message":"Status changed from uninitialized to green - Ready","prevState":"uninitialized","prevMsg":"uninitialized"}
{"type":"log","@timestamp":"2018-01-18T01:03:19Z","tags":["status","plugin:timelion@6.1.1","info"],"pid":190,"state":"green","message":"Status changed from uninitialized to green - Ready","prevState":"uninitialized","prevMsg":"uninitialized"}
{"type":"log","@timestamp":"2018-01-18T01:03:19Z","tags":["listening","info"],"pid":190,"message":"Server running at http://0.0.0.0:5601"}
{"type":"log","@timestamp":"2018-01-18T01:03:19Z","tags":["status","plugin:elasticsearch@6.1.1","info"],"pid":190,"state":"green","message":"Status changed from yellow to green - Ready","prevState":"yellow","prevMsg":"Waiting for Elasticsearch"}

==> /var/log/logstash/logstash-plain.log <==
[2018-01-18T01:03:31,557][INFO ][logstash.modules.scaffold] Initializing module {:module_name=>"netflow", :directory=>"/opt/logstash/modules/netflow/configuration"}
[2018-01-18T01:03:31,567][INFO ][logstash.modules.scaffold] Initializing module {:module_name=>"fb_apache", :directory=>"/opt/logstash/modules/fb_apache/configuration"}
[2018-01-18T01:03:31,626][INFO ][logstash.setting.writabledirectory] Creating directory {:setting=>"path.queue", :path=>"/opt/logstash/data/queue"}
[2018-01-18T01:03:31,629][INFO ][logstash.setting.writabledirectory] Creating directory {:setting=>"path.dead_letter_queue", :path=>"/opt/logstash/data/dead_letter_queue"}
[2018-01-18T01:03:31,921][WARN ][logstash.config.source.multilocal] Ignoring the 'pipelines.yml' file because modules or command line options are specified
[2018-01-18T01:03:31,951][INFO ][logstash.agent           ] No persistent UUID file found. Generating new UUID {:uuid=>"842b7c4f-04e3-45c8-84b5-605f3c14c9d0", :path=>"/opt/logstash/data/uuid"}
[2018-01-18T01:03:32,420][INFO ][logstash.runner          ] Starting Logstash {"logstash.version"=>"6.1.1"}
[2018-01-18T01:03:32,776][INFO ][logstash.agent           ] Successfully started Logstash API endpoint {:port=>9600}
[2018-01-18T01:03:35,705][WARN ][logstash.outputs.elasticsearch] You are using a deprecated config setting "document_type" set in elasticsearch. Deprecated settings will continue to work, but are scheduled for removal from logstash in the future. Document types are being deprecated in Elasticsearch 6.0, and removed entirely in 7.0. You should avoid this feature If you have any questions about this, please visit the #logstash channel on freenode irc. {:name=>"document_type", :plugin=><LogStash::Outputs::ElasticSearch hosts=>[//localhost], manage_template=>false, index=>"%{[@metadata][beat]}-%{+YYYY.MM.dd}", document_type=>"%{[@metadata][type]}", id=>"225ede8330c8baf0f6dca5ad1692f2bdbe28641f42e6c0a0a5af6323ae7b3954", enable_metric=>true, codec=><LogStash::Codecs::Plain id=>"plain_2be8607a-a135-4d96-a03a-c11542b0d4fe", enable_metric=>true, charset=>"UTF-8">, workers=>1, template_name=>"logstash", template_overwrite=>false, doc_as_upsert=>false, script_type=>"inline", script_lang=>"painless", script_var_name=>"event", scripted_upsert=>false, retry_initial_interval=>2, retry_max_interval=>64, retry_on_conflict=>1, action=>"index", ssl_certificate_verification=>true, sniffing=>false, sniffing_delay=>5, timeout=>60, pool_max=>1000, pool_max_per_route=>100, resurrect_delay=>5, validate_after_inactivity=>10000, http_compression=>false>}
[2018-01-18T01:03:36,155][INFO ][logstash.outputs.elasticsearch] Elasticsearch pool URLs updated {:changes=>{:removed=>[], :added=>[http://localhost:9200/]}}
[2018-01-18T01:03:36,165][INFO ][logstash.outputs.elasticsearch] Running health check to see if an Elasticsearch connection is working {:healthcheck_url=>http://localhost:9200/, :path=>"/"}
[2018-01-18T01:03:36,361][WARN ][logstash.outputs.elasticsearch] Restored connection to ES instance {:url=>"http://localhost:9200/"}
[2018-01-18T01:03:36,490][INFO ][logstash.outputs.elasticsearch] ES Output version determined {:es_version=>nil}
[2018-01-18T01:03:36,492][WARN ][logstash.outputs.elasticsearch] Detected a 6.x and above cluster: the `type` event field won't be used to determine the document _type {:es_version=>6}
[2018-01-18T01:03:36,520][INFO ][logstash.outputs.elasticsearch] New Elasticsearch output {:class=>"LogStash::Outputs::ElasticSearch", :hosts=>["//localhost"]}
[2018-01-18T01:03:36,718][INFO ][logstash.pipeline        ] Starting pipeline {:pipeline_id=>"main", "pipeline.workers"=>2, "pipeline.batch.size"=>125, "pipeline.batch.delay"=>5, "pipeline.max_inflight"=>250, :thread=>"#<Thread:0x5b9a96c2 run>"}
[2018-01-18T01:03:37,099][INFO ][logstash.inputs.beats    ] Beats inputs: Starting input listener {:address=>"0.0.0.0:5044"}
[2018-01-18T01:03:37,165][INFO ][logstash.pipeline        ] Pipeline started {"pipeline.id"=>"main"}
[2018-01-18T01:03:37,288][INFO ][logstash.agent           ] Pipelines running {:count=>1, :pipelines=>["main"]}
[2018-01-18T01:03:37,370][INFO ][org.logstash.beats.Server] Starting server on port: 5044
```

Now you should be able to reach kibaba: http://localhost:5601/app/kibana#/home?_g=()

You can click the Discover on the RIGHT side of the web page and click Check for New data in the middle of the web page.

Review what processes are running in docker.  You can use this to get the container's name.

```bash
> docker ps
```

Stop you container - For later.  Remove the container.

```bash
> docker stop elk
> docker rm containerId
```

Save the data for elastic search

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

> PS C:\WINDOWS\system32> Start-Service filebeat

You can review the filebeat log to see if it started successfully.

C:\ProgramData\filebeat\logs

Here is what success looks like...

Here is what failure looks like...

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